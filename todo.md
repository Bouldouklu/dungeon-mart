# DungeonMart - TODO List

## Next Session Tasks

### 1. Testing & Verification
- [ ] **Test complete daily cycle:**
  - [ ] Morning phase: Open delivery boxes, restock shelves
  - [ ] Press O to open shop
  - [ ] Customers spawn (8 customers), serve them all
  - [ ] Day auto-ends when last customer leaves
  - [ ] End of day: Place orders (Tab menu works)
  - [ ] Next morning: Delivery boxes appear

- [ ] **Test phase restrictions:**
  - [ ] Morning: Tab blocked (cannot order)
  - [ ] Business: Tab blocked (cannot order)
  - [ ] End of Day: Tab works (can order)

- [ ] **Test inventory flow:**
  - [ ] Order items → delivered next morning → open boxes → items in inventory
  - [ ] Restock shelves from inventory (E key)
  - [ ] Customers take items from shelves
  - [ ] Money earned from sales

- [ ] **Test edge cases:**
  - [ ] What happens if player doesn't place order?
  - [ ] What happens if shelves are empty when customers arrive?
  - [ ] Can player restock during business hours? (should work)

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

### 3. Future Enhancements (Nice to Have)

- [ ] **Multiple item types:** Add more items (potions, armor, etc.)
- [ ] **Shop upgrades:** Additional shelves, faster checkout
- [ ] **Difficulty progression:** More customers per day as game progresses
- [ ] **Starting items:** Give player some initial inventory on Day 1
- [ ] **Customer patience system:** Customers leave if waiting too long
- [ ] **Special orders:** Customers request specific items

## Current Status

### ✅ Completed (Phase 1-4)
- ✅ Phase 1: Core inventory system
- ✅ Phase 2: Ordering system with UI
- ✅ Phase 3: Day/Night cycle with three phases
- ✅ Phase 4: Customer wave system (fixed spawns per day)
- ✅ Phase 5: Delivery system (boxes spawn, player opens them)
- ✅ Phase 6: Phase restrictions (order menu only at end of day)

### 🎮 Current Gameplay Loop
1. **Morning:** Delivery boxes appear → Press E to open → Items to inventory → Restock shelves
2. **Business:** Press O → Customers spawn → Shop and checkout → Day auto-ends when done
3. **End of Day:** Press Tab → Order stock for tomorrow → Confirm order
4. **Next Morning:** Repeat cycle

### 🔧 Debug Controls
- **M** - Force morning phase
- **O** - Open shop (morning → business)
- **K** - Force end day (business → end of day)
- **I** - Add debug inventory (testing only)
- **Space** - (Disabled, use O instead)

## Notes for Next Session
- All core systems are implemented and working
- Full gameplay loop is functional
- Ready for testing, polish, and additional features
- Consider removing debug keys (M, K, I) after polish phase
