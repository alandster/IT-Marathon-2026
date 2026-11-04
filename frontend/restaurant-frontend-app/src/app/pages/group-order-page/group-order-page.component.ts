import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { Subscription, interval } from 'rxjs';
import { ParticipantCardComponent } from '@app/components/group-orders/participant-card/participant-card.component';
import { ModalComponent } from '@app/shared/custom-components/modal/modal.component';
import { GroupOrdersService } from '@app/services/group-orders/group-orders.service';
import { ToastService } from '@app/services/toast/toast.service';
import { GuestInvite } from '@app/models/reservations/guest-invite.model';
import { Participant } from '@app/models/group-orders/participant.model';
import { GroupStatusResponse } from '@app/models/group-orders/group-status.model';
import { consolidatePreOrder } from '@app/shared/helpers/consolidate-pre-order';

@Component({
  selector: 'app-group-order-page',
  imports: [CommonModule, CurrencyPipe, DatePipe, ParticipantCardComponent, ModalComponent],
  templateUrl: './group-order-page.component.html',
  styleUrl: './group-order-page.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class GroupOrderPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly groupOrdersService = inject(GroupOrdersService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly reservationId = signal<string>('');
  readonly invite = signal<GuestInvite | null>(null);
  readonly participants = signal<Participant[]>([]);
  readonly ownerSessionId = signal<string>('');
  readonly groupStatus = signal<GroupStatusResponse | null>(null);

  readonly isLoading = signal<boolean>(true);
  readonly errorMessage = signal<string | null>(null);
  readonly isSubmitting = signal<boolean>(false);
  readonly isSubmitModalOpen = signal<boolean>(false);

  readonly guestToRemove = signal<Participant | null>(null);
  readonly isRemovingGuest = signal<boolean>(false);

  readonly lastUpdatedAt = signal<Date | null>(null);
  readonly syncError = signal<boolean>(false);

  private syncSub: Subscription | null = null;
  private isSyncing = false;

  readonly consolidatedOrder = computed(() => {
    return consolidatePreOrder(this.participants());
  });

  readonly grandTotal = computed(() => {
    return this.consolidatedOrder().total;
  });

  readonly hasConfirmedDishes = computed(() => {
    return this.consolidatedOrder().items.length > 0;
  });

  readonly unsubmittedCount = computed(() => {
    return this.participants().filter((p) => !p.isSubmitted).length;
  });

  readonly isSubmitted = computed(() => {
    return this.groupStatus()?.status === 'Submitted';
  });

  readonly statusBadge = computed(() => {
    return this.isSubmitted() ? 'Sent to Restaurant' : 'Collecting pre-orders';
  });

  readonly allEmpty = computed(() => {
    const list = this.participants();
    return list.length > 0 && list.every((p) => p.items.length === 0);
  });

  constructor() {
    this.destroyRef.onDestroy(() => {
      this.stopSync();
    });
  }

  ngOnInit(): void {
    const resId = this.route.snapshot.paramMap.get('reservationId') ?? '';
    this.reservationId.set(resId);
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.stopSync();

    const resId = this.reservationId();
    this.groupOrdersService.getGroupInvite$(resId).subscribe({
      next: (invite) => {
        this.invite.set(invite);
        this.loadGroupDetails(invite.GroupReservationId);
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err.status === 404) {
          this.errorMessage.set('Group reservation not found');
        } else {
          this.errorMessage.set('Failed to load group pre-order');
        }
      },
    });
  }

  private loadGroupDetails(groupId: string): void {
    const resId = this.reservationId();

    this.groupOrdersService.getOwnerSessionId$(resId).subscribe({
      next: (ownerId) => this.ownerSessionId.set(ownerId),
      error: () => {
        // Ignored: silent failure for optional owner session ID
      },
    });

    this.groupOrdersService.getStatus$(groupId).subscribe({
      next: (status) => {
        this.groupStatus.set(status);
        if (status.status === 'Open') {
          this.startSync();
        } else {
          this.stopSync();
        }
      },
      error: () => {
        // Ignored: handled by template state
      },
    });

    this.groupOrdersService.getSessions$(groupId).subscribe({
      next: (sessions) => {
        this.participants.set(sessions.participants);
        this.lastUpdatedAt.set(new Date());
        this.syncError.set(false);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err.status === 404) {
          this.errorMessage.set('Group reservation not found');
        } else {
          this.errorMessage.set('Failed to load group sessions');
        }
      },
    });
  }

  startSync(): void {
    if (this.syncSub || this.isSubmitted()) return;

    this.syncSub = interval(15_000).subscribe(() => {
      this.syncNow(true);
    });
  }

  stopSync(): void {
    if (this.syncSub) {
      this.syncSub.unsubscribe();
      this.syncSub = null;
    }
  }

  refresh(): void {
    this.syncNow(false);
  }

  private syncNow(silent: boolean): void {
    const invite = this.invite();
    if (!invite || this.isSyncing || this.isSubmitted()) return;

    this.isSyncing = true;
    const groupId = invite.GroupReservationId;

    this.groupOrdersService.getStatus$(groupId, { silent }).subscribe({
      next: (status) => {
        this.groupStatus.set(status);
        if (status.status === 'Submitted') {
          this.stopSync();
        }
      },
      error: () => {
        // Ignored in background polling
      },
    });

    this.groupOrdersService.getSessions$(groupId, { silent }).subscribe({
      next: (sessions) => {
        this.participants.set(sessions.participants);
        this.lastUpdatedAt.set(new Date());
        this.syncError.set(false);
        this.isSyncing = false;
      },
      error: () => {
        this.syncError.set(true);
        this.isSyncing = false;
      },
    });
  }

  openSubmitModal(): void {
    this.isSubmitModalOpen.set(true);
  }

  closeSubmitModal(): void {
    if (!this.isSubmitting()) {
      this.isSubmitModalOpen.set(false);
    }
  }

  confirmSubmit(): void {
    if (this.isSubmitting()) return;

    const invite = this.invite();
    if (!invite) return;

    this.isSubmitting.set(true);
    const groupId = invite.GroupReservationId;
    const resId = this.reservationId();

    this.groupOrdersService.submit$(groupId, resId).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isSubmitModalOpen.set(false);
        this.stopSync();
        this.toastService.success('Pre-order sent to the restaurant');

        const currentStatus = this.groupStatus();
        if (currentStatus) {
          this.groupStatus.set({ ...currentStatus, status: 'Submitted' });
        } else {
          this.groupStatus.set({ currentParticipants: 0, maxCapacity: 0, status: 'Submitted' });
        }
      },
      error: () => {
        this.isSubmitting.set(false);
        this.closeSubmitModal();
      },
    });
  }

  promptRemoveGuest(participant: Participant): void {
    this.guestToRemove.set(participant);
  }

  closeRemoveModal(): void {
    if (!this.isRemovingGuest()) {
      this.guestToRemove.set(null);
    }
  }

  confirmRemoveGuest(): void {
    const guest = this.guestToRemove();
    const invite = this.invite();
    if (!guest || !invite || this.isRemovingGuest()) return;

    this.isRemovingGuest.set(true);
    const groupId = invite.GroupReservationId;

    this.groupOrdersService.removeSession$(groupId, guest.id).subscribe({
      next: (res) => {
        this.isRemovingGuest.set(false);
        this.guestToRemove.set(null);
        this.participants.set(res.participants);
      },
      error: () => {
        this.isRemovingGuest.set(false);
        this.guestToRemove.set(null);
      },
    });
  }
}
