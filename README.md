# Dungeon Mart 3D - README

## **Game Design Foundation**

**Core Design Questions Answered:**

1. **What is the player doing?** TO BE ANSWERED

2. **What is stopping them?** TO BE ANSWERED

3. **Why are they doing it?** TO BE ANSWERED

---

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
- ✅ Category Filter System: Upgrade shop now has 4 category filters (Shop Expansion, Shelves, Operations, Customer Flow) for better organization
- ✅ Item Category System: Replaced size-based item system (Small/Medium/Big) with flexible category system (Weapons, Shields, Potions, Armor & Apparel, Traps, Magic Items) - supports multiple categories per shelf, hybrid unlock system (category unlocks via upgrades + tier-based item gating), starting categories (Weapons, Shields, Potions) unlocked by default
- ✅ ~~License Upgrade System~~ (REMOVED): Previously had "Licenses" category with 3 license upgrades - replaced by objective-based progression where category unlocks now happen through completing objectives instead of purchasing upgrades
- ✅ Objective-Based Progression System: Completely replaced tier-based progression with parallel objective tracking - 5 objective types (Revenue, CustomersServed, ItemsSold, DaysPlayed, Hybrid), category-specific item tracking, prerequisite system for objectives and upgrades, reveal conditions (AlwaysVisible, AfterObjectiveCount, AfterSpecificObjective), dark humor completion messages, debug keys F11/F12 for testing, ItemCategory.None added for non-filtered objectives
- ✅ Objectives Panel UI System: Full-featured objectives panel with ObjectivesPanelUI managing display/filtering, ObjectiveCard prefab for individual objective display with progress bars and completion states, 3 filter buttons (All/InProgress/Completed), HUD integration with always-enabled Objectives button, dynamic progress text formatting by objective type (Revenue shows $X/$Y, CustomersServed shows X/Y Customers, ItemsSold shows X/Y [Category] Sold), color-coded states (gray for in-progress, dark green for completed), gold checkmark badge for completed objectives, "Unlocks: [upgrade]" text display - UI scripts complete and ready for Unity Editor setup
- ✅ Active Restocking & Fast Pacing: Eliminated dead time during business phase by compressing duration from 3-5 minutes to 90-120 seconds: customer spawn interval reduced from 3s→1.5s, checkout time reduced from 2s→1s, browse time reduced to 0.5-1s random (full shelves) and 0.25-0.5s (empty shelves); NavMeshAgent obstacle avoidance disabled (NoObstacleAvoidance) allowing customers to walk through each other without blocking while still respecting NavMesh-baked static obstacles; active restocking enabled during business phase with no phase restrictions
- ✅ Item Database & Shelf System Expansion: Created comprehensive 35-item database across 6 categories with balanced 3-tier pricing (Early $5-25, Mid $30-75, Late $80-150) and 40% profit margins; Updated CSV importer tool (DungeonMart_Items_v2.csv format) with description field support and prefab auto-linking; Created PlaceholderItemGenerator editor tool for color-coded primitive prefabs (Red=Weapons, Blue=Shields, Green=Potions, Orange=Armor, Purple=Traps, Cyan=Magic); Designed 6 shelf types including multi-category shelves (DefenseWall holds Shields+Armor, GeneralShelf holds Weapons+Shields+Potions); Category-based unlocking system with 18 starting items (Weapons/Shields/Potions) and 17 objective-unlocked items (Armor/Traps/Magic categories unlocked through progression)
- ✅ Asset Organization & Shelf Prefabs: Reorganized item assets into category-based folder structure for better project management, added new shelf type prefabs
- ✅ UI Layering Fix: Corrected render order so dialogue bubbles and quantity badges now properly render behind UI panels (pause menu, order menu)
- ✅ Dynamic Customer Spawn Intervals: Added configurable min/max spawn interval range (1.0s-2.0s default) with randomization for more natural customer flow
- ✅ Phase Progression Button: Added always-visible HUD button with dynamic text ("Open Shop"/"Close Shop"/"Next Day") that intelligently progresses through game phases - stops customer spawning during business phase and waits for all customers to finish before ending day, enabling complete mouse-only gameplay
- ✅ Item Tier System: Implemented 3-tier quality progression system (Tier 1: $1-29 Cheap/Starting, Tier 2: $30-79 Normal/Mid-game, Tier 3: $80+ Premium/Late-game) - tiers unlock via objectives independent of category unlocks, creating hybrid progression where items require both category AND tier unlock; ItemDataSO.tier field with auto-assignment based on sell price; SupplyChainManager tracks currentUnlockedTier with filtering in AvailableItems; ObjectiveDataSO.unlocksTier field (0-3) triggers SupplyChainManager.UnlockTier() on completion; ItemTierAssigner editor tool (Tools → DungeonMart → Assign Item Tiers) batch-updates all items; Debug key U unlocks next tier for testing; combines with objective-based category unlocking for dual-gate item progression

---

## Planned Implementation - Additional Features

### Add Progression Juice (Celebration System)
**Solution:** Add celebration modals, particles, and feedback

### Balance for 15-20 Min Completion
**Solution:** Aggressive rebalancing for quick prototype testing

### Victory Screen
**Solution:** Victory modal on all objectives completed

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
  - [ ] Key binding remapping
  - [ ] Graphics settings
  - [ ] Save/load settings

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

**3. Restocking mechanic and game play should be redone. There is no way to say where we want to restock. The player has to click through it and can't come back if he went too far.**


---

## Current Status

### 🎮 Current Gameplay Loop
1. **Morning:** Delivery boxes appear → Click boxes to open → Items to inventory → Click shelves to restock → Click "Open Shop" button
2. **Business:** Different customer types spawn with unique behaviors → Browse shelves → Show dialogue → Collect 1-4 items → Checkout → Click "Close Shop" button when ready (stops new customers, waits for existing to finish)
3. **End of Day:** Summary panel shows stats → Click "Continue" → Click "Orders" button to place orders → Click "Upgrades" button to purchase upgrades → Click "Next Day" button
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
- **Phase Progress Button** - Click to progress through day phases (shows "Open Shop"/"Close Shop"/"Next Day" based on current phase, always enabled)
- **Orders Button** - Click to open order menu (visible always, only clickable during end of day phase)
- **Upgrades Button** - Click to open upgrades shop (visible always, only clickable during end of day phase)
- **Objectives Button** - Click to open objectives panel (always enabled)

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

**Item Tier Testing:**
- **U** - Unlock next tier (cycles 1→2→3, test tier progression)

**Inventory Testing:**
- **I** - Add debug inventory items (general testing)

### 🛠️ CSV Item Importer Tool

**Purpose:**
- Automate ItemDataSO ScriptableObject creation/updates from Excel spreadsheet
- Single source of truth for game economy balancing
- Preserve manually assigned sprites during updates

**Tool Features:**
- **Unity Editor Window**: Tools → DungeonMart → Import Items from CSV
- **CSV Parsing**: Reads `DungeonMart_Items_v2.csv` from Assets folder
- **Smart Updates**: Updates existing SOs without overwriting sprites
- **PascalCase Filenames**: "Health Potion" → `HealthPotion.asset`
- **Flexible Column Names**: Supports "Item Name", "ItemName", "Name", etc.
- **Prefab Auto-Linking**: Automatically links prefabs from Assets/Prefabs/Items/
- **Batch Processing**: Imports all 35 items in seconds
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
ItemName,Category,SellPrice,RestockCost,UnlockedByDefault,PrefabName,Description
Health Potion,Potions,5,3,TRUE,HealthPotion,The classic red juice
Iron Sword,Weapons,25,15,TRUE,IronSword,Classic adventurer's choice
Crown of Kings,ArmorApparel,150,90,FALSE,CrownOfKings,Assert dominance
```

**Workflow:**
1. Edit Excel spreadsheet with balance changes
2. Export as CSV to `Assets/DungeonMart_Items_v2.csv`
3. In Unity: Tools → DungeonMart → Generate Placeholder Item Prefabs (first time only)
4. In Unity: Tools → DungeonMart → Import Items from CSV
5. All items created/updated automatically with prefabs linked
6. Add custom 3D models later (optional - placeholders work for prototyping)

**Items Created (35 Total):**
- **Weapons (7)**: Rusty Dagger, Wooden Staff, Iron Sword, Steel Axe, Crossbow, Enchanted Blade, Legendary Warhammer
- **Shields (5)**: Wooden Buckler, Iron Shield, Tower Shield, Enchanted Shield, Dragon Scale Shield
- **Potions (6)**: Minor Health Potion, Minor Mana Potion, Health Potion, Mana Potion, Greater Health Potion, Elixir of Power
- **Armor & Apparel (6)**: Leather Tunic, Iron Helmet, Chainmail Vest, Plate Armor, Enchanted Robes, Crown of Kings
- **Traps (5)**: Spike Trap, Net Trap, Bear Trap, Poison Gas Trap, Death Trap
- **Magic Items (6)**: Scroll of Fireball, Ring of Protection, Amulet of Life, Crystal Ball, Staff of Power, Dragon Egg

**Benefits:**
- Rapid iteration on game economy
- No manual SO creation needed
- Easy to rebalance prices across all items
- Designer-friendly workflow (Excel → Unity)
- Sprite assignment workflow preserved

**Location:**
- Scripts: `Assets/Scripts/Editor/ItemDataImporter.cs`, `Assets/Scripts/Editor/PlaceholderItemGenerator.cs`
- CSV: `Assets/DungeonMart_Items_v2.csv`
- Output: `Assets/Resources/Items/*.asset` (ItemDataSO files)
- Prefabs: `Assets/Prefabs/Items/*.prefab` (35 color-coded placeholder prefabs)

---
