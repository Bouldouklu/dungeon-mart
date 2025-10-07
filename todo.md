# DungeonMart - TODO List

## Next Session Tasks

### 1. Testing & Verification
- [ ] **TBD:**
  - [ ] TBD
  - [ ] TBD

### 2. Next Implementation - UI & Polish

**Planned Features:**
- [x] **End of Day Summary Panel:**
  - [x] Show day number
  - [x] Show customers served (X/Y)
  - [x] Show daily revenue earned
  - [x] Show total money
  - [x] "Continue to place orders" button

- [ ] **Visual Polish:**
  - [ ] Better delivery box visuals
  - [ ] Particle effects when opening boxes
  - [ ] Sound effects (optional)
  - [ ] Empty shelf indicators

- [ ] **Tutorial/Help:**
  - [ ] First-time instructions
  - [ ] Contextual hints based on phase

### 3. Future Enhancements (Nice to Have)

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

### ✅ Completed (Phase 1-11)
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

### 🎮 Current Gameplay Loop
1. **Morning:** Delivery boxes appear → Press E to open → Items to inventory → Restock shelves
2. **Business:** Press O → Customers spawn → Shop and checkout → Day auto-ends when done
3. **End of Day:** Summary panel shows stats → Click "Continue" → Press Tab → Order stock for tomorrow → Press M to advance
4. **Next Morning:** Repeat cycle (Day 2, 3, 4...)

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
- Full gameplay loop is functional
- 2D physics system with Dynamic Rigidbody2D using velocity-based movement
- Player properly collides with walls (requires Player tag to be set)
- Customer spawner supports configurable spawn point transform
- Ready for testing, polish, and additional features
- Consider removing debug keys (M, K, I) after polish phase
