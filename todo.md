# DungeonMart - TODO List

## Next Session Tasks

### **Phase 16 - Economy & Progression Balancing (RECOMMENDED NEXT)**

**Goal**: Balance the core gameplay loop with proper economic incentives and progression

**Why This Phase**: Before adding more features, we need to ensure the current game loop is fun and rewarding. Players need clear goals, progression feedback, and meaningful choices.

**Requirements**:
- [ ] Add money/earnings display to HUD (persistent UI)
- [ ] Add day counter display to HUD
- [ ] Balance item prices (sell price vs restock cost → profit margin)
- [ ] Set starting money amount (enough for Day 1 orders)
- [ ] Add "target earnings" goals per day (progression milestones)
- [ ] Test full gameplay loop (order → restock → sell → profit → next day)
- [ ] Adjust customer spawn rates for balanced difficulty
- [ ] Add visual feedback when earning money (text popup or sound)

**Why Important**:
- Currently no visible money counter → players can't track progress
- No clear goals → players don't know if they're doing well
- Economic balance affects all future features
- Foundation for upgrades/expansions system

---

### Alternative: Phase 16 - Visual Polish & Juice

**Goal**: Make the game feel more alive and responsive

**Requirements**:
- [ ] Better item sprites (replace circles with actual item art)
- [ ] Particle effects when opening delivery boxes
- [ ] Sound effects (cash register, item pickup, customer satisfaction)
- [ ] Empty shelf visual indicators (highlight or UI prompt)
- [ ] Smooth camera following player
- [ ] Item pickup animations
- [ ] Customer satisfaction visual feedback

---

### Next Implementation - Additional Features



- [ ] **Visual Polish:**
  - [ ] Better delivery box visuals
  - [ ] Particle effects when opening boxes
  - [ ] Sound effects (optional)
  - [ ] Empty shelf indicators


### Bug Fixes & UX Improvements

**✅ Completed:**
- [x] **Customer.cs dialogue system** - Fixed dialogue timing for empty shelves
  - **Solution**: Reordered logic to check shelf status BEFORE showing dialogue
  - **Now**: Customers show disappointed dialogue immediately at each empty shelf
  - **Also**: Added validation warnings for missing dialogue arrays in ScriptableObjects
  - **Location**: Customer.cs:76-103 (shopping routine with context-aware dialogues)

- [x] **RestockUIManager.cs** - Implemented multiple restock clicks without closing UI
  - **Solution**: Replaced `HideRestockUI()` with `RefreshUI()` on successful restock
  - **Now**: UI stays open, updates inventory counts after each click
  - **Result**: Much faster restocking workflow, players close UI manually with E or Close button
  - **Location**: RestockUIManager.cs:138-153

---

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

---

### Future Enhancements (Nice to Have)

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

## Current Status

### ✅ Completed (Phase 1-15)
- ✅ Phase 1: Core inventory system
- ✅ Phase 2: Ordering system with UI
- ✅ Phase 3: Day/Night cycle with three phases
- ✅ Phase 4: Customer wave system (fixed spawns per day)
- ✅ Phase 5: Delivery system (boxes spawn, player opens them)
- ✅ Phase 6: Phase restrictions (order menu only at end of day)
- ✅ Phase 7: Starting delivery boxes on Day 1 & proper day progression
- ✅ Phase 8: Main Menu Scene with Play, Settings (TBD), and Quit buttons
- ✅ Phase 9: Pause System with ESC key, pause menu overlay, and all buttons
- ✅ Phase 10: End of Day Summary Panel with statistics and continue button
- ✅ Phase 11: 2D Physics collision system (player blocked by walls, configurable spawn points)
- ✅ Phase 12: Customer Types & Corporate Humor (3 types, dialogue system, visual bubbles)
- ✅ Phase 13: Diverse Shelving System with item sizes and multi-item support
- ✅ Phase 14: Single Item Size Per Shelf Type restriction
- ✅ Phase 15: Restock UI System with item selection and size filtering

### 🎮 Current Gameplay Loop
1. **Morning:** Delivery boxes appear → Press E to open → Items to inventory → Restock shelves
2. **Business:** Press O → Different customer types spawn with unique behaviors → Browse shelves → Show dialogue → Collect 1-4 items → Checkout → Day auto-ends when done
3. **End of Day:** Summary panel shows stats → Click "Continue" → Press Tab → Order stock for tomorrow → Press M to advance
4. **Next Morning:** Repeat cycle (Day 2, 3, 4...)

### 👥 Customer Types
- **Quick Shopper**: Fast (4.5 speed), 1 item, impatient, green tint
- **Browser**: Slow (2 speed), 2-3 items, patient, blue tint
- **Big Spender**: Medium (3 speed), 3-4 items, demanding, gold tint

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

### 📋 Phase 13 & 14: Diverse Shelving System (COMPLETED)

**What Was Implemented:**

**New Architecture:**
- **Item Size System**: 3-tier system (Small, Medium, Big) with configurable slot requirements
- **Shelf Types**: Data-driven shelf configuration via ScriptableObjects
- **Multi-Item Storage**: Shelves can now hold multiple different item types simultaneously
- **Slot-Based System**: Each shelf has configurable slots, each slot holds up to N items of one type
- **Single Size Per Shelf**: Each shelf type accepts only ONE item size (Phase 14)

**New Scripts Created:**
1. **ItemDataSO.cs** (Modified):
   - Added `ItemSize` enum (Small, Medium, Big)
   - Added `itemSize` and `slotsRequired` fields
   - Auto-validation: slotsRequired updates based on itemSize

2. **ShelfTypeDataSO.cs** (New):
   - Configurable shelf properties (name, total slots, single allowed item size)
   - Visual layout settings (slot spacing, slots per row, horizontal/vertical)
   - Item display settings (offset, scale)
   - Validates item size compatibility via `CanHoldItemSize(ItemSize)`

3. **ShelfSlot.cs** (New):
   - Individual storage slot component
   - Holds multiple items of the same type (stacking)
   - Visual stacking with slight offsets
   - Automatic item type tracking

4. **Shelf.cs** (Complete Rewrite):
   - Slot-based multi-item storage system
   - Size validation on restocking
   - Auto-generates slots on Awake() based on ShelfTypeDataSO
   - Smart item placement across available slots
   - Methods: `RestockShelf(itemData, quantity)`, `TakeItem(preferredType)`, `GetRandomAvailableItemType()`

5. **InventoryManager.cs** (Extended):
   - Added `GetFirstAvailableItem()` helper method for quick restocking
   - Added `currentInventoryDisplay` list for real-time Inspector viewing (debugging)
   - Displays inventory contents in Unity Inspector during play mode

6. **PlayerController.cs** (Updated):
   - Fixed to work with new `RestockShelf(itemData, quantity)` signature
   - Auto-selects first inventory item when pressing E (temporary - Phase 15 will add UI)

7. **Customer.cs** (Updated):
   - Now picks random item types from multi-item shelves
   - Shows item name in debug logs when picking up

**Design Benefits:**
- **Flexibility**: Create unlimited shelf types with different size restrictions
- **Realism**: Big items require appropriate display spaces (pedestals, floor displays)
- **Scalability**: Easy to add new item sizes or shelf configurations
- **Data-Driven**: All shelf behavior configured via ScriptableObjects

**Architecture Patterns Used:**
- Component-based design (Shelf → ShelfSlots → Items)
- ScriptableObject configuration pattern
- LINQ for collection queries
- Event-driven inventory updates

**Breaking Changes:**
- `Shelf.RestockShelf()` signature changed from `(int quantity)` to `(ItemDataSO itemData, int quantity)`
- Shelves now require `ShelfTypeDataSO` assignment in Inspector
- `ShelfTypeDataSO.allowedItemSizes` (List) changed to `allowedItemSize` (single ItemSize) in Phase 14

**Current Limitations:**
- No visual indicators for shelf capacity or allowed sizes
- Slot positions are procedurally generated (not manually placeable)
- No slot reservation system (customers take first available)

**Shelf Type Configuration (Phase 14)**:
- **Wall Shelf**: Medium items only
- **Display Case**: Small items only
- **Floor Display**: Medium items only
- **Pedestal**: Big items only

---

### 📋 Previous Systems (Still Functional)

- All core systems are implemented and working
- Full gameplay loop is functional with customer variety and humor
- **Customer System Features:**
  - 3 distinct customer types with different behaviors (Quick Shopper, Browser, Big Spender)
  - Multiple items per customer (1-4 items based on type)
  - Patience system implemented (customers track patience while waiting)
  - Visual dialogue bubbles follow customers with corporate evil humor
  - Color-coded customer tints for easy identification
- **Technical Implementation:**
  - CustomerTypeDataSO ScriptableObject system for data-driven design
  - DialogueManager singleton with bubble prefab instantiation
  - Screen space overlay canvas for dialogue UI
  - Customers now browse multiple shelves based on item count
- 2D physics system with Dynamic Rigidbody2D using velocity-based movement
- Player properly collides with walls (requires Player tag to be set)
- Customer spawner supports configurable spawn point transform

### 🎯 Next Steps:
  - **Phase 16: Economy & Progression Balancing** - Add HUD with money/day counter, balance prices, add goals (RECOMMENDED)
  - Alternative: Visual Polish & Juice - Better sprites, particles, sounds, animations
  - Visual indicators for shelf capacity and allowed item sizes
  - More customer dialogue variety and personality-based behaviors
  - Customer returns system with absurd corporate policies
  - Consider removing debug keys (M, K, I) after polish phase
