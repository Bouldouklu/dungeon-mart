# Unity Editor Setup Instructions - Monthly Expenses & Loan System

This document provides step-by-step instructions for setting up the new monthly rent and loan systems in the Unity Editor.

## Overview of New Systems

### New Manager Scripts (Created)
1. **ExpenseManager** - Tracks monthly rent and payment deadlines
2. **LoanManager** - Manages loans with interest and auto-repayment
3. **FailStateManager** - Handles game over conditions

### New UI Scripts (Created)
1. **RentPaymentUI** - Modal for rent payment
2. **LoanUI** - Modal for loan selection
3. **LoanStatusUI** - HUD display for active loans
4. **GameOverUI** - Game over screen
5. **RentCountdownUI** - HUD display for rent countdown

### Updated Scripts
1. **GameManager** - Added money change events
2. **EndOfDaySummaryUI** - Shows rent/loan info

---

## Part 1: Manager Setup

### 1.1 Create Manager GameObject
1. In Unity Hierarchy, locate or create an empty GameObject named **"Managers"**
2. This GameObject should be at the root level of GameScene

### 1.2 Add ExpenseManager
1. Select the **Managers** GameObject
2. Click **Add Component**
3. Search for and add **ExpenseManager** script
4. Configure settings in Inspector:
   - **Monthly Rent Amount**: 500 (default)
   - **Days Per Month**: 7 (default)

### 1.3 Add LoanManager
1. With **Managers** GameObject still selected
2. Click **Add Component**
3. Search for and add **LoanManager** script
4. Configure settings in Inspector:
   - **Available Loan Amounts**: Set size to 3
     - Element 0: 300
     - Element 1: 500
     - Element 2: 1000
   - **Interest Rate**: 0.2 (20% interest)
   - **Repayment Days**: 7

### 1.4 Add FailStateManager
1. With **Managers** GameObject still selected
2. Click **Add Component**
3. Search for and add **FailStateManager** script
4. No configuration needed - it will auto-subscribe to failure events

---

## Part 2: HUD UI Setup

### 2.1 Create Rent Countdown HUD Element
1. In Hierarchy, locate your **Canvas** (main UI canvas)
2. Right-click Canvas → **UI → TextMeshPro - Text**
3. Rename to **"RentCountdownText"**
4. Position in upper-left corner of screen:
   - **Anchor**: Top-Left
   - **Pos X**: 20, **Pos Y**: -60
   - **Width**: 300, **Height**: 40
5. Configure TextMeshPro:
   - **Text**: "Rent Due: 7 days ($500)"
   - **Font Size**: 18
   - **Alignment**: Left, Middle
   - **Color**: White

6. Create UI Manager GameObject:
   - In Hierarchy, locate or create **"UI Managers"** empty GameObject (at root or under Canvas)
   - Right-click UI Managers → **Create Empty**
   - Rename to **"RentCountdownUI"**
   - Add **RentCountdownUI** component to this GameObject
   - Link **RentCountdownText** (from Canvas) to the script's **rentCountdownText** field

### 2.2 Create Loan Status HUD Element
1. **Create Panel on Canvas** (for grouping visual elements):
   - Right-click Canvas → **UI → Panel**
   - Rename to **"LoanStatusPanel"**
   - **Remove Image component** (we just want the layout, not background)
   - Position in upper-left corner (below rent countdown):
     - **Anchor**: Top-Left
     - **Pos X**: 160, **Pos Y**: -100
     - **Width**: 250, **Height**: 60

2. **Create child text elements inside panel**:
   - **Right-click LoanStatusPanel → UI → TextMeshPro - Text**
   - Rename to **"LoanBalanceText"**
   - Text: "Loan: $0/$600"
   - Font Size: 16
   - Position at top of panel

   - **Right-click LoanStatusPanel → UI → TextMeshPro - Text**
   - Rename to **"DaysRemainingText"**
   - Position below balance text
   - Text: "Due in: 7 days"
   - Font Size: 14

   - **Right-click LoanStatusPanel → UI → Image**
   - Rename to **"WarningIndicator"**
   - Small red/yellow indicator icon (10x10 pixels)
   - Position next to days text

3. **Create UI Manager GameObject**:
   - Right-click **UI Managers** → **Create Empty**
   - Rename to **"LoanStatusUI"**
   - Add **LoanStatusUI** component to this GameObject
   - Link elements to script fields:
     - loanStatusPanel → LoanStatusPanel (the panel on canvas)
     - loanBalanceText → LoanBalanceText
     - daysRemainingText → DaysRemainingText
     - warningIndicator → WarningIndicator

---

## Part 3: Rent Payment UI

### 3.1 Create Rent Payment Panel
1. Right-click Canvas → **UI → Panel**
2. Rename to **"RentPaymentPanel"**
3. Set to **stretch/fill** entire screen (anchors to corners)
4. **Background**: Semi-transparent dark color (RGBA: 0, 0, 0, 200)
5. **Disable by default** (uncheck in Inspector)

### 3.2 Create Panel Content
Inside RentPaymentPanel, create:

**Title:**
- UI → TextMeshPro - Text
- Name: "TitleText"
- Text: "Rent Due!"
- Font Size: 48, Bold
- Alignment: Center, Top
- Position: Top-center

**Month Text:**
- UI → TextMeshPro - Text
- Name: "MonthText"
- Text: "Month 1 Complete"
- Font Size: 24
- Position: Below title

**Rent Amount:**
- UI → TextMeshPro - Text
- Name: "RentAmountText"
- Text: "Rent: $500"
- Font Size: 32
- Color: Yellow

**Current Money:**
- UI → TextMeshPro - Text
- Name: "CurrentMoneyText"
- Text: "Your Money: $450"
- Font Size: 28

**Warning Text:**
- UI → TextMeshPro - Text
- Name: "WarningText"
- Text: "Your landlord demands tribute!"
- Font Size: 20
- Color: White

**Buttons:**
- UI → Button - TextMeshPro
  - Name: "PayRentButton"
  - Text: "Pay Rent"
  - Size: 200x50

- UI → Button - TextMeshPro
  - Name: "TakeLoanButton"
  - Text: "Take Emergency Loan"
  - Size: 200x50
  - Position: Below Pay Rent button

### 3.3 Add RentPaymentUI Script
1. In **UI Managers** GameObject:
   - Right-click → **Create Empty**
   - Rename to **"RentPaymentUI"**
   - Add Component → **RentPaymentUI**
2. Link all UI elements from RentPaymentPanel to script fields:
   - rentPanel → RentPaymentPanel
   - titleText → TitleText
   - rentAmountText → RentAmountText
   - currentMoneyText → CurrentMoneyText
   - monthText → MonthText
   - warningText → WarningText
   - payRentButton → PayRentButton
   - takeLoanButton → TakeLoanButton

---

## Part 4: Loan Selection UI

### 4.1 Create Loan Panel
1. Right-click Canvas → **UI → Panel**
2. Rename to **"LoanPanel"**
3. Set to **stretch/fill** entire screen
4. **Background**: Semi-transparent dark color
5. **Disable by default**

### 4.2 Create Loan Panel Content

**Title:**
- TextMeshPro: "Emergency Loan Services"
- Font Size: 42

**Interest Rate Text:**
- TextMeshPro: "Interest Rate: 20% (We're definitely not loan sharks!)"
- Font Size: 18
- Color: Yellow

**Warning Text:**
- TextMeshPro: "Borrow wisely! Loans must be repaid within 7 days."
- Font Size: 16

**Scroll View for Loan Options:**
1. UI → Scroll View
2. Name: "LoanOptionsScrollView"
3. Inside Content area, add **Vertical Layout Group** component:
   - Spacing: 10
   - Child Alignment: Upper Center

**Close Button:**
- UI → Button - TextMeshPro
- Name: "ClosePanelButton"
- Text: "Close"

### 4.3 Create Loan Option Button Prefab

**Important: This needs to be a prefab!**

1. Right-click in Hierarchy → **UI → Button - TextMeshPro**
2. Rename to **"LoanOptionButton"**
3. Configure button:
   - Width: 400
   - Height: 80
   - Add **Horizontal Layout Group**:
     - Padding: 10
     - Spacing: 10
     - Child Alignment: Middle-Left

4. Inside button, create two TextMeshPro elements:
   - **AmountText** (left side):
     - Text: "Borrow $500"
     - Font Size: 24
     - Bold
   - **DetailsText** (right side):
     - Text: "Repay $600 ($100 interest)"
     - Font Size: 18
     - Color: Gray

5. **Drag LoanOptionButton to Project folder** to create prefab
6. Delete from Hierarchy (it will be spawned at runtime)

### 4.4 Add LoanUI Script
1. In **UI Managers** GameObject:
   - Right-click → **Create Empty**
   - Rename to **"LoanUI"**
   - Add Component → **LoanUI**
2. Link all UI elements from LoanPanel:
   - loanPanel → LoanPanel
   - titleText → Title text
   - warningText → Warning text
   - interestRateText → Interest rate text
   - loanOptionsContainer → Scroll View Content area
   - **loanOptionButtonPrefab** → Drag prefab from Project folder
   - closePanelButton → Close button

---

## Part 5: Game Over UI

### 5.1 Create Game Over Panel
1. Right-click Canvas → **UI → Panel**
2. Rename to **"GameOverPanel"**
3. Set to **stretch/fill** entire screen
4. **Background**: Solid dark color (RGBA: 0, 0, 0, 230)
5. **Disable by default**

### 5.2 Create Game Over Content

**Title:**
- TextMeshPro: "GAME OVER"
- Font Size: 64, Bold
- Color: Red
- Position: Top-center

**Reason Text:**
- TextMeshPro: "You've been evicted! The space is now a Spirit Halloween store."
- Font Size: 24
- Word Wrap: Enabled
- Position: Below title

**Statistics Section:**
Create three TextMeshPro elements:
- **StatsDaysSurvivedText**: "Days Survived: 15"
- **StatsTotalRevenueText**: "Total Revenue: $2,450"
- **StatsAmountOwedText**: "Rent Owed: $500"

**Buttons:**
Create three buttons in a vertical layout:
- **TryAgainButton**: "Try Again" (large, green)
- **MainMenuButton**: "Main Menu" (medium, blue)
- **QuitButton**: "Quit" (small, red)

### 5.3 Add GameOverUI Script
1. In **UI Managers** GameObject:
   - Right-click → **Create Empty**
   - Rename to **"GameOverUI"**
   - Add Component → **GameOverUI**
2. Link all UI elements from GameOverPanel:
   - gameOverPanel → GameOverPanel
   - titleText → Title text
   - reasonText → Reason text
   - statsDaysSurvivedText → Days survived text
   - statsTotalRevenueText → Total revenue text
   - statsAmountOwedText → Amount owed text
   - tryAgainButton → Try Again button
   - mainMenuButton → Main Menu button
   - quitButton → Quit button
3. Set **Scene Names** in Inspector:
   - Main Menu Scene Name: "MainMenu"
   - Game Scene Name: "GameScene"

---

## Part 6: Update EndOfDaySummaryUI

### 6.1 Add New Text Elements to Existing Panel
1. Locate **EndOfDaySummaryPanel** in Canvas
2. Add two new TextMeshPro elements:

**RentInfoText:**
- Name: "RentInfoText"
- Text: "Rent Due: 5 days ($500)"
- Font Size: 20
- Position: Below Total Money text

**LoanPaymentText:**
- Name: "LoanPaymentText"
- Text: "Loan Balance: $400 (Due in 5 days)"
- Font Size: 18
- Position: Below Rent Info text
- Initially disabled

### 6.2 Link to Script
1. Select **EndOfDaySummaryPanel** GameObject
2. Find **EndOfDaySummaryUI** component
3. Link new fields:
   - rentInfoText → RentInfoText
   - loanPaymentText → LoanPaymentText

---

## Part 7: Testing & Verification

### 7.1 Test Rent System
1. **Play the game**
2. **Press M** 7 times to advance days
3. **Rent payment UI should appear**
4. Verify:
   - ✅ Rent amount is correct ($500)
   - ✅ Current money is displayed
   - ✅ Pay button works if you have money
   - ✅ Loan button appears if you don't have money

### 7.2 Test Loan System
1. **Start game with low money** (edit GameManager starting money to $100)
2. **Advance to rent day**
3. **Click "Take Emergency Loan"**
4. Verify:
   - ✅ Loan options display with correct amounts
   - ✅ Interest calculations are correct (20%)
   - ✅ Taking loan adds money to player
   - ✅ Rent auto-pays after taking loan
   - ✅ Loan status appears in HUD

### 7.3 Test Auto-Deduction
1. **Take a loan**
2. **Earn some money** (serve customers)
3. **End the day** (Press K)
4. Verify:
   - ✅ Daily loan payment is deducted automatically
   - ✅ Loan status updates in HUD
   - ✅ Summary shows loan balance

### 7.4 Test Game Over Conditions
1. **Don't pay rent and have no money**
2. Verify:
   - ✅ Game Over UI appears
   - ✅ Correct message displays
   - ✅ Statistics are accurate
   - ✅ Buttons work (Try Again, Main Menu, Quit)

2. **Take loan and don't repay**
3. **Wait 7+ days**
4. Verify:
   - ✅ Game Over triggers on loan default
   - ✅ Correct message for loan default

---

## Part 8: Balance Tuning

### 8.1 Recommended Starting Values

**GameManager:**
- Starting Money: $500 (enough for initial rent + buffer)

**ExpenseManager:**
- Monthly Rent: $500
- Days Per Month: 7

**LoanManager:**
- Available Amounts: $300, $500, $1000
- Interest Rate: 20% (0.2)
- Repayment Days: 7

### 8.2 Testing Different Difficulty Levels

**Easy Mode:**
- Starting Money: $700
- Rent: $400
- Interest: 15%

**Normal Mode (Recommended):**
- Starting Money: $500
- Rent: $500
- Interest: 20%

**Hard Mode:**
- Starting Money: $400
- Rent: $600
- Interest: 25%

---

## Part 9: Common Issues & Solutions

### Issue: Rent never triggers
**Solution:** Make sure ExpenseManager is attached to a GameObject in the scene and DayManager.OnDayEnded event is firing.

### Issue: Loan panel doesn't show options
**Solution:** Verify loanOptionButtonPrefab is linked in LoanUI and has correct hierarchy (button with two TextMeshPro children).

### Issue: Game Over doesn't trigger
**Solution:** Check that FailStateManager is in scene and subscribed to ExpenseManager.OnRentFailed and LoanManager.OnLoanDefaulted events.

### Issue: HUD elements not updating
**Solution:** Ensure singleton instances are properly set up (check console for null reference errors).

### Issue: Player can't move after closing rent UI
**Solution:** Verify RentPaymentUI and LoanUI are both calling `PlayerController.SetCanMove(true)` in their close methods.

---

## Part 10: Final Checklist

Before committing changes, verify:

- ✅ All manager scripts attached to Managers GameObject
- ✅ All UI panels created and configured
- ✅ All UI scripts attached and fields linked
- ✅ LoanOptionButton prefab created and linked
- ✅ Rent countdown appears in HUD
- ✅ Loan status appears when loan is active
- ✅ Rent payment UI works correctly
- ✅ Loan UI works correctly
- ✅ Game Over UI works correctly
- ✅ EndOfDaySummary shows rent/loan info
- ✅ All UI panels disabled by default (except HUD)
- ✅ Scene names configured in GameOverUI
- ✅ Tested full gameplay loop with rent and loans

---

## Congratulations!

You've successfully implemented the Monthly Expenses and Loan System! Players now have meaningful financial pressure and ways to lose the game, making DungeonMart3D more challenging and strategic.

### Next Steps:
- Playtest and balance difficulty
- Add more corporate humor to dialogue
- Consider adding grace periods or warnings
- Implement achievement system (survived X months, paid off Y loans, etc.)
