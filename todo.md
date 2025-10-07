# DungeonMart - TODO List

## Next Session Tasks

### 1. Next Implementation - Gameplay, UI & Polish



- [ ] **Visual Polish:**
  - [ ] Better delivery box visuals
  - [ ] Particle effects when opening boxes
  - [ ] Sound effects (optional)
  - [ ] Empty shelf indicators


### 2. Future Enhancements (Nice to Have)

- [ ] **Tutorial/Help:**
    - [ ] First-time instructions
    - [ ] Contextual hints based on phase

- [ ] **Settings Menu:**
  - [ ] Audio volume controls
  - [ ] Key binding remapping
  - [ ] Graphics settings
  - [ ] Save/load settings

- [ ] **Multiple item types:** Add more items (potions, armor, etc.)
- [ ] **Shop upgrades:** Additional shelves, faster checkout
- [ ] **Difficulty progression:** More customers per day as game progresses
- [ ] **Customer patience system:** Customers leave if waiting too long
- [ ] **Special orders:** Customers request specific items

## Current Status

### ✅ Completed (Phase 1-12)
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
- **Recommended Next Steps:**
  - Diversified shelf types (weapon racks, display cases, small/large shelves)
  - More customer dialogue variety and personality-based behaviors
  - Customer returns system with absurd corporate policies
- Consider removing debug keys (M, K, I) after polish phase
