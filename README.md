# DungeonMart3D - README

## ✅ Tracking of implementation, chronological order
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
- ✅ 3D Physics with NavMesh pathfinding (player movement on XZ plane, customer AI navigation, configurable spawn points)
- ✅ Customer Types & Corporate Humor (3 types, dialogue system, visual bubbles)
- ✅ Diverse Shelving System with item sizes and multi-item support
- ✅ Single Item Size Per Shelf Type restriction
- ✅ Restock UI System with item selection and size filtering
- ✅ CSV Item Importer Tool: Automated ItemDataSO generation from Excel/CSV spreadsheet
- ✅ Monthly Expenses System: Rent tracking, loan system with interest, fail states (KNOWN BUG: Game Over UI input blocked)
- ✅ Visual Polish**: Customer visuals now use random SPUM character prefabs (48 variants)
- ✅ Sound System**: Multi-AudioSource sound effects with gameplay and UI sounds
- ✅ Music System**: Phase-based dynamic background music with smooth crossfades
- ✅ Shelf System Refactor**: Replace grid-calculated slot positioning with inspector-assigned transform array for maximum flexibility in shelf design
- ✅ Managers Refactor: Merged 3 Managers (expense, loan, failstate) → 1 Unified Manager (financial)
- ✅ Debug Input System: Centralized DebugInputManager with compilation directives for automatic release build exclusion
- ✅ 2D to 3D Conversion: Complete transformation from 2D orthographic to 3D perspective top-down gameplay with NavMesh pathfinding, WebGL-optimized rendering
- ✅ Item System Refactor: Converted from 2D sprites to 3D models - ItemDataSO now carries prefab reference (data-driven design), Item.cs simplified to pure data container, visual setup handled by prefab structure
- ✅ Progression System: Lifetime revenue tracking, tier-based milestones (5 tiers: Street Vendor → Tycoon), persistent progress UI
- ✅ Upgrade Shop System: Card-based UI, purchase flow, tier-locked upgrades, dynamic rent contribution (8 upgrades for tiers 1-3)
- ✅ Upgrade System Testing (Phase 1.3): All upgrade effects verified and working - shop segment unlocking, shelf capacity increases, customer count bonuses, rent contribution system, full integration testing, edge case handling
- ✅ Rent UI Dynamic Updates: RentCountdownUI now subscribes to OnSegmentUnlocked event for immediate rent display updates when shop expands
- ✅ Mouse-Based Interaction System: Transitioned from WASD keyboard controls to fully mouse-based point-and-click gameplay - click shelves to restock, click delivery boxes to open, hover feedback with pink (#FF6B9D) outline and scale pulse effect
- ✅ Customer Animation System: Component-based animation controller for customer walk/idle states - attaches to visual prefabs, automatically finds NavMeshAgent in parent hierarchy, velocity-based animation switching with configurable threshold
- ✅ Quantity Badge System: Replaced visual item stacking with quantity badges - single item display per slot with badge overlay showing "x2", "x3", etc. for multiple items, badge auto-hides when count ≤ 1, follows DialogueBubble pattern with world-to-screen space conversion, auto-finds canvas at runtime (no manual inspector assignment needed)
- ✅ HUD Button System: Transitioned from keyboard-only to clickable HUD buttons for orders and upgrades - HUDButtonManager manages phase-based button enabling (both buttons only active during EndOfDay), buttons positioned in bottom-right corner with visual feedback (grayed out when disabled), removed Tab key shortcut for orders, removed ESC menu access to upgrades, singleton pattern added to OrderMenu for external access
- ✅ Category Filter System: Upgrade shop now has 5 category filters (Shop Expansion, Shelves, Operations, Customer Flow, Licenses) for better organization
- ✅ Item Category System: Replaced size-based item system (Small/Medium/Big) with flexible category system (Weapons, Shields, Potions, Armor & Apparel, Traps, Magic Items) - supports multiple categories per shelf, hybrid unlock system (category unlocks via upgrades + tier-based item gating), starting categories (Weapons, Shields, Potions) unlocked by default
- ✅ License Upgrade System: Added "Licenses" category to upgrade shop with 3 license upgrades (Armor & Apparel License $300/Tier 1, Trap Merchant Permit $500/Tier 2, Arcane Items Certification $800/Tier 3) - purchasing licenses unlocks new item categories in order menu
- ✅ Objective-Based Progression System: Completely replaced tier-based progression with parallel objective tracking - 5 objective types (Revenue, CustomersServed, ItemsSold, DaysPlayed, Hybrid), category-specific item tracking, prerequisite system for objectives and upgrades, reveal conditions (AlwaysVisible, AfterObjectiveCount, AfterSpecificObjective), dark humor completion messages, debug keys F11/F12 for testing, ItemCategory.None added for non-filtered objectives
- ✅ Objectives Panel UI System: Full-featured objectives panel with ObjectivesPanelUI managing display/filtering, ObjectiveCard prefab for individual objective display with progress bars and completion states, 3 filter buttons (All/InProgress/Completed), HUD integration with always-enabled Objectives button, dynamic progress text formatting by objective type (Revenue shows $X/$Y, CustomersServed shows X/Y Customers, ItemsSold shows X/Y [Category] Sold), color-coded states (gray for in-progress, dark green for completed), gold checkmark badge for completed objectives, "Unlocks: [upgrade]" text display - UI scripts complete and ready for Unity Editor setup
- ✅ Prototype Phase 1: Active Restocking & Fast Pacing - Eliminated dead time during business phase by compressing duration from 3-5 minutes to 90-120 seconds: customer spawn interval reduced from 3s→1.5s, checkout time reduced from 2s→1s, browse time reduced to 0.5-1s random (full shelves) and 0.25-0.5s (empty shelves); Shelf.cs enhanced with CapacityPercentage property and urgency events (OnLowStock <30%, OnStockNormal, OnShelfEmpty) for real-time feedback; ShelfUrgencyVisual.cs component provides red glow material feedback on low stock shelves; SoundType.ShelfEmpty audio alert when shelves empty; NavMeshAgent obstacle avoidance disabled (NoObstacleAvoidance) allowing customers to walk through each other without blocking while still respecting NavMesh-baked static obstacles; active restocking enabled during business phase with no phase restrictions


## 🎯 ACTIVE IMPLEMENTATION: Growth/Tycoon Progression System

### **Game Design Foundation**

**Core Design Questions Answered:**

1. **What is the player doing?** TO BE ANSWERED

2. **What is stopping them?** TO BE ANSWERED

3. **Why are they doing it?** TO BE ANSWERED


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

### Test Not Yet Implemented Effects ⚠️ INFO ONLY

**These upgrade effects have TODO placeholders and will NOT work yet:**

1. **Checkout Speed Decrease** (`DecreaseCheckoutTime`)
   - File: `UpgradeManager.cs:207-210`
   - TODO: Implement checkout speed modifications
   - Requires: CheckoutCounter modifications

2. **Bulk Ordering** (`EnableBulkOrdering`)
   - File: `UpgradeManager.cs:212-215`
   - TODO: Implement bulk ordering feature
   - Requires: OrderManager/SupplyChainManager modifications

3. **Auto-Restock** (`EnableAutoRestock`)
   - File: `UpgradeManager.cs:217-220`
   - TODO: Implement auto-restock feature
   - Requires: DeliveryManager/SupplyChainManager modifications

**Action Required:** Skip testing these effects for now, mark as "PENDING IMPLEMENTATION"

---

## Phase 2: Economy Balancing 💰 PENDING

**Goal:** Ensure the game economy supports progression and feels fair

---

## Phase 3: UI/UX Enhancements 🎨 PENDING

**Goal:** Provide clear progression feedback and celebrate player achievements

---

## Phase 4: Content Creation 📦 PENDING

**Goal:** Define progression tiers and create upgrade definitions

---

## Phase 5: Testing & Balancing 🧪 PENDING

**Goal:** Playtest the full progression system and iterate on balance


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

- [ ] **Customer.cs:56-146** - Consider parallel customer behavior system
  - Current: Sequential shopping coroutine (works great!)
  - Future: Multiple customers browsing simultaneously (more realistic)
  - Note: Only needed if you want more dynamic/overlapping customer behavior

**Pre-Release Tasks:**
- [ ] Verify debug code excluded from WebGL builds (check build log for DebugInputManager exclusion)

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

### 🐛 HIGH PRIORITY

**1. Game Over UI Input Blocking**
- **Status**: Under Investigation
- **Symptoms**: Game Over screen appears correctly but all input stops working, window loses fullscreen, buttons non-clickable
- **Workaround**: None - must restart Unity play mode
- **Files**: `GameOverUI.cs`, `FailStateManager.cs`, `LoanManager.cs`, `DayManager.cs`

### ⚠️ MEDIUM PRIORITY

**2. Not Yet Implemented Upgrade Effects**
- **Status**: Placeholder TODOs in UpgradeManager.cs
- **Pending Implementation**: Checkout speed decrease, bulk ordering, auto-restock
- **Implemented & Tested**: Shop segment unlocking ✅, shelf capacity increases ✅, customer count bonuses ✅
- **Files**: `UpgradeManager.cs:207-220`

---

## Current Status

### 🎮 Current Gameplay Loop
1. **Morning:** Delivery boxes appear → Click boxes to open → Items to inventory → Click shelves to restock
2. **Business:** Press O → Different customer types spawn with unique behaviors → Browse shelves → Show dialogue → Collect 1-4 items → Checkout → Day auto-ends when done
3. **End of Day:** Summary panel shows stats → Click "Continue" → Click "Orders" button to place orders → Click "Upgrades" button to purchase upgrades → Press M to advance
4. **Next Morning:** Repeat cycle (Day 2, 3, 4...)

### 👥 Customer Types
- **Quick Shopper**: Fast (4.5 speed), 1 item, impatient
- **Browser**: Slow (2 speed), 2-3 items, patient
- **Big Spender**: Medium (3 speed), 3-4 items, demanding
- **Visual Variety**: Each customer spawns with a random SPUM character model (48 unique variants)

### 🔧 Controls
- **Mouse Click** - Interact with objects (click shelves to restock, click delivery boxes to open, click HUD buttons)
- **Mouse Hover** - Visual feedback on interactive objects (pink outline with subtle pulse animation)
- **ESC** - Pause/unpause game (opens pause menu with Resume, Return to Main Menu, and Quit options)
- **Orders Button** - Click to open order menu (visible always, only clickable during end of day phase)
- **Upgrades Button** - Click to open upgrades shop (visible always, only clickable during end of day phase)

### 🐛 Debug Controls

**Note:** All debug keys are managed by `DebugInputManager.cs`, which is wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD` compilation directives. These controls will automatically be excluded from release builds.

**Day Management:**
- **M** - Advance to next day (increments day counter, starts morning phase)
- **O** - Open shop (force morning → business transition)
- **K** - Force end day (force business → end of day transition)
- **1/2/3/5** - Time scale controls (1x, 2x, 3x, 5x speed)

**Money & Progression Testing:**
- **7** - Add $500 (test tier 1 unlocks)
- **8** - Add $1,500 (test tier 2 unlocks)
- **9** - Add $5,000 (test tier 3+ unlocks)

**Upgrade System Testing:**
- **F4** - Unlock segment 1 (test shop expansion)
- **F5** - Unlock segment 2 (test shop expansion)
- **F6** - Unlock segment 3 (test shop expansion)
- **F7** - Add +2 shelf capacity bonus (test Efficient Shelving upgrade)
- **F8** - Add +2 bonus customers (test Extended Hours upgrade)
- **F9** - Log rent contribution breakdown (debug rent calculation)
- **F10** - Pay rent immediately (test rent payment system)

**Objective System Testing:**
- **F11** - Complete next incomplete objective (test objective completion flow)
- **F12** - Add 10 items sold to random category (test item tracking)

**Inventory Testing:**
- **I** - Add debug inventory items (general testing)

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
