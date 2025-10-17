# DungeonMart - 2D to 3D Conversion Framework

## Executive Summary

**Decision: CONVERT TO 3D** ✅
**Estimated Time:** 34-50 hours (~40 hours realistic)
**Complexity Level:** LOW-TO-MODERATE
**WebGL Browser Support:** YES - Fully supported with Synty assets
**Cost:** $0 (Synty asset packs already owned)

---

## Why 3D is the Right Choice

### Developer-Specific Advantages
1. **Natural Mental Model** - You think spatially in 3D, fighting 2D workflow slows development
2. **Asset Availability** - Synty packs provide complete, consistent asset library
3. **No 2D Sprite Hunt** - Eliminated weeks of searching for matching sprite styles
4. **Easier Asset Management** - 3D lighting, materials, and consistency "just work"
5. **Zero Budget Required** - All assets already owned (Synty Fantasy, Polygon, Animation packs)

### Gameplay & Design Benefits
1. **Spatial Clarity** - Shop layout, shelf organization more intuitive in 3D
2. **Customer Pathfinding** - NavMesh provides natural customer movement through aisles
3. **Item Size Differentiation** - Big throne vs small potion reads better in 3D space
4. **Future Expansion** - Multi-floor shops, camera rotation, VR support enabled
5. **Immersion** - Walking through 3D shop feels more engaging than flat 2D

### Technical Validation
1. **WebGL Performance** - Synty low-poly assets optimized for browser deployment
2. **Clean Codebase** - 60% of code already dimension-agnostic (uses Vector3, managers)
3. **Top-Down Camera** - Simpler than third-person, maintains 2D-like controls
4. **Expected Build Size** - 60-100MB WebGL with 60 FPS target (acceptable)

---

## Current Project Analysis (From Live Unity Inspection)

### Confirmed 2D Dependencies

**Player GameObject:**
- Component: `Rigidbody2D` (Dynamic, Continuous collision detection, gravity=0, freeze rotation)
- Component: `CapsuleCollider2D` (size: 0.58 x 1.15)
- Position: (5.91, -4.09, **0.0**) ← Classic 2D z=0 plane
- Script: `PlayerController.cs` - Lines 3, 8, 14, 18-20, 34, 43, 86-88 have 2D dependencies

**Main Camera:**
- Mode: **Orthographic** (orthographic: true)
- Orthographic Size: 5.0
- Position: (0, 0, -10) ← Standard 2D camera offset
- FOV: 34 (unused in orthographic mode)

**Shelves (4 total):**
- FloorDisplay[M], Pedestal[L], WallShelf[M], DisplayCase[S]
- All positioned at z=0
- Script: `Shelf.cs` - Already uses Transform arrays ✅ (minimal changes needed)

**Customer Visuals:**
- 48 SPUM 2D sprite character prefabs (will replace with Synty 3D models)
- Customer.cs uses Vector3.MoveTowards (already 3D-compatible! ✅)

**Items:**
- Current prefabs: PotionHealth, PotionMana, IronSword, IronShield, Throne, VampireCoffin
- All 2D sprites (will replace with Synty props)

### Code That's Already 3D-Ready ✅

**No Changes Required (60% of codebase):**
- All Manager singletons: GameManager, DayManager, FinancialManager, InventoryManager, ProgressionManager, UpgradeManager, SupplyChainManager, AudioManager, PauseManager, DialogueManager
- All ScriptableObjects: ItemDataSO, CustomerTypeDataSO, ProgressionDataSO, UpgradeDataSO, ShelfTypeDataSO
- All UI systems (screen-space canvas works in both 2D/3D)
- Economic systems (ordering, inventory, finances, loans, rent)
- Day/night cycle and phase management
- Customer behavior logic (shopping routine, patience, dialogue)
- Checkout counter queue system
- DeliveryBox.cs (uses Vector3.Distance - works as-is!)
- Shelf.cs (uses Transform arrays - just reposition in 3D)

---

## File-by-File Conversion Checklist

### 🔴 HIGH Priority - Core Physics & Movement (8-12 hours)

#### **1. PlayerController.cs** (2-3 hours)
**Location:** `Assets/Scripts/PlayerController.cs`

**Changes Required:**

```csharp
// LINE 3: Change component requirement
[RequireComponent(typeof(Rigidbody))] // was Rigidbody2D

// LINE 8: Change field type
private Rigidbody rb; // was Rigidbody2D

// LINE 14: Update GetComponent call
rb = GetComponent<Rigidbody>();

// LINES 18-20: Update Rigidbody configuration
rb.bodyType = RigidbodyType.Dynamic; // Remove "2D"
rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Remove "2D"
rb.useGravity = false; // New for top-down
rb.constraints = RigidbodyConstraints.FreezeRotationX |
                 RigidbodyConstraints.FreezeRotationZ |
                 RigidbodyConstraints.FreezePositionY; // Lock to ground plane

// LINE 34: Change velocity to Vector3
rb.linearVelocity = Vector3.zero;

// LINE 43: Update movement for 3D top-down (Y becomes Z)
Vector3 moveInput3D = new Vector3(moveInput.x, 0, moveInput.y);
rb.linearVelocity = moveInput3D * moveSpeed;

// LINE 86-88: Change collision callback
private void OnCollisionEnter(Collision collision) { // was OnCollisionEnter2D
    Debug.Log("Collision detected with: " + collision.gameObject.name);
}
```

**Testing Steps:**
1. Attach new Rigidbody (3D) component to Player
2. Replace CapsuleCollider2D with CapsuleCollider (radius: 0.3, height: 2.0, direction: Y-axis)
3. Test WASD movement on XZ plane
4. Verify wall collisions work with 3D BoxColliders

---

#### **2. Customer.cs** (2-4 hours)
**Location:** `Assets/Scripts/Customer.cs`

**Option A: Simple Movement Update** (2 hours - Quick start)

```csharp
// LINE 79: Update shelf approach position
Vector3 shelfPos = targetShelf.transform.position + new Vector3(0, 0, -1f);
// Changed: Y offset → Z offset for 3D forward direction

// LINE 219: Movement code ALREADY WORKS! ✅
// Vector3.MoveTowards works in 3D as-is, just ensure targetPosition has correct Y value
```

**Option B: NavMesh Integration** (4 hours - RECOMMENDED)

```csharp
// Add at top of file
using UnityEngine.AI;

// Add component requirement
[RequireComponent(typeof(NavMeshAgent))]

// Add field
private NavMeshAgent agent;

// In Awake/Start
agent = GetComponent<NavMeshAgent>();
agent.speed = customerType.moveSpeed;
agent.stoppingDistance = 0.5f;

// Replace MoveToPosition method (LINE 212-215)
private void MoveToPosition(Vector3 target) {
    targetPosition = target;
    agent.SetDestination(target);
    isMoving = true;
}

// Update Update method (LINE 217-225)
private void Update() {
    if (isMoving && agent != null) {
        // Check if NavMeshAgent reached destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) {
            isMoving = false;
        }
    }
}
```

**Why NavMesh?**
- Handles obstacle avoidance automatically
- Customers navigate around shelves naturally
- Looks more realistic in 3D space
- Easier than manual pathfinding

**Testing Steps:**
1. Add NavMeshAgent component to Customer prefab
2. Bake NavMesh in scene (Window → AI → Navigation → Bake)
3. Test customer movement to shelves
4. Verify customers don't clip through obstacles

---

#### **3. DialogueBubble.cs** (30 minutes)
**Location:** `Assets/Scripts/UI/DialogueBubble.cs`

**LINE 44: Works as-is, just adjust offset values**

```csharp
// Current code WORKS in 3D! ✅
Vector3 worldPosition = speakerTransform.position + offset;
transform.position = Camera.main.WorldToScreenPoint(worldPosition);

// Just update offset initialization (in DialogueManager) from:
offset = new Vector3(0, 1, 0); // 2D height
// To:
offset = new Vector3(0, 2.5f, 0); // 3D character height (adjust based on model)
```

**Optional Enhancement:** Switch to WorldSpace canvas (1 hour)
- Better depth sorting in 3D
- Bubbles rotate with camera
- More visually integrated

---

#### **4. CustomerSpawner.cs** (30 minutes)
**Location:** `Assets/Scripts/Singletons/CustomerSpawner.cs`

**LINE 105: Update spawn position**

```csharp
// Current code uses spawnPoint transform position - already correct! ✅
Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;

// Just ensure spawnPoint GameObject is positioned correctly in 3D scene:
// - X: Entrance x-coordinate
// - Y: Ground level (0 or terrain height)
// - Z: Entrance z-coordinate
```

**No code changes needed** - just reposition spawnPoint transform in Unity Editor.

---

#### **5. DeliveryBox.cs** (15 minutes)
**Location:** `Assets/Scripts/DeliveryBox.cs`

**LINE 40: Already uses Vector3.Distance** ✅

```csharp
// This code ALREADY WORKS in 3D! No changes needed.
float distance = Vector3.Distance(transform.position, playerTransform.position);
```

**Unity Editor Changes:**
1. Replace 2D sprite with 3D box model (Synty crate/package)
2. Add BoxCollider (3D) instead of BoxCollider2D
3. Reposition delivery boxes in 3D scene layout

---

#### **6. Shelf.cs** (15 minutes)
**Location:** `Assets/Scripts/Shelf.cs`

**Already uses Transform arrays** ✅ - No code changes needed!

```csharp
// LINE 47-63: This code is dimension-agnostic! ✅
// slotPositions[] array of transforms works in 2D and 3D
```

**Unity Editor Changes:**
1. Replace 2D shelf sprites with 3D shelf models (Synty furniture)
2. Reposition slotPositions child transforms in 3D space
3. Adjust slot spacing for 3D depth (Z-axis stacking)
4. Add BoxCollider (3D) for shelf collision

---

### 🟡 MEDIUM Priority - Scene & Prefab Conversion (10-15 hours)

#### **Camera Setup** (30 minutes)
**GameObject:** Main Camera

**Changes:**
1. **Switch to Perspective Mode:**
   - Inspector → Camera component → Projection: Perspective
   - Field of View: 60

2. **Position for Top-Down View:**
   ```
   Position: (0, 15, 0)    // 15 units above ground
   Rotation: (90, 0, 0)    // Looking straight down
   ```

3. **Alternative: Angled Top-Down (Recommended):**
   ```
   Position: (0, 12, -8)   // Above and behind
   Rotation: (50, 0, 0)    // Angled downward
   ```

4. **Add Camera Controller (Optional, +2 hours):**
   - Smooth follow player
   - Zoom in/out with scroll wheel
   - Rotate with Q/E keys

**Testing:** Verify you can see entire shop floor from camera angle.

---

#### **Player GameObject** (1 hour)
**Current Setup:** 2D sprite + Rigidbody2D + CapsuleCollider2D

**Conversion Steps:**

1. **Replace Components:**
   - Remove: CapsuleCollider2D
   - Add: CapsuleCollider (Radius: 0.3, Height: 2.0, Direction: Y-Axis)
   - Remove: Rigidbody2D
   - Add: Rigidbody (Mass: 1, Drag: 0, Angular Drag: 0.05, Use Gravity: OFF)
   - Constraints: Freeze Position Y, Freeze Rotation X, Freeze Rotation Z

2. **Replace Visual:**
   - Delete 2D sprite child
   - Drag Synty character model into Player GameObject as child
   - Position model at (0, 0, 0) local
   - Scale if needed (Synty models usually 1:1)

3. **Update Script:**
   - Apply PlayerController.cs changes (see section 1 above)

**Testing:** Player should move on XZ plane, collide with walls, stay at Y=0.

---

#### **Shelf Conversion** (2-3 hours for 4 shelves)
**Current Shelves:** FloorDisplay[M], Pedestal[L], WallShelf[M], DisplayCase[S]

**Per-Shelf Conversion (30-45 mins each):**

1. **Replace Visual Model:**
   - Find matching Synty shelf model (Fantasy Kingdom pack has shelves, display cases)
   - Replace 2D sprite with 3D model
   - Position model to align with original shelf position

2. **Update Colliders:**
   - Remove BoxCollider2D
   - Add BoxCollider (3D) matching shelf dimensions
   - Adjust size to prevent player walking through

3. **Reposition Slot Transforms:**
   - Shelf.cs uses slotPositions[] array ✅ (no code change)
   - In Scene view, reposition each slot child transform in 3D space
   - For FloorDisplay[M]: Arrange slots in rows with Z-depth
   - For WallShelf[M]: Stagger slots along wall with Z-offset
   - For Pedestal[L]: Center slots on pedestal surface

4. **Test Item Placement:**
   - Use restock UI to add items to shelf
   - Verify items appear at correct slot positions
   - Adjust slot positions if clipping occurs

**Shelf-Specific Notes:**
- **FloorDisplay[M]** (Medium items): 3x2 grid with depth
- **Pedestal[L]** (Big items): 1-2 central positions
- **WallShelf[M]** (Medium items): Linear row along wall
- **DisplayCase[S]** (Small items): Dense grid, multiple layers

---

#### **Item Prefab Conversion** (3-4 hours for 23 items)
**Current Items:** 8 prefabs created, 23 total in CSV

**Per-Item Conversion (10-15 mins each):**

1. **Find Matching Synty Model:**
   - Health Potion → Synty potion bottle prop
   - Iron Sword → Synty sword model
   - Throne → Synty throne/chair model
   - etc.

2. **Update Prefab:**
   - Open item prefab (e.g., `PotionHealth.prefab`)
   - Delete SpriteRenderer component
   - Add child GameObject with MeshFilter + MeshRenderer
   - Assign Synty model mesh to MeshFilter
   - Assign material to MeshRenderer
   - Add BoxCollider or MeshCollider (if needed for physics)

3. **Scale Adjustment:**
   - Small items: Scale 0.5-1.0
   - Medium items: Scale 1.0-1.5
   - Big items: Scale 1.5-3.0
   - Adjust to match slot size

4. **Script Compatibility:**
   - Item.cs script works as-is ✅ (no changes needed)
   - Just verify item displays correctly when placed on shelf

**Batch Approach:**
- Convert all Small items first (9 items)
- Then Medium items (8 items)
- Finally Big items (6 items)

---

#### **Customer Prefab** (2 hours)
**Current:** Customer.prefab with 48 SPUM 2D sprite variants

**Conversion Steps:**

1. **Update Base Prefab:**
   - Open `Assets/Prefabs/Customer.prefab`
   - Remove 2D sprite components
   - Add NavMeshAgent component:
     - Speed: 3.5 (matches customerType.moveSpeed)
     - Angular Speed: 120
     - Acceleration: 8
     - Stopping Distance: 0.5
     - Auto Braking: ON
     - Radius: 0.3
     - Height: 2.0

2. **Replace Visual Prefabs Array:**
   - Customer.cs has `[SerializeField] private GameObject[] visualPrefabs;` (Line 8)
   - Currently: 48 SPUM 2D sprite prefabs
   - Replace with: 48 Synty character model prefabs
   - Drag Synty character prefabs into array (Fantasy Kingdom pack has variety)

3. **Update itemCarryPoint Position:**
   - In 2D: Position was (0, 0.5, 0) above character
   - In 3D: Position should be (0.3, 1.2, 0) - hand height
   - Adjust based on Synty model hand position

4. **Apply Customer.cs Changes:**
   - Option A: Simple movement update (Section 2, Option A)
   - Option B: NavMesh integration (Section 2, Option B - RECOMMENDED)

**Testing:**
- Spawn customer in play mode
- Verify random Synty model appears
- Test movement to shelf
- Test item pickup and carry
- Test checkout queue navigation

---

#### **Environment Setup** (2-3 hours)
**New GameObjects to Create:**

1. **Ground Plane** (15 mins)
   - Create: GameObject → 3D Object → Plane
   - Scale: (2, 1, 2) - adjust to cover shop area
   - Material: Synty floor texture (wood, stone, tile)
   - Add MeshCollider (Convex: OFF)

2. **Walls** (1 hour)
   - Use Synty modular wall pieces (Fantasy Kingdom pack)
   - Arrange around shop perimeter
   - Add BoxColliders to each wall segment
   - Ensure no gaps for customers to escape through

3. **Checkout Counter** (30 mins)
   - Replace 2D checkout counter sprite with Synty counter model
   - Position: Front of shop near entrance
   - Update CheckoutCounter.cs checkoutPosition transform to match 3D position
   - Add BoxCollider for collision

4. **Door/Entrance** (15 mins)
   - Add Synty door model at entrance
   - Position CustomerSpawner spawnPoint just outside door
   - Purely visual (no door open/close needed for prototype)

5. **Decorations (Optional)** (30 mins)
   - Add Synty props: barrels, crates, lanterns, banners
   - Enhance shop atmosphere
   - Don't block customer pathfinding

**NavMesh Baking** (15 mins)
1. Window → AI → Navigation
2. Select all walkable surfaces (ground, floor areas)
3. Mark as Navigation Static in Inspector
4. Navigation window → Bake tab → Bake
5. Verify blue NavMesh overlay covers shop floor
6. Exclude shelf areas, counter areas

---

#### **Lighting Setup** (1-2 hours)
**Current:** Likely default 2D lighting (ambient only)

**3D Lighting Configuration:**

1. **Directional Light** (Main sunlight) - 30 mins
   - Create: GameObject → Light → Directional Light
   - Rotation: (50, -30, 0) - angled from above
   - Intensity: 1.0
   - Color: Warm white (#FFFAF0)
   - Shadows: Soft Shadows (or None for performance)

2. **Ambient Lighting** - 15 mins
   - Window → Rendering → Lighting
   - Environment Lighting → Source: Color
   - Ambient Color: Warm gray (#A09080)
   - Intensity: 0.8

3. **Point Lights (Optional)** - 30 mins
   - Add point lights above shelves for highlighting
   - Intensity: 0.5-1.0
   - Range: 5-10 units
   - Color: Warm yellow (#FFEEAA)
   - Use 2-4 lights total (performance consideration)

4. **Lightmap Baking (Optional)** - 30 mins
   - For better WebGL performance
   - Mark static objects (walls, floor, shelves) as Lightmap Static
   - Lighting window → Generate Lighting
   - Results in pre-calculated shadows (better FPS)

**Testing:** Ensure shop is well-lit, no dark corners, items visible on shelves.

---

#### **Delivery Box Prefab** (30 mins)
**Current:** DeliveryBox.prefab with 2D sprite

**Conversion Steps:**

1. **Replace Visual:**
   - Open `Assets/Prefabs/DeliveryBox.prefab`
   - Delete SpriteRenderer component
   - Add Synty crate/box/package model as child
   - Scale to appropriate size (1-2 units)

2. **Update Collider:**
   - Remove BoxCollider2D
   - Add BoxCollider (3D) matching model dimensions

3. **Adjust Interaction Range:**
   - DeliveryBox.cs LINE 10: `interactionRange = 1.5f`
   - May need to increase to 2.0f for 3D perspective
   - Test in play mode

4. **Update Prompt UI Position:**
   - promptUI child (Canvas with "Press E" text)
   - Adjust Y position to hover above 3D box
   - Convert to WorldSpace canvas if needed

**Testing:** Player can approach and press E to open box, items added to inventory.

---

### 🟢 LOW Priority - Polish & Optimization (3-5 hours)

#### **Camera Polish** (1-2 hours)
**Optional Enhancements:**

1. **Smooth Follow Player:**
   ```csharp
   // New script: CameraFollow.cs
   public class CameraFollow : MonoBehaviour {
       public Transform target;
       public Vector3 offset = new Vector3(0, 12, -8);
       public float smoothSpeed = 5f;

       void LateUpdate() {
           Vector3 desiredPosition = target.position + offset;
           Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
           transform.position = smoothedPosition;
           transform.LookAt(target);
       }
   }
   ```

2. **Zoom Control:**
   ```csharp
   // Add to CameraFollow.cs
   public float minZoom = 8f;
   public float maxZoom = 15f;

   void Update() {
       float scroll = Input.GetAxis("Mouse ScrollWheel");
       offset.y = Mathf.Clamp(offset.y - scroll * 10f, minZoom, maxZoom);
   }
   ```

---

#### **UI Adjustments** (1 hour)
**Potential Issues in 3D:**

1. **DialogueBubble Scaling:**
   - May appear too large/small in perspective view
   - Adjust canvas scaler or bubble size

2. **Order Menu / Restock UI:**
   - Already screen-space overlay ✅ (works as-is)
   - No changes needed

3. **End-of-Day Summary:**
   - Already screen-space ✅
   - No changes needed

**Testing:** Open all UI panels in 3D perspective, verify readability.

---

#### **Performance Optimization** (1-2 hours)
**For WebGL Target:**

1. **Occlusion Culling:**
   - Window → Rendering → Occlusion Culling
   - Bake occlusion data for indoor shop
   - Hides objects behind walls (better FPS)

2. **LOD Groups (Optional):**
   - Add LOD Group to shelves, decorations
   - Lower poly count when camera far away
   - Synty assets have LOD variants

3. **Texture Atlasing:**
   - Combine shelf textures into atlas
   - Reduce draw calls
   - Use Unity Sprite Atlas tool

4. **Batch Static Objects:**
   - Mark walls, floor as Static in Inspector
   - Unity combines into single draw call

**Target:** 60 FPS in editor, 30-60 FPS in WebGL browser.

---

## Asset Integration Workflow

### Exporting Synty Assets from Other Project

**Step 1: Identify Required Assets**
- Characters: ~10-15 character models for customer variety
- Furniture: Shelves, display cases, counters, pedestals
- Props: Potions, weapons, armor, large items (thrones, statues)
- Environment: Walls, floor tiles, doors, windows
- Containers: Crates, boxes for delivery system

**Step 2: Export as Unity Package**
1. Open project with Synty assets
2. In Project window, select folders:
   - `PolygonFantasyKingdom/Models`
   - `PolygonFantasyKingdom/Materials`
   - `PolygonFantasyKingdom/Prefabs`
3. Right-click → Export Package
4. Include dependencies: ON
5. Save as `SyntyAssets_DungeonMart.unitypackage`

**Step 3: Import to DungeonMart**
1. Open DungeonMart project
2. Assets → Import Package → Custom Package
3. Select `SyntyAssets_DungeonMart.unitypackage`
4. Import all selected items
5. Wait for import to complete (~5-10 mins)

**Step 4: Verify Import**
- Check `Assets/PolygonFantasyKingdom` folder exists
- Verify materials have textures assigned
- Test drag-and-drop prefab into scene
- Confirm no pink missing texture errors

---

### Synty Asset Mapping Guide

**Characters (Customers):**
- Path: `PolygonFantasyKingdom/Prefabs/Characters`
- Recommended: Use humanoid characters (SK_Chr_*)
- Variety: Warriors, mages, merchants, peasants
- Array Size: 10-15 models (reduced from 48 SPUM for simplicity)

**Shelves & Displays:**
- FloorDisplay[M] → `SM_Prop_Table_01` or `SM_Prop_Shelf_01`
- Pedestal[L] → `SM_Prop_Pedestal_01`
- WallShelf[M] → `SM_Prop_Shelf_Wall_01`
- DisplayCase[S] → `SM_Prop_Cabinet_01` or `SM_Prop_Chest_01`

**Items:**
- **Small Items:**
  - Health Potion → `SM_Prop_Potion_01`
  - Mana Potion → `SM_Prop_Potion_02`
  - Rings → `SM_Prop_Ring_01`
  - Scrolls → `SM_Prop_Scroll_01`

- **Medium Items:**
  - Iron Sword → `SM_Wep_Sword_01`
  - Wooden Shield → `SM_Wep_Shield_01`
  - Crossbow → `SM_Wep_Crossbow_01`
  - Armor → `SM_Prop_Armor_01`

- **Big Items:**
  - Throne → `SM_Prop_Throne_01`
  - Statues → `SM_Prop_Statue_01`
  - Bear Trap → `SM_Prop_Trap_01`

**Environment:**
- Floor → `SM_Env_Floor_01` (tile-able)
- Walls → `SM_Bld_Wall_*` (modular pieces)
- Door → `SM_Bld_Door_01`
- Counter → `SM_Prop_Counter_01`

**Containers:**
- Delivery Box → `SM_Prop_Crate_01` or `SM_Prop_Box_01`

---

## Testing Strategy

### Phase 1: Proof of Concept (Day 1-2)
**Goal:** Validate core 3D mechanics work

**Tests:**
1. ✅ Player moves on XZ plane with WASD
2. ✅ Player collides with walls (can't walk through)
3. ✅ Camera shows top-down view of shop
4. ✅ Player can interact with delivery box (Press E)
5. ✅ Items added to inventory from box
6. ✅ Player can approach shelf and open restock UI
7. ✅ Items can be placed on 3D shelf slots

**Pass Criteria:** All 7 tests pass → Proceed to Phase 2

---

### Phase 2: Customer System (Day 3-4)
**Goal:** Validate customer AI works in 3D

**Tests:**
1. ✅ Customer spawns with 3D Synty model
2. ✅ Customer navigates to shelf (NavMesh pathfinding)
3. ✅ Customer picks up item from 3D shelf
4. ✅ Item appears in customer's hand (itemCarryPoint)
5. ✅ Customer navigates to checkout counter
6. ✅ Customer completes transaction
7. ✅ Money added to player account
8. ✅ Customer despawns after transaction

**Pass Criteria:** Full customer flow works → Proceed to Phase 3

---

### Phase 3: Full Gameplay Loop (Day 5-6)
**Goal:** Complete day cycle in 3D

**Tests:**
1. ✅ Morning: Delivery boxes spawn in 3D positions
2. ✅ Player opens boxes, items to inventory
3. ✅ Player restocks multiple shelves with 3D items
4. ✅ Business phase: 6 customers spawn and shop
5. ✅ All customers complete purchases
6. ✅ End-of-day summary shows correct stats
7. ✅ Order menu works (place order for next day)
8. ✅ Advance to Day 2, repeat cycle

**Pass Criteria:** 2-day cycle works smoothly → Proceed to Phase 4

---

### Phase 4: Systems Integration (Day 7)
**Goal:** Validate all game systems in 3D

**Tests:**
1. ✅ Rent payment system (Day 7 rent due)
2. ✅ Loan system (take loan, daily payments)
3. ✅ Progression system (tier-up on revenue milestone)
4. ✅ Upgrade shop (purchase shelf expansion)
5. ✅ New shelf appears in 3D scene
6. ✅ Game over screen (fail to pay rent)
7. ✅ Pause menu (ESC key)
8. ✅ Audio (sounds, music) work correctly

**Pass Criteria:** All systems functional → Ready for polish

---

### WebGL Build Testing (Final)
**Goal:** Validate browser deployment

**Build Settings:**
- Platform: WebGL
- Compression: Brotli
- Code Optimization: Master
- Texture Compression: DXT
- Build and Run

**Browser Tests:**
1. ✅ Game loads within 30 seconds
2. ✅ 30+ FPS on modern browser (Chrome/Firefox)
3. ✅ Controls work (WASD, E, Tab, ESC)
4. ✅ Audio plays
5. ✅ No console errors
6. ✅ Build size < 150MB

**Pass Criteria:** Playable in browser → Conversion complete! 🎉

---

## Common Issues & Solutions

### Issue 1: NavMesh Not Generating
**Symptom:** "Failed to create NavMesh" error

**Solutions:**
- Ensure ground plane is marked Navigation Static
- Check Navigation window → Bake tab → Agent Radius (should be 0.3-0.5)
- Verify no overlapping colliders on floor
- Try manual bake: Window → AI → Navigation → Bake

---

### Issue 2: Customers Clipping Through Walls
**Symptom:** Customers walk through walls, shelves

**Solutions:**
- Add BoxColliders to all wall segments
- Mark walls as Navigation Static → Obstacles
- Rebake NavMesh after adding obstacles
- Increase NavMeshAgent radius to 0.4-0.5

---

### Issue 3: Items Floating/Clipping on Shelves
**Symptom:** Items appear above/below shelf surface

**Solutions:**
- Adjust slotPosition transforms Y-coordinate to shelf surface height
- Check item prefab pivot point (should be at bottom center)
- Ensure shelf model is at scale (1, 1, 1)
- Manually position slots in Scene view

---

### Issue 4: Camera Shows Wrong Angle
**Symptom:** Can't see shop floor, too close/far

**Solutions:**
- For top-down: Position (0, 15, 0), Rotation (90, 0, 0)
- For angled: Position (0, 12, -8), Rotation (50, 0, 0)
- Adjust camera Y-position for zoom level
- Use Scene view to preview camera angle (GameObject → Align View to Selected)

---

### Issue 5: Player Falls Through Floor
**Symptom:** Player drops infinitely at game start

**Solutions:**
- Ensure Rigidbody Use Gravity = OFF
- Add MeshCollider or BoxCollider to ground plane
- Set Player Y position = 0.5 (half capsule height above floor)
- Verify Rigidbody constraints: Freeze Position Y

---

### Issue 6: Low FPS in WebGL Build
**Symptom:** Game runs slow in browser (<20 FPS)

**Solutions:**
- Disable real-time shadows (Lighting → Disable Shadows)
- Reduce light count (max 2-3 lights)
- Use baked lightmaps instead of real-time lighting
- Lower texture quality: Edit → Project Settings → Quality → Texture Quality: Medium
- Enable GPU Instancing on materials
- Reduce NavMesh agent count (spawn fewer customers)

---

### Issue 7: UI Overlaps in 3D View
**Symptom:** Dialogue bubbles too large, covering gameplay

**Solutions:**
- Adjust DialogueBubble offset from (0, 1, 0) to (0, 2.5, 0)
- Reduce bubble canvas scale to 0.8-0.9
- Use WorldSpace canvas instead of ScreenSpace overlay
- Adjust Camera clipping planes: Near 0.3, Far 100

---

### Issue 8: Synty Materials Missing (Pink Textures)
**Symptom:** Models appear pink after import

**Solutions:**
- Reimport package with "Include dependencies" checked
- Check `PolygonFantasyKingdom/Materials` folder exists
- Verify textures imported: `PolygonFantasyKingdom/Textures`
- Manually assign material: Select model → Inspector → Materials → Assign matching material

---

## WebGL Optimization Checklist

### Build Settings
```
Platform: WebGL
Compression Format: Brotli
Code Optimization: Master
Managed Stripping Level: Medium
Enable Exceptions: None
```

### Quality Settings
```
Window → Edit → Project Settings → Quality

- Shadow Quality: Disable Shadows (or Low)
- Anti-aliasing: None or 2x MSAA
- Texture Quality: Medium
- Anisotropic Textures: Per Texture
- LOD Bias: 1.0
- Pixel Light Count: 1-2
- Soft Particles: OFF
- VSync Count: Don't Sync
```

### Performance Targets
- **Editor Play Mode:** 60 FPS (uncapped)
- **WebGL Development Build:** 45-60 FPS
- **WebGL Production Build:** 30-60 FPS (acceptable)
- **Build Size:** 60-150MB (compressed)
- **Initial Load Time:** 10-30 seconds

### Optimization Techniques
1. **Texture Compression:**
   - Select all textures → Inspector → Override for WebGL
   - Format: DXT1/DXT5 (desktop), ASTC (mobile)
   - Max Size: 1024 or 2048

2. **Mesh Optimization:**
   - Synty models already optimized ✅
   - Typical poly count: 500-2000 tris per model
   - Total scene budget: <100k tris

3. **Audio Compression:**
   - Music: Vorbis, Quality 0.5
   - SFX: PCM or Vorbis, Quality 0.7
   - Load Type: Compressed in Memory

4. **Batching:**
   - Enable Static Batching: Edit → Project Settings → Player → Static Batching ON
   - GPU Instancing: Enable on all Synty materials

5. **Lighting:**
   - Use 1 Directional Light (shadows OFF or soft)
   - Max 2-3 Point Lights
   - Bake lightmaps for static objects
   - Ambient Source: Color (not skybox)

---

## Project Duplication Strategy

### Recommended Folder Structure
```
D:/Unity/Games/
├── DungeonMart/              ← Original 2D project (legacy backup)
│   ├── Assets/
│   ├── ProjectSettings/
│   └── README.md
│
└── DungeonMart_3D/           ← New 3D conversion project
    ├── Assets/
    ├── ProjectSettings/
    ├── 2D_to_3D_Framework.md  ← This document
    └── README.md
```

### Duplication Steps

**Option A: Manual Copy (Recommended)**
1. Close Unity (both projects)
2. Windows Explorer: Navigate to `D:/Unity/Games/`
3. Right-click `DungeonMart` folder → Copy
4. Paste in same directory → Rename to `DungeonMart_3D`
5. Open Unity Hub → Add → Select `DungeonMart_3D` folder
6. Open `DungeonMart_3D` project
7. Verify all scripts, assets, scenes present

**Option B: Unity Version Control**
1. Original project: Initialize Git repo
2. Commit current 2D state: `git commit -m "2D version baseline"`
3. Create branch: `git checkout -b 3d-conversion`
4. Perform 3D conversion on branch
5. Keep main branch as 2D legacy
6. Merge or keep separate long-term

**Option C: Unity Package Export/Import**
1. Original project: Select all Assets folders
2. Right-click → Export Package
3. Save as `DungeonMart_2D_Backup.unitypackage`
4. Create new Unity project: `DungeonMart_3D`
5. Import package into new project
6. Apply 3D conversion to new project

---

### Git Workflow (If Using Version Control)
```bash
# In DungeonMart_3D project folder
git init
git add .
git commit -m "Initial 3D conversion - baseline from 2D"

# Create checkpoint commits during conversion
git commit -m "Phase 1: PlayerController converted to 3D"
git commit -m "Phase 2: Customer NavMesh integrated"
git commit -m "Phase 3: All prefabs converted to 3D"
git commit -m "Phase 4: Full gameplay loop tested"

# Tag milestones
git tag -a v3.0-alpha -m "First playable 3D build"
```

---

## Conversion Timeline

### Realistic Schedule (Part-Time, 2 hours/day)

**Week 1: Core Systems**
- **Day 1 (2h):** Export Synty assets, import to DungeonMart_3D
- **Day 2 (2h):** Convert PlayerController.cs, test movement
- **Day 3 (2h):** Convert Customer.cs, add NavMesh
- **Day 4 (2h):** Update camera, convert 2 shelves
- **Day 5 (2h):** Convert remaining 2 shelves
- **Day 6 (2h):** Convert 10 item prefabs
- **Day 7 (2h):** Convert remaining 13 item prefabs

**Week 2: Environment & Testing**
- **Day 8 (2h):** Build environment (floor, walls, door)
- **Day 9 (2h):** Bake NavMesh, test customer pathfinding
- **Day 10 (2h):** Set up lighting, delivery boxes
- **Day 11 (2h):** Full gameplay loop testing (Day 1-7)
- **Day 12 (2h):** Fix bugs, adjust balance
- **Day 13 (2h):** WebGL build test, optimization
- **Day 14 (2h):** Final polish, deploy to itch.io

**Total:** 28 hours over 2 weeks

---

### Aggressive Schedule (Full-Time, 8 hours/day)

**Day 1: Foundation**
- Morning (4h): Export/import Synty, convert PlayerController
- Afternoon (4h): Convert Customer.cs, camera setup

**Day 2: Prefabs**
- Morning (4h): Convert all 4 shelves
- Afternoon (4h): Convert 15 item prefabs

**Day 3: Environment**
- Morning (4h): Convert remaining 8 items, delivery boxes
- Afternoon (4h): Build environment, bake NavMesh

**Day 4: Testing**
- Morning (4h): Full gameplay loop test, bug fixing
- Afternoon (4h): Lighting, optimization, WebGL build

**Day 5: Polish**
- Morning (4h): Final testing, adjustments
- Afternoon (4h): Deploy, documentation

**Total:** 40 hours over 5 days

---

## Success Criteria

### Minimum Viable 3D Product (Phase 1 Complete)
- ✅ Player moves in 3D top-down view
- ✅ Can interact with delivery boxes
- ✅ Can restock 3D shelves
- ✅ One customer can complete purchase
- ✅ Basic 3D models (even if placeholder)

**Decision Gate:** If this works, full conversion is viable.

---

### Feature-Complete 3D (Phase 4 Complete)
- ✅ All game systems work (rent, loans, progression, upgrades)
- ✅ Full day cycle (morning → business → end-of-day)
- ✅ 6+ customers per day complete purchases
- ✅ All 23 items converted to 3D
- ✅ All UI panels functional
- ✅ Audio/music playing
- ✅ 30+ FPS in editor

**Decision Gate:** Ready for WebGL build.

---

### WebGL Production Ready
- ✅ Build size < 150MB
- ✅ Loads in browser within 30 seconds
- ✅ 30+ FPS in Chrome/Firefox
- ✅ No critical bugs
- ✅ All controls responsive
- ✅ Playable on itch.io or similar platform

**Decision Gate:** Ready for public release/playtesting.

---

## Rollback Plan

### If 3D Conversion Fails

**Scenario 1: Technical Issues (NavMesh doesn't work, performance too low)**
- Keep original 2D project intact ✅
- Abandon 3D conversion
- Time lost: Only hours invested so far
- Return to 2D development with lessons learned

**Scenario 2: Doesn't "Feel" Right (gameplay worse in 3D)**
- Test after Phase 1 proof of concept
- If player movement feels awkward → stop early
- Time lost: ~8 hours maximum
- Return to 2D with confidence it was right choice

**Scenario 3: Asset Issues (Synty models don't match vision)**
- Evaluate during asset import phase (Day 1)
- If style doesn't match game tone → stop
- Time lost: ~2 hours
- Return to 2D, consider different art style

**Safety Net:** Original project preserved in `DungeonMart/` folder - always available to resume.

---

## Next Steps (Immediate Action Items)

### Today (Preparation)
1. ✅ Read this entire document
2. ✅ Duplicate project: `DungeonMart` → `DungeonMart_3D`
3. ✅ Open Synty project, identify required assets
4. ⏳ Export Synty assets as Unity package
5. ⏳ Import Synty package into `DungeonMart_3D`
6. ⏳ Verify import successful (no pink materials)

### Tomorrow (Phase 1 Start)
1. ⏳ Create test scene (copy GameScene.unity → TestScene3D.unity)
2. ⏳ Convert PlayerController.cs (Section 1)
3. ⏳ Update Player GameObject (Section: Player GameObject)
4. ⏳ Switch camera to perspective top-down
5. ⏳ Test movement with WASD
6. ⏳ Verify collision with walls works

### This Weekend (Proof of Concept)
1. ⏳ Convert one shelf to 3D
2. ⏳ Convert 3 item prefabs (1 small, 1 medium, 1 big)
3. ⏳ Test full restock flow
4. ⏳ Convert Customer.cs with NavMesh
5. ⏳ Test one customer completing purchase
6. ⏳ **Decision Gate:** Does it feel good? → Proceed or rollback

---

## Conclusion

**The 2D to 3D conversion is feasible, beneficial, and recommended for DungeonMart.**

**Key Reasons:**
1. You already own all required assets (Synty packs)
2. Your codebase is well-architected for the switch
3. You think spatially in 3D - will speed up future development
4. WebGL browser deployment fully supported
5. 40-hour investment pays off in long-term productivity

**Risk Mitigation:**
- Original 2D project preserved as backup
- Proof of concept phase validates approach early
- Incremental testing prevents wasted effort
- Clear rollback plan if issues arise

**Expected Outcome:**
- **Week 1:** Working 3D prototype with core mechanics
- **Week 2:** Feature-complete 3D game
- **Week 3:** WebGL build deployed and playable in browser

**Final Recommendation: Proceed with conversion.** 🚀

---

## Document History

**Version:** 1.0
**Created:** 2025-01-17
**Author:** Claude Code Analysis
**Purpose:** Complete 2D to 3D conversion guide for DungeonMart Unity project
**Status:** Ready for implementation

**Updates:**
- v1.0 (2025-01-17): Initial comprehensive framework created
