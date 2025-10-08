# DungeonMart - TODO List

## Next Session Tasks

### **Phase 15 - Restock UI System (NEXT)**

**Goal**: Replace auto-select restocking with proper UI for item selection

**Requirements**:
- [ ] Create RestockUIManager singleton script
- [ ] Create RestockItemButton component script
- [ ] Design UI Canvas with item selection panel (grid layout)
- [ ] Create item button prefab (icon, name, quantity display)
- [ ] Update PlayerController to open UI instead of auto-restocking
- [ ] Filter items by shelf's allowed item size
- [ ] Handle click events to restock selected item
- [ ] Close UI after successful restock
- [ ] Test with multiple item types in inventory

**Current Workaround**: PlayerController auto-selects first available inventory item when pressing E near shelf

---

### Next Implementation - Gameplay, UI & Polish



- [ ] **Visual Polish:**
  - [ ] Better delivery box visuals
  - [ ] Particle effects when opening boxes
  - [ ] Sound effects (optional)
  - [ ] Empty shelf indicators


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

### ✅ Completed (Phase 1-14)
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
- **WASD/Arrow Keys** - Move player (blocked by walls)
- **ESC** - Pause/unpause game (opens pause menu)
- **Tab** - Open order menu (end of day only)
- **E** - Interact (open delivery boxes, restock shelves)

### 🐛 Debug Controls
- **M** - Advance to next day (increments day counter and starts morning)
- **O** - Open shop (morning → business)
- **K** - Force end day (business → end of day)
- **I** - Add debug inventory (testing only)
- **1/2/3/5** - Time scale controls (1x, 2x, 3x, 5x speed)

## Notes for Next Session

### 🆕 Phase 13 & 14: Diverse Shelving System (TESTED & COMPLETE)

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

**Current Limitations (To Be Addressed in Phase 15):**
- ⚠️ No UI for selecting which item to restock (uses first available from inventory)
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
  - **Phase 15: Restock UI System** - Proper UI for selecting items to restock (HIGH PRIORITY)
  - Visual indicators for shelf capacity and allowed item sizes
  - More customer dialogue variety and personality-based behaviors
  - Customer returns system with absurd corporate policies
  - Consider removing debug keys (M, K, I) after polish phase
