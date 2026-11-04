# Звіти про дефекти (Bug Reports) за результатами тестування US11

**Посилання на шаблон:** [Google Spreadsheet Template (Дефект репорти)](https://docs.google.com/spreadsheets/d/1nOY-mdac7DvBux4VyclO6BOyY456S51a7pEfb0LFWUI/edit?pli=1&gid=1194140512#gid=1194140512)  
**Артефакт тестування (Screen capture):** `HW QA/image (4).png`

---

### Bug Report #1
* **ID:** 1
* **Summary:** Inconsistent letter capitalization across UI elements (Title Case vs Sentence case: 'Book a Table' vs 'Main page', 'All dishes' vs 'Main Courses')
* **Env:** `P_PRD / Web`
* **Priority:** `Low`
* **Severity:** `Minor`
* **Pre-conditions:**
  1. Guest has joined the group session (status 'Reserved').
  2. Guest is on the Menu page.
* **Steps:**
  1. Navigate to the Menu page as a Guest.
  2. Inspect the top header navigation links: `'Main page'`, `'Book a Table'`, `'Reservations'`.
  3. Inspect breadcrumbs: `'Main page'` > `'Menu'`.
  4. Inspect category tabs: `'All dishes'`, `'Appetizers'`, `'Main Courses'`.
  5. Inspect availability status badge on cards 1 & 2: `'Out Of Stock'`.
* **Actual Result:**  
  Letter capitalization rules are mixed and inconsistently applied across navigation, categories, and badges without following a single capitalization guide:
  - **Header navigation:** mixes Sentence case and Title Case: `'Main page'` (lowercase `'p'`) vs `'Book a Table'` (capitalized `'T'`).
  - **Breadcrumbs:** `'Main page'` (lowercase `'p'`) vs `'Menu'`.
  - **Category tabs:** `'All dishes'` (lowercase `'d'`, Sentence case) vs `'Main Courses'` (capitalized `'M'` and `'C'`, Title Case).
  - **Status badges:** `'Out Of Stock'` (capitalizes preposition `'Of'`) vs requirements terminology `'On Stop'`.
* **Expected Result:**  
  UI text across navigation, category filters, and badges must strictly adhere to a consistent capitalization style guide (preferably Title Case: `'Main Page'`, `'Book a Table'`, `'All Dishes'`, `'Main Courses'`, `'Out of Stock'` / `'On Stop'`).
* **Attachments:** `image (4).png` (Header navigation, breadcrumbs, category tabs)

---

### Bug Report #2
* **ID:** 2
* **Summary:** Grammatical typo in serving size for 'Fresh Avocado Toast': weight displayed as '2000' without units instead of '200 g'
* **Env:** `P_PRD / Web`
* **Priority:** `Medium`
* **Severity:** `Minor`
* **Pre-conditions:**
  1. Guest has joined the group session (status 'Reserved').
  2. Guest is on the Menu page viewing dish items.
* **Steps:**
  1. Navigate to the Menu page as a Guest.
  2. Locate the dish card 'Fresh Avocado Toast'.
  3. Inspect the serving size / weight label next to the price ($12.00).
* **Actual Result:**  
  The weight is displayed as `'2000'` (typo with an extra zero, and the unit `'g'` is completely missing).
* **Expected Result:**  
  According to **AC 2**: *"Each menu item must clearly display name, price, serving size in grams..."*. The weight must be formatted with unit `'g'` and reflect realistic portion size: `'200 g'`.
* **Attachments:** `image (4).png` (Картка 1: Fresh Avocado Toast)

---

### Bug Report #3
* **ID:** 3
* **Summary:** Inconsistent spacing and units formatting in serving size across menu cards ('2000', '350g', '450 g')
* **Env:** `P_PRD / Web`
* **Priority:** `Low`
* **Severity:** `Minor`
* **Pre-conditions:**
  1. Guest is on the Menu page.
  2. Dishes with serving sizes are displayed.
* **Steps:**
  1. Navigate to the Menu page.
  2. Compare the weight label formatting across all three cards: 'Fresh Avocado Toast', 'Truffle Mushroom Risotto', and 'Margherita Pizza'.
* **Actual Result:**  
  Weight formatting is completely inconsistent:
  - `Fresh Avocado Toast`: `'2000'` (missing unit `'g'`, extra zero).
  - `Truffle Mushroom Risotto`: `'350g'` (no space between number and unit).
  - `Margherita Pizza`: `'450 g'` (standard space before unit).
* **Expected Result:**  
  Serving size must follow a uniform format specification across the application: `'{value} g'` (e.g. `'200 g'`, `'350 g'`, `'450 g'`).
* **Attachments:** `image (4).png` (Усі картки)

---

### Bug Report #4
* **ID:** 4
* **Summary:** Status badge terminology mismatch with requirements ('Out Of Stock' in UI vs 'On Stop' in US AC 4)
* **Env:** `P_PRD / Web`
* **Priority:** `Low`
* **Severity:** `Trivial`
* **Pre-conditions:**
  1. Guest is on the Menu page.
  2. Menu contains unavailable dishes ('Fresh Avocado Toast', 'Truffle Mushroom Risotto').
* **Steps:**
  1. Navigate to the Menu page.
  2. Inspect the availability badge label on unavailable dishes ('Fresh Avocado Toast', 'Truffle Mushroom Risotto').
* **Actual Result:**  
  The badge displays `'Out Of Stock'`, whereas Acceptance Criteria AC 4 explicitly defines the requirement: *"Items currently marked 'On Stop' are clearly indicated as unavailable..."*.
* **Expected Result:**  
  Terminology in the application UI should match the approved requirements specification and design glossary: use `'On Stop'` (or update the requirements specification to standard `'Out of Stock'`).
* **Attachments:** `image (4).png` (Картки 1 та 2)
