# Item Database & Shelf System - Unity Editor Instructions

## Overview
This guide provides step-by-step Unity Editor instructions to complete the item database and shelf system implementation. All C# scripts have been created and are ready to use.

---

## Step 1: Generate Placeholder Item Prefabs

### Instructions:
1. In Unity Editor, go to **Tools → DungeonMart → Generate Placeholder Item Prefabs**
2. A window will open showing the placeholder generator
3. Click **"Generate All Placeholder Prefabs"** button
4. Wait for the progress bar to complete (creates 35 prefabs)
5. Verify output in Console:
   - Should show "Created: 35" (or "Skipped: X" if some already exist)

### What This Does:
- Creates 35 prefab files in `Assets/Prefabs/Items/`
- Color-coded by category:
  - **Weapons**: Red cubes
  - **Shields**: Blue cubes
  - **Potions**: Green cylinders
  - **Armor & Apparel**: Orange cubes
  - **Traps**: Purple cubes
  - **Magic Items**: Cyan cubes
- Each prefab has the `Item` component attached
- Creates color-coded materials in `Assets/Materials/ItemPlaceholders/`

### Verification:
- Check `Assets/Prefabs/Items/` folder - should contain 35 .prefab files
- Check `Assets/Materials/ItemPlaceholders/` folder - should contain 6 .mat files

---

## Step 2: Import Items from CSV

### Instructions:
1. In Unity Editor, go to **Tools → DungeonMart → Import Items from CSV**
2. The CSV path should default to `Assets/DungeonMart_Items_v2.csv`
3. Click **"Import Items"** button
4. Wait for the import to complete
5. A dialog will show: "Created: 35, Updated: 0, Errors: 0" (or similar)

### What This Does:
- Reads the CSV file with 35 item definitions
- Creates ItemDataSO ScriptableObject assets in `Assets/Resources/Items/`
- Automatically links prefabs to ItemDataSO assets
- Sets category, prices, unlock status, and descriptions

### Verification:
- Check `Assets/Resources/Items/` folder - should contain 35 .asset files
- Open any ItemDataSO asset in Inspector
- Verify fields are populated:
  - ✅ Item Name (e.g., "Iron Sword")
  - ✅ Sell Price (e.g., 25)
  - ✅ Restock Cost (e.g., 15)
  - ✅ Item Category (e.g., Weapons)
  - ✅ Unlocked By Default (TRUE for Weapons/Shields/Potions, FALSE for others)
  - ✅ Description (e.g., "Classic adventurer's choice")
  - ✅ Item Prefab (linked to prefab in Assets/Prefabs/Items/)

### Troubleshooting:
- If "Prefab not found" warnings appear in Console, the prefabs weren't generated (go back to Step 1)
- If ItemDataSO fields are empty, check CSV file format

---

## Step 3: Create New ShelfTypeDataSO Assets

### Instructions for Each Shelf Type:

#### 3.1 Create "Defense Wall" Shelf
1. In Project window, navigate to `Assets/Resources/ShelfTypes/`
2. Right-click → **Create → DungeonMart → Shelf Type**
3. Name the asset: `DefenseWall`
4. Select the asset and in Inspector, set:
   - **Shelf Type Name**: `Defense Wall`
   - **Allowed Categories** (click + twice):
     - Element 0: `Shields`
     - Element 1: `ArmorApparel`
   - **Item Scale**: `1.0`

#### 3.2 Create "Trap Workshop" Shelf
1. Right-click in `Assets/Resources/ShelfTypes/` → **Create → DungeonMart → Shelf Type**
2. Name the asset: `TrapWorkshop`
3. In Inspector, set:
   - **Shelf Type Name**: `Trap Workshop`
   - **Allowed Categories** (click + once):
     - Element 0: `Traps`
   - **Item Scale**: `1.0`

#### 3.3 Create "Magic Pedestal" Shelf
1. Right-click in `Assets/Resources/ShelfTypes/` → **Create → DungeonMart → Shelf Type**
2. Name the asset: `MagicPedestal`
3. In Inspector, set:
   - **Shelf Type Name**: `Magic Pedestal`
   - **Allowed Categories** (click + once):
     - Element 0: `MagicItems`
   - **Item Scale**: `1.0`

#### 3.4 Create "General Shelf" (Multi-Category)
1. Right-click in `Assets/Resources/ShelfTypes/` → **Create → DungeonMart → Shelf Type**
2. Name the asset: `GeneralShelf`
3. In Inspector, set:
   - **Shelf Type Name**: `General Shelf`
   - **Allowed Categories** (click + three times):
     - Element 0: `Weapons`
     - Element 1: `Shields`
     - Element 2: `Potions`
   - **Item Scale**: `1.0`

### Verification:
- `Assets/Resources/ShelfTypes/` should now contain 6-7 shelf type assets (including existing ones)
- Each shelf has appropriate categories assigned
- Multi-category shelves (DefenseWall, GeneralShelf) have multiple entries in allowedCategories

---

## Step 4: Update Existing Shelf Types (If Needed)

### Instructions:

#### 4.1 Check "WeaponRack" Asset
1. Navigate to `Assets/Resources/ShelfTypes/`
2. Find and select `WeaponRack.asset`
3. In Inspector, verify:
   - **Shelf Type Name**: `Weapon Rack`
   - **Allowed Categories**: Should contain `Weapons`
4. If missing, add `Weapons` to the list

#### 4.2 Rename/Update "PotionsShelf" → "AlchemyShelf"
1. Find `PotionsShelf.asset` (or similar name)
2. Rename file to `AlchemyShelf.asset`
3. In Inspector, update:
   - **Shelf Type Name**: `Alchemy Shelf`
   - **Allowed Categories**: Should contain `Potions`

#### 4.3 Check "ShieldDisplay" Asset
1. Find `ShieldDisplay.asset`
2. In Inspector, verify:
   - **Shelf Type Name**: `Shield Display`
   - **Allowed Categories**: Should contain `Shields`

---

## Step 5: Test Item Unlocking System

### Instructions:

#### 5.1 Test Starting Items (Day 1)
1. Enter Play Mode
2. Press **O** key to open Orders menu
3. Verify you see **18 items** available:
   - 7 Weapons (Rusty Dagger → Legendary Warhammer)
   - 5 Shields (Wooden Buckler → Dragon Scale Shield)
   - 6 Potions (Minor Health Potion → Elixir of Power)
4. Verify you see items at all price ranges ($5 - $150)
5. Exit Play Mode

**Expected Result**: All Weapons, Shields, and Potions categories are unlocked by default (18 items total).

#### 5.2 Test Armor & Apparel License Unlock
1. Enter Play Mode
2. Press **8** key to add $1,500 (testing money)
3. Press **ESC** → Click **Upgrades** button
4. Find and purchase **"Armor & Apparel License"** ($300)
5. Close upgrade shop
6. Press **O** to open Orders menu
7. Verify you now see **24 items** (18 starting + 6 armor items):
   - New items: Leather Tunic, Iron Helmet, Chainmail Vest, Plate Armor, Enchanted Robes, Crown of Kings
8. Exit Play Mode

**Expected Result**: Armor category unlocks, adding 6 new items to order menu.

#### 5.3 Test Trap License Unlock
1. Enter Play Mode
2. Press **8** key twice to add $3,000
3. Open Upgrades → Purchase **"Trap Merchant Permit"** ($500)
4. Open Orders menu
5. Verify you now see **29 items** (24 + 5 trap items):
   - New items: Spike Trap, Net Trap, Bear Trap, Poison Gas Trap, Death Trap
6. Exit Play Mode

**Expected Result**: Traps category unlocks, adding 5 new items.

#### 5.4 Test Magic Items License Unlock
1. Enter Play Mode
2. Press **8** key three times to add $4,500
3. Open Upgrades → Purchase **"Arcane Items Certification"** ($800)
4. Open Orders menu
5. Verify you now see **ALL 35 items**:
   - New items: Scroll of Fireball, Ring of Protection, Amulet of Life, Crystal Ball, Staff of Power, Dragon Egg
6. Exit Play Mode

**Expected Result**: Magic Items category unlocks, all 35 items now available.

---

## Step 6: Test Shelf Category Filtering

### Instructions:

#### 6.1 Test Weapon Rack (Single Category)
1. Enter Play Mode
2. Find a shelf in the scene with ShelfTypeDataSO = `WeaponRack`
3. Click the shelf to open restock UI
4. Verify the item selection panel shows **ONLY Weapons** (7 items)
5. Try restocking the shelf with a weapon item
6. Exit Play Mode

**Expected Result**: Shelf filtering works correctly for single-category shelves.

#### 6.2 Test General Shelf (Multi-Category)
1. Enter Play Mode
2. Find a shelf with ShelfTypeDataSO = `GeneralShelf`
3. Click the shelf to open restock UI
4. Verify the item selection panel shows **Weapons + Shields + Potions** (18 items)
5. Try restocking with items from all three categories
6. Exit Play Mode

**Expected Result**: Multi-category shelves accept multiple item types.

#### 6.3 Test Defense Wall (Shields + Armor)
1. Enter Play Mode
2. Press **8** twice, purchase Armor License
3. Find a shelf with ShelfTypeDataSO = `DefenseWall`
4. Click the shelf to open restock UI
5. Verify it shows **Shields (5) + Armor & Apparel (6)** = 11 items
6. Exit Play Mode

**Expected Result**: DefenseWall shelf accepts both Shields and ArmorApparel categories.

---

## Step 7: Verify Item Pricing & Profit Margins

### Instructions:
1. Open `Assets/DungeonMart_Items_v2.csv` in Excel/Notepad
2. Verify pricing formula: **Restock Cost = Sell Price × 0.60**
3. Calculate profit margins for sample items:
   - Rusty Dagger: Sell $5, Cost $3 → Profit $2 (40%)
   - Iron Sword: Sell $25, Cost $15 → Profit $10 (40%)
   - Dragon Egg: Sell $150, Cost $90 → Profit $60 (40%)
4. In Unity, open a few ItemDataSO assets
5. Verify sellPrice and restockCost match CSV values

**Expected Result**: All items have consistent 40% profit margins.

---

## Step 8: Test Item Descriptions (Optional)

### Instructions:
1. Open any ItemDataSO asset in Inspector
2. Check the **Description** field
3. Verify flavor text is present (e.g., "Barely sharp but it's a start" for Rusty Dagger)
4. If you plan to display descriptions in UI later, these are ready to use

---

## Common Issues & Solutions

### Issue: "Prefab not found" warnings in Console
**Solution**: Run Step 1 (Generate Placeholder Item Prefabs) first before importing CSV.

### Issue: Items not appearing in Order Menu
**Solution**: Check SupplyChainManager.cs - ensure `unlockedCategories` contains the correct categories. Verify ItemDataSO has correct `isUnlockedByDefault` value.

### Issue: Shelf restock UI shows wrong items
**Solution**: Check the shelf's ShelfTypeDataSO → Allowed Categories list. Ensure it matches the intended category configuration.

### Issue: CSV importer creates assets with missing data
**Solution**: Verify CSV file format matches exactly:
```
ItemName,Category,SellPrice,RestockCost,UnlockedByDefault,PrefabName,Description
```
No extra spaces, correct column order, TRUE/FALSE in caps.

---

## Success Criteria Checklist

After completing all steps, verify:
- ✅ 35 placeholder prefabs exist in `Assets/Prefabs/Items/`
- ✅ 35 ItemDataSO assets exist in `Assets/Resources/Items/`
- ✅ 6-7 ShelfTypeDataSO assets exist in `Assets/Resources/ShelfTypes/`
- ✅ Order menu shows 18 starting items (Weapons/Shields/Potions)
- ✅ License purchases unlock Armor (6), Traps (5), Magic (6) items
- ✅ All 35 items are accessible after all licenses purchased
- ✅ Shelf category filtering works correctly
- ✅ Multi-category shelves (GeneralShelf, DefenseWall) work correctly
- ✅ Item prices have consistent 40% profit margins
- ✅ No errors in Console during testing

---

## Next Steps (Future Enhancements)

Once the item database is fully tested and balanced:
1. **Customer AI Tier System**: Implement shopping behavior based on item tiers
   - Budget customers (buy $5-25 items)
   - Regular customers (buy $5-75 items)
   - Premium customers (buy $5-150 items)
2. **Replace Placeholder Prefabs**: Substitute colored cubes with proper 3D models
3. **UI Item Descriptions**: Display item descriptions in order menu or tooltip system
4. **Item Icons**: Add 2D sprites for UI representation (itemSprite field)

---

## File Locations Summary

**C# Scripts:**
- `Assets/Scripts/SOs/ItemDataSO.cs` - Item data structure (✅ Updated)
- `Assets/Scripts/Editor/ItemDataImporter.cs` - CSV import tool (✅ Updated)
- `Assets/Scripts/Editor/PlaceholderItemGenerator.cs` - Prefab generator (✅ New)

**Data Files:**
- `Assets/DungeonMart_Items_v2.csv` - Item database CSV (✅ New)
- `Assets/Resources/Items/*.asset` - ItemDataSO assets (⏳ Generated by importer)
- `Assets/Resources/ShelfTypes/*.asset` - ShelfTypeDataSO assets (⏳ Create manually)

**Generated Assets:**
- `Assets/Prefabs/Items/*.prefab` - Placeholder item prefabs (⏳ Generated by tool)
- `Assets/Materials/ItemPlaceholders/*.mat` - Color-coded materials (⏳ Generated by tool)

---

## End of Implementation Guide

All C# code is complete. Follow the Unity Editor steps above to finish the implementation.