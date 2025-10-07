# DungeonMart - TODO List

## Next Session Tasks

### 1. Testing & Verification
- [ ] **TBD:**
  - [ ] TBD
  - [ ] TBD

### 2. Continue Implementation - Step 5: UI & Polish

**Planned Features:**
- [ ] **End of Day Summary Panel:**
  - [ ] Show day number
  - [ ] Show customers served (X/Y)
  - [ ] Show daily revenue earned
  - [ ] Show total money
  - [ ] "Continue to place orders" button

- [ ] **Visual Polish:**
  - [ ] Better delivery box visuals
  - [ ] Particle effects when opening boxes
  - [ ] Sound effects (optional)
  - [ ] Empty shelf indicators

- [ ] **Tutorial/Help:**
  - [ ] First-time instructions
  - [ ] Contextual hints based on phase

### 3. Core Menu & System Features

- [x] **Main Menu Scene:**
  - [x] Create dedicated main menu scene
  - [x] Start Game button (loads game scene)
  - [x] Settings button (opens settings menu - placeholder TBD)
  - [x] Quit button
  - [x] Game title/logo display

- [x] **Pause System:**
  - [x] Pause menu overlay (ESC key)
  - [x] Resume button
  - [x] Settings button
  - [x] Return to Main Menu button
  - [x] Pause gameplay when menu is open
  - [x] Unpause when resuming

### 4. Future Enhancements (Nice to Have)

- [ ] **Settings Menu:**
  - [ ] Audio volume controls
  - [ ] Key binding remapping
  - [ ] Graphics settings
  - [ ] Save/load settings

- [ ] **Multiple item types:** Add more items (potions, armor, etc.)
- [ ] **Shop upgrades:** Additional shelves, faster checkout
- [ ] **Difficulty progression:** More customers per day as game progresses
- [x] **Starting items:** Starting delivery boxes spawn on Day 1
- [ ] **Customer patience system:** Customers leave if waiting too long
- [ ] **Special orders:** Customers request specific items

## Current Status

### ✅ Completed (Phase 1-9)
- ✅ Phase 1: Core inventory system
- ✅ Phase 2: Ordering system with UI
- ✅ Phase 3: Day/Night cycle with three phases
- ✅ Phase 4: Customer wave system (fixed spawns per day)
- ✅ Phase 5: Delivery system (boxes spawn, player opens them)
- ✅ Phase 6: Phase restrictions (order menu only at end of day)
- ✅ Phase 7: Starting delivery boxes on Day 1 & proper day progression
- ✅ Phase 8: Main Menu Scene with Play, Settings (TBD), and Quit buttons
- ✅ Phase 9: Pause System with ESC key, pause menu overlay, and all buttons

### 🎮 Current Gameplay Loop
1. **Morning:** Delivery boxes appear → Press E to open → Items to inventory → Restock shelves
2. **Business:** Press O → Customers spawn → Shop and checkout → Day auto-ends when done
3. **End of Day:** Press Tab → Order stock for tomorrow → Confirm order → Press M to advance
4. **Next Morning:** Repeat cycle (Day 2, 3, 4...)

### 🔧 Controls
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
- Ready for testing, polish, and additional features
- Consider removing debug keys (M, K, I) after polish phase
