import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, map, switchMap } from 'rxjs';
import { API_BASE_URL } from '@app/shared/config/api-base-url.token';
import { SKIP_ERROR_TOAST } from '@app/auth/skip-error-toast.context';
import { GuestInvite } from '@app/models/reservations/guest-invite.model';
import { GroupSessionsResponse } from '@app/models/group-orders/group-sessions-response.model';
import { GroupStatusResponse } from '@app/models/group-orders/group-status.model';

export interface RequestOptions {
  silent?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class GroupOrdersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private createContext(options?: RequestOptions): HttpContext {
    const context = new HttpContext();
    if (options?.silent) {
      context.set(SKIP_ERROR_TOAST, true);
    }
    return context;
  }

  getGroupInvite$(reservationId: string): Observable<GuestInvite> {
    return this.http
      .get<{ token: string }>(`${this.baseUrl}/reservations/${reservationId}/invite-token`)
      .pipe(
        switchMap(({ token }) =>
          this.http.get<GuestInvite>(`${this.baseUrl}/guest/invites/${token}`)
        )
      );
  }

  getSessions$(groupReservationId: string, options?: RequestOptions): Observable<GroupSessionsResponse> {
    return this.http.get<GroupSessionsResponse>(
      `${this.baseUrl}/guest/groups/${groupReservationId}/sessions`,
      { context: this.createContext(options) }
    );
  }

  getOwnerSessionId$(reservationId: string): Observable<string> {
    return this.http
      .get<{ guestSessionId: string }>(`${this.baseUrl}/reservations/${reservationId}/guest-session-id`)
      .pipe(map((res) => res.guestSessionId));
  }

  getStatus$(groupReservationId: string, options?: RequestOptions): Observable<GroupStatusResponse> {
    return this.http.get<GroupStatusResponse>(
      `${this.baseUrl}/guest/groups/${groupReservationId}/status`,
      { context: this.createContext(options) }
    );
  }

  submit$(
    groupReservationId: string,
    reservationId: string
  ): Observable<{ dishId: string; quantity: number }[]> {
    return this.http.post<{ dishId: string; quantity: number }[]>(
      `${this.baseUrl}/group-reservations/${groupReservationId}/submit`,
      { reservationId }
    );
  }

  removeSession$(
    groupReservationId: string,
    guestSessionId: string
  ): Observable<GroupSessionsResponse> {
    return this.http.delete<GroupSessionsResponse>(
      `${this.baseUrl}/guest/groups/${groupReservationId}/sessions/${guestSessionId}`
    );
  }
}
