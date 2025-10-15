# DungeonMart - TODO List

## 🎯 ACTIVE IMPLEMENTATION: Growth/Tycoon Progression System

### **Game Design Foundation**

**Core Design Questions Answered:**

1. **What is the player doing?**
   - Running a fantasy shop: receiving deliveries → restocking shelves → serving customers → collecting money
   - Making economic decisions (ordering stock, managing cash flow)
   - Balancing time-sensitive operations

2. **What is stopping them?**
   - Economic pressure (monthly rent $500 every 7 days)
   - Inventory scarcity (limited starting money)
   - Time pressure (fixed customer waves)
   - Spatial constraints (shelf capacity, item sizes)
   - Fail states: Cannot pay rent or default on loans

3. **Why are they doing it?** ⚠️ **CURRENT WEAKNESS - NEEDS FIXING**
   - Current: Only survival ("don't go bankrupt") - negative motivation
   - **NEW GOAL: Build a thriving business empire** - positive motivation
   - Transform from survival game → empire-building tycoon game

---

## 📋 Implementation Roadmap

### ⚠️ **CRITICAL WORKFLOW: TEST AFTER EACH PHASE**

**DO NOT** proceed to the next phase until the current phase is:
1. ✅ Fully implemented
2. ✅ Tested in Unity play mode
3. ✅ Working as expected
4. ✅ Approved for next phase

This iterative testing ensures we catch bugs early and validate design decisions before building on top of them.

---

## Phase 1: Core Progression Framework ✅ COMPLETE

**Goal:** Give players clear milestones and track their empire growth

### 1.1 Revenue Goal System ✅ COMPLETE

**Implementation:**
- Created `ProgressionManager.cs` to track lifetime revenue and tier progression
- Created `ProgressionDataSO.cs` for tier definitions (5 tiers: Street Vendor → Tycoon)
- Created `ProgressionUI.cs` for persistent progress bar display
- Milestone tracking: $0, $1,500, $4,000, $8,000, $15,000
- Events: OnTierReached, OnLifetimeRevenueChanged

**Testing:** All core functionality tested and working ✅

---

### 1.2 Shop Upgrade System ✅ COMPLETE

**Backend Implementation:**
- Created `UpgradeDataSO.cs`, `UpgradeManager.cs`, `ShopSegmentManager.cs`
- Created `UpgradeShopUI.cs` and `UpgradeCard.cs` for upgrade shop interface
- Integrated rent contribution into `ExpenseManager` (base + segment costs)
- Modified `Shelf.cs`, `CustomerSpawner.cs` for dynamic upgrades
- Created 8 UpgradeDataSO assets for tiers 1-3

**UI Implementation (Current Session):**
- ✅ Card-based upgrade shop with ScrollView layout
- ✅ Purchase confirmation dialog
- ✅ Visual states: Locked (gray), Available (green), Owned (blue), Maxed (gold)
- ✅ Price color logic: Green (affordable), Red (can't afford), Gray (tier-locked)
- ✅ Click detection fixed (raycast target + interactable logic)
- ✅ Tier requirement validation working correctly
- ⚠️ **Known Issue**: Filter buttons not functional (deferred to next session)

**Testing Results:**
- ✅ Cards display correctly with proper state colors
- ✅ Tier locks prevent premature purchases
- ✅ Purchase flow functional (click → confirm → money deducted)
- ✅ Rent contribution integrated and tested
- ⚠️ Upgrade effect application NOT yet tested (next session)

**Debug Keys:** F4-F10 for upgrade testing

---

## 📝 Next Session Priorities

1. **Fix Filter System** - Category filter buttons not working
2. **Test Upgrade Effects** - Verify actual gameplay impact of purchases
3. **Full Integration Test** - Purchase → Effect → Gameplay loop
4. **Polish** - Add purchase success feedback (animation/sound)
5. **Consider** - Tier-up celebration modal

---

## Phase 2: Economy Balancing 💰 PENDING

**Goal:** Ensure the game economy supports progression and feels fair

### 2.1 Starting Balance Adjustments

**Current Issues:**
- Starting money ($500) = exactly 1 month rent (no buffer, impossible to survive)
- Item profit margins unclear (need consistent margins across item types)
- No clear daily revenue targets (players don't know if they're doing well)

**Changes to Make:**
- [ ] **Increase starting money** in `GameManager.cs` to **$800-1000**
  - Allows Day 1 ordering with rent buffer
  - Test: Can player afford initial stock + survive until first revenue?

- [ ] **Balance item profit margins** via CSV update:
  - Small items: 50-80% profit margin (sell $5, restock $3 = $2 profit)
  - Medium items: 60-100% profit margin (sell $25, restock $15 = $10 profit)
  - Big items: 70-120% profit margin (sell $200, restock $130 = $70 profit)
  - Update `DungeonMart_Economy_Balance.csv` and re-import

- [ ] **Set daily revenue target**: $100-150/day minimum
  - Covers rent ($500/7 days = ~$71/day) + growth buffer
  - Test: Can player achieve this with 6-8 customers?

**Testing Checklist:**
- [ ] Full 7-day playthrough test:
  - [ ] Day 1: Order stock, restock shelves, serve customers
  - [ ] Days 2-6: Maintain stock, serve customers, earn profit
  - [ ] Day 7: Pay rent successfully with money left over
  - [ ] Track daily revenue and verify it meets $100-150 target
- [ ] Calculate profit per customer (should be $15-25 average)
- [ ] Verify player has enough money for rent + next week's orders

---

### 2.2 Dynamic Difficulty Scaling

**Goal:** Make progression feel rewarding but challenging

**Changes to Make:**
- [ ] **Modify `CustomerSpawner.cs`** - Base customer count on progression tier
  - Tier 1 (Street Vendor): 6 customers/day
  - Tier 2 (Shop Owner): 8 customers/day
  - Tier 3 (Merchant): 10 customers/day (requires "Extended Hours" upgrade)
  - Tier 4+ (Trade Baron): 12+ customers/day
  - Hook into `ProgressionManager.CurrentTier` property

- [ ] **Modify `ExpenseManager.cs`** - Scale rent based on shop size
  - Base rent: $500
  - +$100 per additional shelf unlocked (via upgrades)
  - Show rent breakdown in RentPaymentUI: "Base: $500, Expansions: +$200 = $700"
  - Hook into `UpgradeManager.OwnedUpgrades` count

**Integration Points:**
- [ ] `CustomerSpawner.CalculateCustomerCount()` method
  - Check `ProgressionManager.Instance.CurrentTier`
  - Check `UpgradeManager.Instance.HasUpgrade("ExtendedHours")`
  - Return adjusted customer count

- [ ] `ExpenseManager.CalculateRent()` method
  - Base rent + (shelf expansion upgrades * $100)
  - Cache result until next rent cycle

**Testing Checklist:**
- [ ] Customer count increases when reaching Tier 2
- [ ] Extended Hours upgrade adds +2 customers
- [ ] Rent increases when purchasing shelf expansions
- [ ] Rent breakdown UI displays correctly
- [ ] Balance check: Higher rent is offset by higher revenue from more customers

---

## Phase 3: UI/UX Enhancements 🎨 PENDING

**Goal:** Provide clear progression feedback and celebrate player achievements

### 3.1 Progression Feedback

**New UI Elements to Create:**
- [ ] **Persistent Progression Bar** (always visible, top-left corner)
  - Tier icon + tier name
  - Progress bar with percentage (67% to next tier)
  - Hover tooltip: "Lifetime Revenue: $2,347 / $3,000 | Days Played: 12 | Current Tier: Shop Owner"

- [ ] **Tier-Up Celebration Modal**
  - Full-screen overlay (semi-transparent dark background)
  - Large tier icon with glow effect
  - Title: "🎉 Congratulations! You've reached [Tier Name]!"
  - Subtitle: "You've unlocked new upgrades and opportunities"
  - List of newly unlocked upgrades (icons + names)
  - "Continue" button to close modal
  - Particle effect (confetti or sparkles)
  - Sound effect (fanfare, success jingle)

- [ ] **Enhanced End-of-Day Summary**
  - Add "Lifetime Revenue" stat
  - Add "Progress to next tier" stat
  - Highlight if player unlocked something today (NEW badge)
  - Show tier-up achievement if applicable

**Unity Editor Setup Instructions:**
- Create TierUpCelebrationUI panel (full-screen, disabled by default)
- Add particle system prefab (confetti emitter)
- Add TextMeshProUGUI components for title, subtitle, unlocks list
- Create animation: fade in + scale up for celebration modal
- Link sound effect to AudioManager trigger

**Testing Checklist:**
- [ ] Progress bar updates smoothly when earning money
- [ ] Tier-up celebration triggers at exact milestone
- [ ] Celebration modal displays correct tier info
- [ ] Unlocked upgrades list is accurate
- [ ] Particle effects and sound play correctly
- [ ] Modal can be dismissed with "Continue" button
- [ ] End-of-day summary shows lifetime revenue
- [ ] NEW badge appears for fresh unlocks

---

### 3.2 Upgrade Shop UI Polish

**Design Improvements:**
- [ ] Card-based layout with visual hierarchy
- [ ] Color-coded states:
  - Locked (gray, requires higher tier)
  - Available (green, can purchase now)
  - Owned (blue, already purchased)
  - Maxed Out (gold, repeatable upgrade at max level)
- [ ] Purchase confirmation dialog: "Buy [Upgrade Name] for $[Cost]? This will leave you with $[RemainingMoney]."
- [ ] Tooltips show detailed effect descriptions
- [ ] Visual feedback on purchase (sound, animation, success message)

**Testing Checklist:**
- [ ] Upgrade cards display correct colors for each state
- [ ] Tooltips appear on hover with full descriptions
- [ ] Purchase confirmation prevents accidental buys
- [ ] Success animation plays on purchase
- [ ] Sound effect plays on purchase
- [ ] Upgrade shop closes after purchase (or stays open with updated UI)

---

## Phase 4: Content Creation 📦 PENDING

**Goal:** Define progression tiers and create upgrade definitions

### 4.1 Define Progression Tiers (ScriptableObjects)

**Create 5 ProgressionDataSO assets:**
1. [ ] **Street Vendor** (Tier 0 - Starting Tier)
   - Required Revenue: $0 (starting state)
   - Description: "A humble beginning. Start building your merchant empire!"
   - Unlocks: Basic operations only

2. [ ] **Shop Owner** (Tier 1)
   - Required Revenue: $1,500
   - Description: "Your shop is gaining recognition in the local market."
   - Unlocks: Shelf Expansion Pack, Bulk Deals, Efficient Shelving Lv1

3. [ ] **Merchant** (Tier 2)
   - Required Revenue: $4,000
   - Description: "Adventurers from distant lands seek your wares."
   - Unlocks: Extended Hours, Efficient Shelving Lv2, Express Checkout Lv1

4. [ ] **Trade Baron** (Tier 3)
   - Required Revenue: $8,000
   - Description: "Your reputation as a merchant lord is spreading across the realm."
   - Unlocks: Auto-Restock Robot, Express Checkout Lv2, Premium Displays

5. [ ] **Tycoon** (Tier 4 - End-game)
   - Required Revenue: $15,000
   - Description: "You are the undisputed master of dungeon commerce!"
   - Unlocks: Second Floor Expansion, Exclusive Items, Legendary Status

**Unity Editor Setup:**
- Create folder: `Assets/Resources/Progression/`
- Create 5 ProgressionDataSO assets
- Assign tier icons (crown, star, diamond sprites)
- Write flavor text descriptions
- Link to UpgradeDataSO assets that unlock at each tier

---

### 4.2 Create Upgrade Definitions (ScriptableObjects)

**Create UpgradeDataSO assets (examples):**

1. [ ] **Shelf Expansion Pack** - $400
   - Category: Shelves
   - Effect: Unlock 2 new shelf slots
   - Tier Requirement: Shop Owner (Tier 1)
   - Repeatable: Yes (max 3 times)
   - Description: "Add more display space to your shop floor."

2. [ ] **Efficient Shelving Lv1** - $300
   - Category: Shelves
   - Effect: +2 items per shelf slot (from 5 to 7)
   - Tier Requirement: Shop Owner (Tier 1)
   - Repeatable: No
   - Description: "Clever stacking techniques allow more items per shelf."

3. [ ] **Efficient Shelving Lv2** - $600
   - Category: Shelves
   - Effect: +2 items per shelf slot (from 7 to 9)
   - Tier Requirement: Merchant (Tier 2)
   - Repeatable: No
   - Description: "Master-level organization maximizes shelf capacity."

4. [ ] **Express Checkout Lv1** - $500
   - Category: Operations
   - Effect: Customers move 25% faster at checkout
   - Tier Requirement: Merchant (Tier 2)
   - Repeatable: No
   - Description: "Streamlined payment process speeds up transactions."

5. [ ] **Express Checkout Lv2** - $800
   - Category: Operations
   - Effect: Customers move 50% faster at checkout (cumulative)
   - Tier Requirement: Trade Baron (Tier 3)
   - Repeatable: No
   - Description: "Instant payment magic shortens queues dramatically."

6. [ ] **Bulk Deals** - $600
   - Category: Operations
   - Effect: Order 5x items at once, 10% discount on restock cost
   - Tier Requirement: Shop Owner (Tier 1)
   - Repeatable: No
   - Description: "Volume discounts from suppliers reduce costs."

7. [ ] **Extended Hours** - $700
   - Category: CustomerFlow
   - Effect: +2 customers per day
   - Tier Requirement: Merchant (Tier 2)
   - Repeatable: Yes (max 2 times, +4 total)
   - Description: "Stay open longer to serve more adventurers."

8. [ ] **Auto-Restock Robot** - $1,200
   - Category: Operations
   - Effect: Morning deliveries automatically fill empty shelves
   - Tier Requirement: Trade Baron (Tier 3)
   - Repeatable: No
   - Description: "A magical construct handles tedious restocking for you."

**Unity Editor Setup:**
- Create folder: `Assets/Resources/Upgrades/`
- Create 8+ UpgradeDataSO assets
- Assign upgrade icons (sprites for each upgrade type)
- Write clear descriptions with exact numeric effects
- Set tier requirements correctly

---

## Phase 5: Testing & Balancing 🧪 PENDING

**Goal:** Playtest the full progression system and iterate on balance

### 5.1 Full Playthrough Test Scenarios

**Test 1: Sprint to Tier 2 (Target: 7-10 days)**
- [ ] Start new game with $800 starting money
- [ ] Play optimally: order wisely, restock efficiently, serve all customers
- [ ] Track daily revenue and expenses
- [ ] Goal: Reach Shop Owner (Tier 1, $1,500) within 7 days
- [ ] Goal: Reach Merchant (Tier 2, $4,000) within 10 days
- [ ] Verify rent payments don't block progression

**Test 2: Economic Viability Check**
- [ ] Can player afford rent on Day 7 without taking a loan?
- [ ] Can player afford first upgrade (Shelf Expansion $400) by Day 10?
- [ ] Does purchasing upgrades feel rewarding (tangible benefit)?
- [ ] Are upgrade costs balanced (not too cheap/expensive)?

**Test 3: Difficulty Curve Check**
- [ ] Tier 1 (6 customers): Easy, manageable, allows learning
- [ ] Tier 2 (8 customers): Moderate challenge, requires efficiency
- [ ] Tier 3 (10 customers): Hard, requires upgrades to handle demand
- [ ] Are shelves too full or too empty? Adjust spawn rates/capacity

**Test 4: Upgrade Impact Validation**
- [ ] Test each upgrade individually:
  - Shelf capacity increase: Can see more items on shelves?
  - Customer count increase: More customers spawn?
  - Checkout speed: Customers move faster at counter?
  - Bulk ordering: Can order 5x items at once with discount?
  - Auto-restock: Shelves auto-fill in morning phase?
- [ ] Verify upgrades feel impactful and worth the cost

**Test 5: Edge Cases**
- [ ] What happens if player takes loan at Tier 0? (Should be viable)
- [ ] Can player afford both loan payments AND upgrades?
- [ ] What if player ignores upgrades? (Should be harder but possible)
- [ ] Can player reach Tier 4 (Tycoon) within 30 days? (Target: 25-35 days)

---

### 5.2 Balance Iteration Points

**If progression feels too slow:**
- [ ] Reduce tier revenue requirements by 20%
- [ ] Increase item profit margins by 10-15%
- [ ] Increase starting money to $1000
- [ ] Reduce upgrade costs by 20%

**If progression feels too fast:**
- [ ] Increase tier revenue requirements by 20%
- [ ] Decrease item profit margins by 10%
- [ ] Increase upgrade costs by 30%
- [ ] Add more tiers (split Tier 2 into 2a and 2b)

**If upgrades feel weak:**
- [ ] Increase effect values (e.g., +3 items instead of +2)
- [ ] Reduce costs to encourage experimentation
- [ ] Add visual feedback for upgrade effects

**If difficulty spikes too hard:**
- [ ] Smooth customer count increases (6 → 7 → 8 instead of 6 → 8)
- [ ] Make early upgrades cheaper ($200 instead of $400)
- [ ] Increase grace period before rent scaling kicks in

---

## 📊 Expected Outcomes

### Player Experience Goals:
✅ **Clear "Why":** Players chase tier promotions and upgrades (positive motivation, not just survival)
✅ **Visible Progress:** Every sale contributes to visible progress bar (dopamine loop)
✅ **Strategic Depth:** Upgrade choices create meaningful trade-offs (capacity vs speed vs automation)
✅ **Celebration Moments:** Tier-ups feel like achievements (emotional peaks)
✅ **Replayability:** Different upgrade paths = different playstyles
✅ **Long-term Engagement:** 5 tiers × 4-7 days per tier = 20-35 hours of content

### Design Success Metrics:
- [ ] Player can articulate their current goal ("I'm 67% to Merchant rank!")
- [ ] Player makes deliberate upgrade choices (not random purchases)
- [ ] Player feels progression even on "bad" days (slow but steady progress bar)
- [ ] Player excited to reach next tier (anticipation of unlocks)
- [ ] Player replays to try different upgrade strategies

---

## 📁 Files to Create (Summary)

### Scripts:
- [ ] `Assets/Scripts/Singletons/ProgressionManager.cs`
- [ ] `Assets/Scripts/Singletons/UpgradeManager.cs`
- [ ] `Assets/Scripts/SOs/ProgressionDataSO.cs`
- [ ] `Assets/Scripts/SOs/UpgradeDataSO.cs`
- [ ] `Assets/Scripts/UI/ProgressionUI.cs`
- [ ] `Assets/Scripts/UI/UpgradeShopUI.cs`
- [ ] `Assets/Scripts/UI/TierUpCelebrationUI.cs`

### ScriptableObject Assets:
- [ ] 5 ProgressionDataSO assets (Tiers 0-4)
- [ ] 8+ UpgradeDataSO assets (upgrades for each tier)

### Files to Modify:
- [ ] `Assets/Scripts/Singletons/GameManager.cs` - Add lifetime revenue tracking
- [ ] `Assets/Scripts/Singletons/DayManager.cs` - Hook for tier celebration timing
- [ ] `Assets/Scripts/Singletons/ExpenseManager.cs` - Dynamic rent scaling
- [ ] `Assets/Scripts/Singletons/CustomerSpawner.cs` - Accept variable customer count
- [ ] `Assets/Scripts/Shelf.cs` - Support capacity upgrades
- [ ] `Assets/Scripts/Singletons/OrderManager.cs` - Support bulk ordering upgrades
- [ ] `Assets/Scripts/CheckoutCounter.cs` - Support checkout speed upgrades

---

## Future Implementation - Additional Features

### Code Quality & Optimization Tasks (From Architecture Review)

**Minor Concerns (Medium Priority):**
- [ ] **PlayerController.cs:46-58** - Optimize shelf search (currently runs every frame)
  - Option 1: Cache shelves array in Start() and refresh on level change
  - Option 2: Use trigger colliders (OnTriggerEnter/Exit) to detect nearby shelves automatically
  - Impact: Low priority for now (<10 shelves), but important for 50+ shelves

- [ ] **DeliveryBox.cs:19** - Replace tag-based player search with serialized field
  - Current: `GameObject.FindGameObjectWithTag("Player")` in Start()
  - Better: Add `[SerializeField] private PlayerController player;` and link in Inspector
  - Impact: More robust, less fragile (doesn't depend on tag being set)

**Optimizations to Consider (Low Priority):**
- [ ] **Shelf.cs:42-50** - Pre-generate slots in editor instead of procedural generation
  - Current: Slots created at runtime in Awake()
  - Benefit: Visual tweaking in editor, see slot layout before play mode
  - Note: Current approach works fine, this is polish only

- [ ] **Customer.cs:56-146** - Consider parallel customer behavior system
  - Current: Sequential shopping coroutine (works great!)
  - Future: Multiple customers browsing simultaneously (more realistic)
  - Note: Only needed if you want more dynamic/overlapping customer behavior

**Pre-Release Tasks:**
- [ ] Remove debug keys before browser release (M, K, I, O, 1-5) in DayManager.cs
- [ ] Remove debug inventory system or hide behind developer mode

**CSV importer tool optimization:**
- [ ] refactor the tool to generate folders when items are created segmenting items per size. 


### Nice to Have

- [ ] **Tutorial/Help:**
    - [ ] First-time instructions
    - [ ] Contextual hints based on phase

- [ ] **Settings Menu:**
  - [ ] Audio volume controls
  - [ ] Key binding remapping
  - [ ] Graphics settings
  - [ ] Save/load settings

- [ ] **Multiple item types:** Add more items (potions, armor, etc.)
- [ ] **Shop upgrades:** Additional shelves, faster checkout, more floors, auto-restocking, etc.
- [ ] **Difficulty progression:** More customers per day as game progresses
- [ ] **Customer patience system:** Customers leave if waiting too long
- [ ] **Special orders:** Customers request specific items, that can't be ordered, put in inventory but not shown in the shop. Customers will come back after a few days and ask for it at the checkout counter

## Known Issues

### 🐛 Game Over UI Input Blocking (HIGH PRIORITY)

**Status**: Under Investigation

**Symptoms**:
- Game Over screen appears visually correct
- All UI elements properly configured and visible
- Window loses fullscreen mode when Game Over triggers
- Mouse/keyboard input completely stops working
- All buttons non-clickable despite being `interactable=True` and `active=True`
- Unity play mode continues running (doesn't crash)
- Must manually stop play mode via Unity Editor

**Debug Evidence**:
```
✅ EventSystem found and enabled
✅ EventSystem GameObject active
✅ All buttons marked as interactable=True, active=True
✅ Panel activated successfully
✅ No exceptions or errors in console (after NullReferenceException fix)
❌ Input processing completely blocked
❌ Window loses fullscreen
```

**What's Been Tried**:
1. ~~Removed `PauseManager.PauseGame()` call (Time.timeScale = 0 blocking input)~~ - Didn't fix
2. ~~Added button delay mechanism with coroutine~~ - Coroutine never completed
3. ~~Fixed NullReferenceException when toggling EventSystem~~ - Fixed exception but input still blocked
4. ~~Removed button delay entirely, made buttons immediately interactable~~ - Input still blocked
5. ~~Added EventSystem.SetSelectedGameObject(null)~~ - Input still blocked

**Potential Causes**:
- Unity EventSystem input processing blocked during `Update()` chain execution
- Canvas/GraphicRaycaster configuration issue
- Multiple UI panels competing for input (RentPaymentUI, GameOverUI overlap)
- Unity Input System vs Legacy Input conflict
- Game window focus loss at OS level

**Investigation Areas**:
- Check Canvas render mode and sorting order settings in Unity Editor
- Verify GraphicRaycaster component on Canvas
- Check if multiple EventSystems exist in scene
- Test triggering Game Over outside of DayManager.Update() chain
- Check Unity Input System package configuration
- Investigate if other UI modals (RentPaymentUI, LoanUI) are still active

**Workaround**: None - Game Over state is unrecoverable, must restart Unity play mode

**Files Involved**:
- `Assets/Scripts/UI/GameOverUI.cs` - Main UI controller
- `Assets/Scripts/Singletons/FailStateManager.cs` - Game over trigger
- `Assets/Scripts/Singletons/LoanManager.cs` - Loan default trigger
- `Assets/Scripts/Singletons/DayManager.cs` - Update loop where Game Over happens

---

## Current Status

### ✅ Tracking of implementation:
- ✅ Core inventory system
- ✅ Ordering system with UI
- ✅ Day/Night cycle with three phases
- ✅ Customer wave system (fixed spawns per day)
- ✅ Delivery system (boxes spawn, player opens them)
- ✅ Phase restrictions (order menu only at end of day)
- ✅ Starting delivery boxes on Day 1 & proper day progression
- ✅ Main Menu Scene with Play, Settings (TBD), and Quit buttons
- ✅ Pause System with ESC key, pause menu overlay, and all buttons
- ✅ End of Day Summary Panel with statistics and continue button
- ✅ 2D Physics collision system (player blocked by walls, configurable spawn points)
- ✅ Customer Types & Corporate Humor (3 types, dialogue system, visual bubbles)
- ✅ Diverse Shelving System with item sizes and multi-item support
- ✅ Single Item Size Per Shelf Type restriction
- ✅ Restock UI System with item selection and size filtering
- ✅ **CSV Item Importer Tool**: Automated ItemDataSO generation from Excel/CSV spreadsheet
- ✅ **Monthly Expenses System**: Rent tracking, loan system with interest, fail states (KNOWN BUG: Game Over UI input blocked)
- ✅ **Visual Polish**: Customer visuals now use random SPUM character prefabs (48 variants)
- ✅ **Sound System**: Multi-AudioSource sound effects with gameplay and UI sounds
- ✅ **Music System**: Phase-based dynamic background music with smooth crossfades
- ✅ **Shelf System Refactor**: Replace grid-calculated slot positioning with inspector-assigned transform array for maximum flexibility in shelf design.

### 🎮 Current Gameplay Loop
1. **Morning:** Delivery boxes appear → Press E to open → Items to inventory → Restock shelves
2. **Business:** Press O → Different customer types spawn with unique behaviors → Browse shelves → Show dialogue → Collect 1-4 items → Checkout → Day auto-ends when done
3. **End of Day:** Summary panel shows stats → Click "Continue" → Press Tab → Order stock for tomorrow → Press M to advance
4. **Next Morning:** Repeat cycle (Day 2, 3, 4...)

### 👥 Customer Types
- **Quick Shopper**: Fast (4.5 speed), 1 item, impatient
- **Browser**: Slow (2 speed), 2-3 items, patient
- **Big Spender**: Medium (3 speed), 3-4 items, demanding
- **Visual Variety**: Each customer spawns with a random SPUM character model (48 unique variants)

### 🔧 Controls
- **WASD/Arrow Keys** - Move player (blocked by walls, disabled when UI open)
- **ESC** - Pause/unpause game (opens pause menu)
- **Tab** - Open order menu (end of day only)
- **E** - Interact (open delivery boxes, toggle restock UI near shelves)

### 🐛 Debug Controls
- **M** - Advance to next day (increments day counter and starts morning)
- **O** - Open shop (morning → business)
- **K** - Force end day (business → end of day)
- **I** - Add debug inventory (testing only)
- **1/2/3/5** - Time scale controls (1x, 2x, 3x, 5x speed)

### 🛠️ CSV Item Importer Tool

**Purpose:**
- Automate ItemDataSO ScriptableObject creation/updates from Excel spreadsheet
- Single source of truth for game economy balancing
- Preserve manually assigned sprites during updates

**Tool Features:**
- **Unity Editor Window**: Tools → DungeonMart → Import Items from CSV
- **CSV Parsing**: Reads `DungeonMart_Economy_Balance.csv` from Assets folder
- **Smart Updates**: Updates existing SOs without overwriting sprites
- **PascalCase Filenames**: "Health Potion" → `HealthPotion.asset`
- **Flexible Column Names**: Supports "Item Name", "ItemName", "Name", etc.
- **Batch Processing**: Imports all 23 items in seconds
- **Import Log**: Detailed feedback on created/updated items

**Technical Implementation:**
1. **ItemDataImporter.cs** (Editor Script):
   - Custom EditorWindow with GUI
   - CSV parsing with header detection
   - AssetDatabase integration for SO creation/updates
   - Regex-based PascalCase converter
   - Error handling and validation

**CSV Format:**
```csv
Item Name,Sell Price,Restock Cost,Size,Slots Required
Health Potion,5,3,Small,1
Iron Sword,25,15,Medium,2
Dragon Throne,200,130,Big,3
```

**Workflow:**
1. Edit Excel spreadsheet with balance changes
2. Export as CSV to `Assets/DungeonMart_Economy_Balance.csv`
3. In Unity: Tools → DungeonMart → Import Items from CSV
4. All items created/updated automatically
5. Add sprites manually in Inspector (preserved on re-import)

**Items Created (23 Total):**
- **Small Items (9)**: Health Potion, Mana Potion, Antidote Vial, Energy Drink, Cursed Ring, Lucky Amulet, Poison Dagger, Diamond Ring, Spell Scroll
- **Medium Items (8)**: Iron Sword, Wooden Shield, Crossbow, Leather Armor, Chainmail Vest, Steel Helmet, Enchanted Bow, Magic Staff
- **Big Items (6)**: Bear Trap, Wooden Throne, Spike Trap Kit, Demonic Statue, Dragon Skull, Golden Throne

**Benefits:**
- Rapid iteration on game economy
- No manual SO creation needed
- Easy to rebalance prices across all items
- Designer-friendly workflow (Excel → Unity)
- Sprite assignment workflow preserved

**Location:**
- Script: `Assets/Scripts/Editor/ItemDataImporter.cs`
- CSV: `Assets/DungeonMart_Economy_Balance.csv`
- Output: `Assets/Resources/Items/*.asset`

---

## Notes for Next Session

### 🆕 Phase 15: Restock UI System (TESTED & COMPLETE)

**What Was Implemented:**

**New Features:**
- **Interactive Item Selection**: Press E near shelf to open UI panel with filtered inventory
- **Size-Based Filtering**: Only items matching shelf's allowed size are shown
- **Visual Item Display**: Each button shows item icon, name, and quantity
- **Click to Restock**: Click item button to restock shelf with 1 item
- **Player Movement Control**: Movement disabled while UI is open
- **E Key Toggle**: Press E to open/close UI (no need for separate close button)

**New Scripts Created:**
1. **RestockUIManager.cs** (Singleton):
   - Manages restock UI panel visibility
   - Filters inventory by shelf's allowed item size
   - Dynamically spawns item buttons
   - Disables/enables player movement
   - Methods: `ShowRestockUI(shelf)`, `HideRestockUI()`, `IsUIOpen()`
   - Uses `FindFirstObjectByType<PlayerController>()` for movement control

2. **RestockItemButton.cs** (UI Component):
   - Individual button for each inventory item
   - Displays item icon (Image), name (TextMeshProUGUI), quantity (TextMeshProUGUI)
   - Handles click events via callback pattern
   - Method: `Setup(itemData, quantity, onClickCallback)`

**Code Changes:**
3. **PlayerController.cs** (Updated):
   - Added `canMove` boolean flag
   - Added `SetCanMove(bool)` public method for UI managers
   - Movement input blocked when `canMove = false`
   - E key now toggles UI (checks `IsUIOpen()` before opening)
   - Velocity set to zero when movement disabled

**UI Structure (Unity Editor):**
```
Canvas (Main UI Canvas)
└── RestockPanel [disabled by default]
    ├── TitleText (TextMeshProUGUI)
    ├── ItemScrollView (Scroll View)
    │   └── Viewport
    │       └── Content (Vertical Layout Group + Content Size Fitter)
    │           └── [RestockItemButton prefabs spawned here]
    ├── CloseButton (Button)
    └── MessageText (TextMeshProUGUI) [optional - "No compatible items"]
```

**RestockItemButton Prefab Structure:**
```
RestockItemButton (Button + Horizontal Layout Group)
├── ItemIcon (Image)
├── ItemNameText (TextMeshProUGUI)
└── QuantityText (TextMeshProUGUI)
```

**User Experience Flow:**
1. Player walks near shelf → Press E
2. UI opens, shows only compatible items (filtered by size)
3. Player clicks item → Shelf restocks, inventory decreases, UI closes
4. OR player presses E again → UI closes without restocking
5. OR player clicks Close button → UI closes

**Technical Highlights:**
- **TextMeshPro Support**: All UI text uses TMP for better rendering
- **LINQ Filtering**: Uses `.Where()` to filter inventory by item size
- **Event-Driven**: Callback pattern for button clicks
- **Singleton Pattern**: Consistent with existing managers
- **Movement Control**: PlayerController exposes public API for external control
- **Auto-Cleanup**: Destroys spawned buttons on UI close

**Design Benefits:**
- **User Agency**: Players choose which item to restock (no auto-select)
- **Clear Feedback**: Visual confirmation of available items and quantities
- **Error Prevention**: Size filtering prevents invalid restock attempts
- **Intuitive Controls**: E key toggles UI (consistent with "interact" pattern)
- **Non-Blocking**: UI-only pause (doesn't stop entire game like pause menu)

**UX Improvements Implemented:**
- Player movement disabled while UI open (prevents accidental movement)
- E key toggles UI (no need to click close button)
- UI auto-closes on successful restock
- Empty inventory shows message instead of blank screen

**Tested Scenarios:**
- ✅ Size filtering (Small/Medium/Big shelves show correct items)
- ✅ Multiple item types in inventory
- ✅ Empty inventory handling
- ✅ Full shelf behavior
- ✅ E key toggle open/close
- ✅ Player movement disabled when UI open
- ✅ Multiple shelves of same size
- ✅ Inventory depletion (buttons disappear when quantity = 0)

---

### 🔧 Shelf System Refactor: Transform-Based Positioning (COMPLETED)

**What Was Changed:**

**Goal**: Replace grid-calculated slot positioning with inspector-assigned transform array for maximum flexibility in shelf design.

**Problem Solved:**
- Previous system used procedural grid calculations (rows, columns, spacing)
- All shelves forced into regular grid patterns (horizontal/vertical only)
- No support for irregular shelf shapes (curved, diagonal, asymmetric displays)
- Designer couldn't visually position slots without code changes

**New Architecture:**
- **Inspector-Assigned Transform Array**: Each shelf has explicit array of Transform references
- **Visual Editor Workflow**: Designers create empty child GameObjects and position them where items should spawn
- **Per-Shelf Customization**: Each shelf instance can have completely unique slot layouts
- **Irregular Shape Support**: Corner shelves, curved displays, asymmetric arrangements fully supported

**Code Changes:**

1. **ShelfTypeDataSO.cs** (Simplified):
   - **Removed**: `totalSlots`, `slotSpacing`, `slotsPerRow`, `horizontalLayout` fields
   - **Removed**: `GetSlotPosition()` method (no longer needed)
   - **Kept**: `allowedItemSize` for item size validation
   - **Kept**: `itemScale` for display settings

2. **Shelf.cs** (Refactored):
   - **Added**: `Transform[] slotPositions` inspector array
   - **Removed**: `slotsContainer` field (no longer needed)
   - **Updated**: `InitializeShelf()` now loops through assigned transforms instead of creating new GameObjects
   - Validates `slotPositions` array is populated
   - Adds `ShelfSlot` components to existing transforms
   - Handles null transforms gracefully with warnings

3. **ShelfSlot.cs** (No Changes):
   - Already used transform-based positioning
   - Stacking system continues to work with local positions
   - Fully compatible with new approach

**Unity Editor Workflow:**
1. Create empty child GameObjects under shelf (e.g., "SlotPos_0", "SlotPos_1")
2. Position them visually where items should appear
3. In Shelf component inspector, set array size and drag transforms into slots
4. Visual and explicit - no hidden logic or naming conventions

**Design Benefits:**
- ✅ **Visual Freedom**: Create any shelf shape (curved, diagonal, irregular)
- ✅ **Per-Shelf Customization**: Each shelf instance has unique layout
- ✅ **Designer-Friendly**: No code changes needed for new designs
- ✅ **Clear and Explicit**: Transform references visible in inspector
- ✅ **Flexible Stacking**: Multiple items per slot still work perfectly

**Breaking Changes:**
- Existing shelves need slot position transforms created and assigned
- ScriptableObject fields removed (totalSlots, slotSpacing, etc.)
- Shelves without assigned transforms will log errors and fail to initialize

**Backward Compatibility:**
- ShelfSlot stacking behavior unchanged
- RestockShelf() and TakeItem() APIs unchanged
- Item size validation unchanged

**Files Modified:**
- `Assets/Scripts/SOs/ShelfTypeDataSO.cs` - Removed grid calculation fields
- `Assets/Scripts/Shelf.cs` - Added transform array, refactored initialization

---
