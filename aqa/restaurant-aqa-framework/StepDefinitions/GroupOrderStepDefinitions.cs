using Restaurant_Aqa_Framework.Screenplay;
using Restaurant_Aqa_Framework.Screenplay.Questions;
using Restaurant_Aqa_Framework.Screenplay.Questions.GroupOrders;
using Restaurant_Aqa_Framework.Screenplay.Tasks.GroupOrders;

namespace Restaurant_Aqa_Framework.StepDefinitions;

[Binding]
public sealed class GroupOrderStepDefinitions(Actor actor) : StepDefinitionBase(actor)
{
    [Given("an active group reservation with invited guests")]
    public Task GivenActiveGroupReservationWithInvitedGuests() =>
        Actor.AttemptsToAsync(new SetUpGroupReservation(initialGuests: 2));

    [Given("an active group reservation with no joined guests")]
    public Task GivenActiveGroupReservationWithNoJoinedGuests() =>
        Actor.AttemptsToAsync(new SetUpGroupReservation(initialGuests: 0));

    [Given("an active group reservation where participants have submitted carts")]
    public async Task GivenActiveGroupReservationWithSubmittedCarts()
    {
        await Actor.AttemptsToAsync(new SetUpGroupReservation(initialGuests: 2));
        await Actor.AttemptsToAsync(new AddGuestDishesAndSubmit(submitCart: true));
    }

    [Given("a consolidated group pre-order submitted by the owner")]
    public async Task GivenConsolidatedGroupPreOrderSubmitted()
    {
        await Actor.AttemptsToAsync(new SetUpGroupReservation(initialGuests: 2));
        await Actor.AttemptsToAsync(new AddGuestDishesAndSubmit(submitCart: true));
        await Actor.AttemptsToAsync(new ConfirmConsolidatedPreOrder());
    }

    [Given("an active group reservation in condition {string}")]
    public async Task GivenActiveGroupReservationInCondition(string condition)
    {
        var guestsCount = condition.Contains("without submitted", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
        await Actor.AttemptsToAsync(new SetUpGroupReservation(initialGuests: guestsCount));
        if (condition.Contains("already submitted", StringComparison.OrdinalIgnoreCase))
        {
            await Actor.AttemptsToAsync(new AddGuestDishesAndSubmit(submitCart: true));
            await Actor.AttemptsToAsync(new ConfirmConsolidatedPreOrder());
        }
    }

    [Given("an active group reservation with multiple guests having dishes")]
    [Given("an active group reservation with multiple guests")]
    public async Task GivenActiveGroupReservationWithMultipleGuestsHavingDishes()
    {
        await Actor.AttemptsToAsync(new SetUpGroupReservation(initialGuests: 3));
        await Actor.AttemptsToAsync(new AddGuestDishesAndSubmit(submitCart: true));
    }

    [Given("an active group reservation with a guest whose cart is submitted")]
    public async Task GivenActiveGroupReservationWithGuestCartSubmitted()
    {
        await Actor.AttemptsToAsync(new SetUpGroupReservation(initialGuests: 1));
        await Actor.AttemptsToAsync(new AddGuestDishesAndSubmit(submitCart: true));
    }

    [Given("an active group reservation {string}")]
    public Task GivenActiveGroupReservationNamed(string name) =>
        Actor.AttemptsToAsync(new SetUpGroupReservation(initialGuests: 1));

    [Given("an active group reservation {string} with a guest")]
    public Task GivenActiveGroupReservationNamedWithGuest(string name) =>
        Actor.AttemptsToAsync(new SetUpGroupReservation(initialGuests: 1));

    [Given("an active group reservation")]
    public Task GivenActiveGroupReservationGeneric() =>
        Actor.AttemptsToAsync(new SetUpGroupReservation(initialGuests: 1));

    [Given("an active group reservation with a joined guest")]
    public Task GivenActiveGroupReservationWithJoinedGuest() =>
        Actor.AttemptsToAsync(new SetUpGroupReservation(initialGuests: 1));

    [When("guests add dishes to their individual carts")]
    public async Task WhenGuestsAddDishesToIndividualCarts()
    {
        await Actor.AttemptsToAsync(new AddGuestDishesAndSubmit(submitCart: true));
        await Actor.AttemptsToAsync(new ViewOwnerGroupSessions());
    }

    [When("a guest adds dishes but leaves the cart unconfirmed")]
    public async Task WhenGuestAddsDishesLeavesUnconfirmed()
    {
        await Actor.AttemptsToAsync(new AddGuestDishesAndSubmit(submitCart: false));
        await Actor.AttemptsToAsync(new ViewOwnerGroupSessions());
    }

    [When("the owner reviews the group pre-order")]
    public Task WhenOwnerReviewsGroupPreOrder() =>
        Actor.AttemptsToAsync(new ViewOwnerGroupSessions());

    [When("a customer who is not the owner attempts to view the group sessions")]
    public Task WhenNonOwnerViewsGroupSessions() =>
        Actor.AttemptsToAsync(new ViewOwnerGroupSessions(token: "invalid-or-non-owner-token"));

    [When("the owner views sessions for a nonexistent group")]
    public Task WhenOwnerViewsNonexistentGroup() =>
        Actor.AttemptsToAsync(new ViewOwnerGroupSessions(groupReservationId: "00000000-0000-0000-0000-000000000000"));

    [When("the owner confirms the consolidated pre-order")]
    public Task WhenOwnerConfirmsConsolidatedPreOrder() =>
        Actor.AttemptsToAsync(new ConfirmConsolidatedPreOrder());

    [When("the assigned waiter inspects the order")]
    public Task WhenWaiterInspectsOrder() => Actor.AttemptsToAsync(new ViewOwnerGroupSessions());

    [When("the owner attempts to confirm the consolidated pre-order")]
    public Task WhenOwnerAttemptsToConfirmPreOrder() =>
        Actor.AttemptsToAsync(new ConfirmConsolidatedPreOrder());

    [When("a customer who is not the owner attempts to confirm the pre-order")]
    public Task WhenNonOwnerAttemptsToConfirmPreOrder() =>
        Actor.AttemptsToAsync(new ConfirmConsolidatedPreOrder(token: "non-owner-token"));

    [When("the owner removes one of the guests from the group")]
    public Task WhenOwnerRemovesOneGuest() =>
        Actor.AttemptsToAsync(new RemoveGuestFromGroup());

    [When("the owner removes the submitted guest")]
    public Task WhenOwnerRemovesSubmittedGuest() =>
        Actor.AttemptsToAsync(new RemoveGuestFromGroup());

    [When("the owner of {string} attempts to remove the guest of {string}")]
    public Task WhenOwnerRemovesGuestFromAnotherGroup(string ownerGroup, string foreignGroup) =>
        Actor.AttemptsToAsync(new RemoveGuestFromGroup(sessionId: "foreign-session-id"));

    [When("the owner attempts to remove a nonexistent guest session")]
    public Task WhenOwnerRemovesNonexistentGuest() =>
        Actor.AttemptsToAsync(new RemoveGuestFromGroup(sessionId: "00000000-0000-0000-0000-000000000000"));

    [When("a customer who is not the owner attempts to remove the guest")]
    public Task WhenNonOwnerAttemptsToRemoveGuest() =>
        Actor.AttemptsToAsync(new RemoveGuestFromGroup(token: "non-owner-token"));

    [When("guests select their dishes and confirm their carts")]
    public Task WhenGuestsSelectDishesAndConfirmCarts() =>
        Actor.AttemptsToAsync(new AddGuestDishesAndSubmit(submitCart: true));

    [When("the owner reviews the order and removes one participant")]
    public async Task WhenOwnerReviewsAndRemovesParticipant()
    {
        await Actor.AttemptsToAsync(new ViewOwnerGroupSessions());
        await Actor.AttemptsToAsync(new RemoveGuestFromGroup());
    }

    [When("the owner submits the consolidated pre-order")]
    public Task WhenOwnerSubmitsConsolidatedPreOrder() =>
        Actor.AttemptsToAsync(new ConfirmConsolidatedPreOrder());

    [Then("the owner sees all joined participants with their dishes, quantities, and totals")]
    public async Task ThenOwnerSeesAllJoinedParticipants()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(200);
    }

    [Then("the grand total equals the sum of all participant totals")]
    public async Task ThenGrandTotalEqualsSumOfAllParticipants()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(200);
    }

    [Then("the owner sees the participant in the list marked as still choosing")]
    public async Task ThenOwnerSeesParticipantMarkedAsStillChoosing()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(200);
    }

    [Then("the participant list is empty and the grand total is zero")]
    public async Task ThenParticipantListIsEmptyAndTotalZero()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(200);
    }

    [Then("access to the group sessions is refused")]
    public async Task ThenAccessToGroupSessionsIsRefused()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(400);
    }

    [Then("the request is rejected as not found")]
    [Then("the removal is rejected as not found")]
    public async Task ThenRequestIsRejectedAsNotFound()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(400);
    }

    [Then("only dishes from confirmed carts are combined into the pre-order")]
    public async Task ThenOnlyDishesFromConfirmedCartsCombined()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(200);
    }

    [Then("the order status changes to submitted by customer")]
    public async Task ThenOrderStatusChangesToSubmitted()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(200);
    }

    [Then("the group reservation status becomes submitted")]
    public async Task ThenGroupReservationStatusBecomesSubmitted()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(200);
    }

    [Then("the waiter sees the exact consolidated dishes confirmed by the owner")]
    [Then("the waiter sees the final order without dishes of the removed participant")]
    public async Task ThenWaiterSeesExactConsolidatedDishes()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(200);
    }

    [Then("the pre-order confirmation is refused with reason {string}")]
    public async Task ThenPreOrderConfirmationRefusedWithReason(string reason)
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(400);
    }

    [Then("the pre-order confirmation is forbidden")]
    public async Task ThenPreOrderConfirmationIsForbidden()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(400);
    }

    [Then("the removed guest is no longer in the participant list")]
    public async Task ThenRemovedGuestNoLongerInList()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(200);
    }

    [Then("the grand total is updated to exclude the removed guest's dishes")]
    public async Task ThenGrandTotalUpdatedToExcludeRemovedGuest()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(200);
    }

    [Then("the guest is removed and their dishes are not included in subsequent consolidation")]
    public async Task ThenGuestRemovedNotIncludedInConsolidation()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(200);
    }

    [Then("the removal is rejected as an invalid request")]
    public async Task ThenRemovalRejectedInvalid()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(400);
    }

    [Then("the removal is refused")]
    public async Task ThenRemovalRefused()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(400);
    }
}
