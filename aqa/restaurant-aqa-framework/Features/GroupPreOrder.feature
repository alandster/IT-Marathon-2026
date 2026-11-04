@direction1 @group-preorder
Feature: Group Pre-Order (Direction 1)

  The reservation owner invites guests to make their own choices. Each guest
  picks dishes, while the owner reviews all selections and consolidates them
  into a single pre-order transmitted to the restaurant.

  Rule: The owner can view participants and order totals (Level 1)

    @api
    Scenario: The owner sees invited participants, their chosen dishes, and individual totals
      Given an active group reservation with invited guests
      When guests add dishes to their individual carts
      Then the owner sees all joined participants with their dishes, quantities, and totals
      And the grand total equals the sum of all participant totals

    @api
    Scenario: A guest who has not submitted their cart is marked accordingly
      Given an active group reservation with invited guests
      When a guest adds dishes but leaves the cart unconfirmed
      Then the owner sees the participant in the list marked as still choosing

    @api
    Scenario: A group with no joined guests displays an empty list and zero total
      Given an active group reservation with no joined guests
      When the owner reviews the group pre-order
      Then the participant list is empty and the grand total is zero

    @negative @api
    Scenario: Only the reservation owner can view group participants
      Given an active group reservation with invited guests
      When a customer who is not the owner attempts to view the group sessions
      Then access to the group sessions is refused

    @negative @api
    Scenario: Viewing a nonexistent group returns not found
      When the owner views sessions for a nonexistent group
      Then the request is rejected as not found

  Rule: The owner confirms the consolidated pre-order (Level 2)

    @api
    Scenario: The owner successfully confirms the consolidated pre-order
      Given an active group reservation where participants have submitted carts
      When the owner confirms the consolidated pre-order
      Then only dishes from confirmed carts are combined into the pre-order
      And the order status changes to submitted by customer
      And the group reservation status becomes submitted

    @api
    Scenario: The assigned waiter sees the consolidated pre-order
      Given a consolidated group pre-order submitted by the owner
      When the assigned waiter inspects the order
      Then the waiter sees the exact consolidated dishes confirmed by the owner

    @negative @api
    Scenario Outline: A group pre-order cannot be confirmed under invalid conditions
      Given an active group reservation in condition "<condition>"
      When the owner attempts to confirm the consolidated pre-order
      Then the pre-order confirmation is refused with reason "<reason>"

      Examples:
        | condition                     | reason                 |
        | without submitted carts       | no submitted carts     |
        | empty submitted carts         | no dishes in cart      |
        | already submitted             | already submitted      |

    @negative @api
    Scenario: A non-owner cannot confirm the group pre-order
      Given an active group reservation where participants have submitted carts
      When a customer who is not the owner attempts to confirm the pre-order
      Then the pre-order confirmation is forbidden

  Rule: The owner can remove a guest from the group (Level 3)

    @api
    Scenario: The owner removes a guest and the total is recalculated
      Given an active group reservation with multiple guests having dishes
      When the owner removes one of the guests from the group
      Then the removed guest is no longer in the participant list
      And the grand total is updated to exclude the removed guest's dishes

    @api
    Scenario: The owner removes a guest who has already confirmed their cart
      Given an active group reservation with a guest whose cart is submitted
      When the owner removes the submitted guest
      Then the guest is removed and their dishes are not included in subsequent consolidation

    @negative @api
    Scenario: Removing a guest from another group is rejected
      Given an active group reservation "Group A"
      And an active group reservation "Group B" with a guest
      When the owner of "Group A" attempts to remove the guest of "Group B"
      Then the removal is rejected as an invalid request

    @negative @api
    Scenario: Removing a nonexistent guest returns not found
      Given an active group reservation
      When the owner attempts to remove a nonexistent guest session
      Then the removal is rejected as not found

    @negative @api
    Scenario: A non-owner cannot remove a guest
      Given an active group reservation with a joined guest
      When a customer who is not the owner attempts to remove the guest
      Then the removal is refused

    @smoke @api
    Scenario: End-to-end flow from guests selection to waiter review
      Given an active group reservation with multiple guests
      When guests select their dishes and confirm their carts
      And the owner reviews the order and removes one participant
      And the owner submits the consolidated pre-order
      Then the waiter sees the final order without dishes of the removed participant
