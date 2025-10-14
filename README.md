# DungeonMart - TODO List

## Next Session Tasks

### **Economy & Progression Balancing (RECOMMENDED NEXT)**

**Goal**: Balance the core gameplay loop with proper economic incentives and progression

**Why This Phase**: Before adding more features, we need to ensure the current game loop is fun and rewarding. Players need clear goals, progression feedback, and meaningful choices.

**Requirements**:
- [ ] Balance item prices (sell price vs restock cost → profit margin)
- [ ] Set starting money amount (enough for Day 1 orders)
- [ ] Add "target earnings" goals per day (progression milestones)
- [ ] Test full gameplay loop (order → restock → sell → profit → next day)
- [ ] Create dynamic customer spawn rates for balanced difficulty

**Why Important**:
- No clear goals → players don't know if they're doing well
- Economic balance affects all future features
- Foundation for upgrades/expansions system

---

### Alternative: Visual Polish & Juice

**Goal**: Make the game feel more alive and responsive

**Requirements**:
- [ ] Better item sprites (replace circles with actual item art)
- [ ] Particle effects when opening delivery boxes
- [ ] Empty shelf visual indicators (highlight or UI prompt)
- [ ] Smooth camera following player
- [ ] Item pickup animations
- [ ] Customer satisfaction visual feedback

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
