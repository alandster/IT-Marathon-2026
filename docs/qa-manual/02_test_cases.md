# Тест-кейси для перевірки User Story US11 (Test Cases Suite)

**Об'єкт тестування:** User Story `[Draft] US11_Guest Viewing Reservation Menu`  
**Посилання на шаблон:** [Google Spreadsheet Template](https://docs.google.com/spreadsheets/d/1nOY-mdac7DvBux4VyclO6BOyY456S51a7pEfb0LFWUI/edit?pli=1&gid=0#gid=0)

---

### TC-01: Verify that Guest can view the full menu structured logically by category (AC 1)
* **Status:** Passed
* **Pre-conditions:**
  1. Guest has joined the session via valid invite link (US_G10).
  2. Group reservation status is "Reserved".
  3. Restaurant has dishes configured across multiple categories (Appetizers, Main Courses, Desserts, Beverages).
* **Steps:**
  1. Open the Menu page as a Guest.
  2. Verify category tabs/buttons displayed at the top of the menu section.
  3. Click on each category tab (e.g., 'Appetizers', 'Main Courses').
* **Expected Result:**
  1. Menu page loads successfully displaying all available dishes.
  2. Categories match the restaurant catalog ('All dishes', 'Appetizers', 'Main Courses', etc.).
  3. Clicking on a category displays only dishes belonging to that category; the selected category is highlighted as active.

---

### TC-02: Verify dish card content and layout against Figma design (AC 2)
* **Status:** **Failed** *(Пов'язано з Bug #1 та Bug #2: вага '2000' замість '200 g')*
* **Pre-conditions:**
  1. Guest is on the Menu page.
  2. Active dishes exist in the selected category.
* **Steps:**
  1. Inspect an available dish card (e.g., 'Margherita Pizza') and cards in the list.
  2. Check presence of: dish image, name, price, serving size in grams, and action button.
* **Expected Result:**
  1. Dish card contains:
     - Dish thumbnail image
     - Dish name
     - Price formatted in currency (e.g., '$14.99')
     - Weight formatted with units (e.g., '450 g', '200 g')
     - Active [Pre-Order] button.
  2. Layout strictly adheres to Figma specification.
* **Actual Result:**
  * Serving size for 'Fresh Avocado Toast' displays `'2000'` without units and with an extra zero.

---

### TC-03: Verify adding an available dish to guest personal subcart via [Pre-Order] button (AC 3)
* **Status:** Passed
* **Pre-conditions:**
  1. Guest is on the Menu page.
  2. Dish is available (not 'On Stop' / 'out of Stock').
* **Steps:**
  1. Click the [Pre-Order] button on an available dish card (e.g., 'Margherita Pizza').
  2. Check guest personal cart/state.
* **Expected Result:**
  1. The dish is added to the Guest's personal pre-order subcart with quantity 1.
  2. Subcart counter or toast notification reflects the addition.

---

### TC-04: Verify that 'On Stop' / 'Out of Stock' items display unavailability badge and cannot be ordered (AC 4)
* **Status:** Passed
* **Pre-conditions:**
  1. A dish in the database has state 'Unavailable' / 'On Stop'.
  2. Guest is on the Menu page.
* **Steps:**
  1. Locate the 'On Stop' / 'out of Stock' dish on the page (e.g., 'Fresh Avocado Toast').
  2. Verify visual indication of unavailability.
  3. Verify that the dish cannot be added to the pre-order subcart (action button is greyed-out/disabled, card is non-interactive).
* **Expected Result:**
  1. Dish card displays unavailability badge: 'out of Stock' with warning styling.
  2. Action button [Pre-Order] is greyed-out and disabled (`[disabled]` attribute) so user cannot add it to cart.
  3. Guest cannot add the dish to cart.

---

### TC-05: Verify that clicking an unavailable card does not allow adding to cart or open pre-order action (AC 4)
* **Status:** Passed
* **Pre-conditions:**
  1. Guest is on the Menu page.
  2. Item is marked as 'out of Stock'.
* **Steps:**
  1. Click on the body of an unavailable dish card.
  2. Attempt keyboard interaction (Tab + Enter/Space) on the card.
* **Expected Result:**
  1. Dish card has `tabindex="-1"` or disabled state; no pre-order action is triggered.
  2. If details modal opens, the [Pre-Order] button inside is disabled.

---

### TC-06: Verify menu data freshness upon opening or manual page refresh (AC 5)
* **Status:** Passed
* **Pre-conditions:**
  1. Guest is viewing the Menu page.
  2. Restaurant manager changes the price or availability of a dish in the backend.
* **Steps:**
  1. Guest refreshes the page (or clicks Refresh).
  2. Observe updated dish price and status.
* **Expected Result:**
  1. The page fetches current data from GET /api/v1/dishes.
  2. Updated pricing and availability status are accurately rendered.

---

### TC-07: Verify menu behavior when parent reservation status is Cancelled or Expired (Pre-conditions / Negative)
* **Status:** Passed
* **Pre-conditions:**
  1. Parent reservation status changed to 'Cancelled' or 'Closed'.
  2. Guest attempts to access the menu via the invitation link.
* **Steps:**
  1. Navigate to the menu URL using guest session token for a cancelled reservation.
* **Expected Result:**
  1. Guest is redirected to an informative screen: "This pre-order session has expired or is no longer active."
  2. Menu items and [Pre-Order] actions are not accessible.

---

### TC-08: Verify dish list sorting functionality (Figma / Functional)
* **Status:** Passed
* **Pre-conditions:**
  1. Guest is on the Menu page with multiple dishes displayed.
* **Steps:**
  1. Click on 'Sort by:' dropdown.
  2. Select 'Price: Low to High'.
  3. Select 'Price: High to Low'.
* **Expected Result:**
  1. Dropdown offers expected options (Recommended, Price: Low to High, Price: High to Low, Name A-Z).
  2. Dish cards are reordered dynamically according to the selected criteria.

---

### TC-09: Verify reservation details banner at the top of the menu page
* **Status:** Passed
* **Pre-conditions:**
  1. Guest joined a valid group session.
* **Steps:**
  1. Inspect the notice banner above the menu categories.
* **Expected Result:**
  1. Banner displays accurate reservation details: restaurant address, table number, and reserved time slot (e.g., 'You are making a pre-order for a reservation...').

---

### TC-10: Verify consistency and formatting of UI labels, status badges and weights on menu page
* **Status:** **Failed** *(Пов'язано з Bug #3 та Bug #4)*
* **Pre-conditions:**
  1. Guest is on the Menu page.
  2. Multiple dish cards are rendered.
* **Steps:**
  1. Inspect the status badge text on out-of-stock items.
  2. Inspect the weight format and button captions across all cards.
  3. Compare unavailable cards ('Fresh Avocado Toast' vs 'Truffle Mushroom Risotto').
* **Expected Result:**
  1. Status badge label casing and terminology are uniform ('Out of Stock' or 'On Stop').
  2. Weight units are consistently formatted (e.g. '200 g', '350 g', '450 g').
  3. Uniform UI display across cards of identical status.
* **Actual Result:**
  * Status badge uses non-standard lowercase first letter `'out of Stock'` and differs from the US AC 4 terminology `'On Stop'`.
  * Inconsistent weight formatting: displayed as `'2000'` (extra 0, missing unit), `'350g'` (missing space), `'450 g'`.
  * Inconsistent action button rendering: 'Fresh Avocado Toast' displays a greyed-out button while 'Truffle Mushroom Risotto' has no button.
