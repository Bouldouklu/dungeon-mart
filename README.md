# Dungeon Mart 3D - README

## Potential Additional Features

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

### ⚠️ MEDIUM PRIORITY

**1. Not Yet Implemented Upgrade Effects**
- **Status**: Auto-restock placeholder remains in UpgradeManager.cs
- **Pending Implementation**: Auto-restock (not yet designed)
- **Implemented & Tested**: Shop segment unlocking ✅, customer count bonuses ✅, auto-scaling customers with shop expansion ✅, express checkout ✅, bulk ordering ✅
- **Removed**: Shelf capacity increases (removed with 1-item-per-slot redesign)
- **Files**: `UpgradeManager.cs:252-255`

**2. Restocking mechanic and game play should be redone. There is no way to say where we want to restock. The player has to click through it and can't come back if he went too far.**


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
- **F7** - Add +1 bonus customer (test Extended Hours upgrade effect)
- **F8** - Add +2 bonus customers (faster testing of customer scaling)
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
