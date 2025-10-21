# DungeonMart3D - Architecture Diagram

## Game Overview
DungeonMart3D is a 3D shop management tycoon game built in Unity 6.2 where players run a fantasy item shop, managing inventory, serving customers, and growing their business empire.

---

## 1. High-Level System Architecture

```mermaid
graph TB
    subgraph "Core Game Loop"
        GM[GameManager<br/>Money System]
        DM[DayManager<br/>Phase Control]
        PM[PauseManager<br/>Game Pause]
    end

    subgraph "Financial Systems"
        FM[FinancialManager<br/>Rent, Loans, Fail States]
    end

    subgraph "Progression Systems"
        PGM[ProgressionManager<br/>Tiers & Milestones]
        UM[UpgradeManager<br/>Shop Upgrades]
        SSM[ShopSegmentManager<br/>Shop Expansion]
    end

    subgraph "Supply Chain"
        SCM[SupplyChainManager<br/>Orders & Deliveries]
        IM[InventoryManager<br/>Item Storage]
    end

    subgraph "Customer System"
        CS[CustomerSpawner<br/>Wave Spawning]
        CUST[Customer<br/>Shopping AI]
    end

    subgraph "Shop Systems"
        SHELF[Shelf<br/>Display System]
        CC[CheckoutCounter<br/>Payment System]
    end

    subgraph "Player"
        PC[PlayerController<br/>Movement & Interaction]
    end

    subgraph "Audio/Visual"
        AM[AudioManager<br/>Music & SFX]
        DLG[DialogueManager<br/>Customer Speech]
    end

    GM --> FM
    GM --> SCM
    GM --> UM
    DM --> CS
    DM --> SCM
    DM --> FM
    PGM --> UM
    UM --> SSM
    FM --> DM
    PC --> SHELF
    PC --> SCM
    CUST --> SHELF
    CUST --> CC
    CC --> GM
    DM --> AM
    CUST --> DLG
    IM --> SHELF
    IM --> SCM
```

---

## 2. Manager Systems (Singleton Pattern)

```mermaid
classDiagram
    class GameManager {
        -int currentMoney
        +AddMoney(amount)
        +SpendMoney(amount)
        +OnMoneyChanged
    }

    class DayManager {
        -int currentDay
        -GamePhase currentPhase
        +StartMorningPhase()
        +OpenShop()
        +EndDay()
        +StartNextDay()
        +RecordCustomerSale()
    }

    class FinancialManager {
        -int baseRentAmount
        -int daysUntilRentDue
        -bool hasActiveLoan
        +PayRent()
        +TakeLoan(amount)
        +CheckRentDue()
        +CheckLoanStatus()
        +TriggerGameOver(reason)
    }

    class ProgressionManager {
        -int lifetimeRevenue
        -int currentTier
        +AddRevenue(amount)
        +CheckTierProgression()
        +OnTierReached
    }

    class UpgradeManager {
        -List~UpgradeDataSO~ ownedUpgrades
        +PurchaseUpgrade(upgrade)
        +CanPurchaseUpgrade(upgrade)
        +ApplyUpgradeEffect(upgrade)
        +GetUpgradeState(upgrade)
    }

    class SupplyChainManager {
        -List~OrderItem~ currentOrder
        -List~DeliveryItem~ pendingDeliveries
        +AddToOrder(item, quantity)
        +ConfirmOrder()
        +SpawnDeliveryBoxes()
    }

    class CustomerSpawner {
        -int customersPerDay
        -int bonusCustomers
        +StartSpawningWave()
        +AddBonusCustomers(count)
    }

    class InventoryManager {
        -Dictionary~ItemDataSO, int~ inventory
        +AddItem(item, quantity)
        +RemoveItem(item, quantity)
        +GetItemCount(item)
    }

    class AudioManager {
        -Dictionary~SoundType, AudioClip~ sounds
        +PlaySound(soundType)
        +PlayMusic(musicType)
        +CrossfadeMusic(musicType)
    }

    GameManager --> FinancialManager : triggers rent/loan events
    GameManager --> ProgressionManager : tracks revenue
    DayManager --> CustomerSpawner : starts waves
    DayManager --> SupplyChainManager : triggers deliveries
    DayManager --> FinancialManager : advances rent timer
    ProgressionManager --> UpgradeManager : unlocks tiers
    UpgradeManager --> ShopSegmentManager : applies shop expansion
```

---

## 3. Game Loop Flow

```mermaid
stateDiagram-v2
    [*] --> Morning

    state Morning {
        [*] --> DeliverySpawn
        DeliverySpawn --> PlayerRestocks
        PlayerRestocks --> ReadyToOpen
    }

    ReadyToOpen --> OpenForBusiness : Press O

    state OpenForBusiness {
        [*] --> CustomerWave
        CustomerWave --> CustomerBrowsing
        CustomerBrowsing --> CustomerCheckout
        CustomerCheckout --> CustomerLeaves
        CustomerLeaves --> AllCustomersServed
    }

    AllCustomersServed --> EndOfDay

    state EndOfDay {
        [*] --> ShowSummary
        ShowSummary --> CheckRent
        CheckRent --> PlayerOrders
        PlayerOrders --> ReadyForNextDay
    }

    ReadyForNextDay --> Morning : Press M

    CheckRent --> GameOver : Rent unpaid / Loan default
    GameOver --> [*]
```

---

## 4. Data Architecture (ScriptableObjects)

```mermaid
classDiagram
    class ItemDataSO {
        +string itemName
        +int sellPrice
        +int restockCost
        +ItemSize size
        +int slotsRequired
        +GameObject prefab
    }

    class CustomerTypeDataSO {
        +string typeName
        +float moveSpeed
        +int minItemsToBuy
        +int maxItemsToBuy
        +string[] dialogueLines
        +GameObject prefabVariant
    }

    class ProgressionDataSO {
        +int tierLevel
        +string tierName
        +int requiredRevenue
        +string description
        +Sprite tierIcon
    }

    class UpgradeDataSO {
        +string upgradeName
        +string description
        +int cost
        +int requiredTier
        +UpgradeCategory category
        +UpgradeEffectType effectType
        +int effectValue
        +bool isRepeatable
        +int maxPurchases
    }

    class ShelfTypeDataSO {
        +string shelfTypeName
        +ItemSize acceptedSize
        +int slotCount
        +int baseItemsPerSlot
        +GameObject shelfPrefab
    }

    InventoryManager ..> ItemDataSO : uses
    CustomerSpawner ..> CustomerTypeDataSO : spawns from
    ProgressionManager ..> ProgressionDataSO : tracks
    UpgradeManager ..> UpgradeDataSO : manages
    Shelf ..> ShelfTypeDataSO : configured by
```

---

## 5. Player Interaction Flow

```mermaid
sequenceDiagram
    participant P as Player
    participant DB as DeliveryBox
    participant INV as InventoryManager
    participant S as Shelf
    participant RUI as RestockUI

    Note over P,DB: Morning Phase - Receiving Deliveries
    P->>DB: Press E (Open Box)
    DB->>INV: Transfer items to inventory
    INV-->>P: Inventory updated

    Note over P,S: Morning Phase - Restocking
    P->>S: Approach shelf
    P->>RUI: Press E (Open Restock UI)
    RUI->>P: Show available items (filtered by size)
    P->>RUI: Select item
    RUI->>INV: RemoveItem(item, quantity)
    INV->>S: AddItemToSlot(item, slot)
    S-->>P: Shelf restocked
```

---

## 6. Customer AI Flow

```mermaid
sequenceDiagram
    participant CS as CustomerSpawner
    participant C as Customer
    participant S as Shelf
    participant CC as CheckoutCounter
    participant GM as GameManager
    participant DM as DayManager

    Note over CS,C: Business Phase - Customer Spawning
    CS->>C: Spawn customer (with CustomerTypeDataSO)
    C->>C: Navigate to random shelf

    loop Shopping Behavior
        C->>S: Check shelf for items
        alt Items Available
            S-->>C: Take item
            C->>C: Add to shopping list
        else Shelf Empty
            C->>C: Show disappointed dialogue
        end
        C->>C: Navigate to next shelf
    end

    Note over C,CC: Checkout Process
    C->>CC: Navigate to checkout
    C->>CC: Wait in queue
    CC->>C: Process payment
    C->>GM: AddMoney(totalPrice)
    C->>DM: RecordCustomerSale(totalPrice)
    C->>C: Despawn

    alt All Customers Served
        CS->>DM: EndDay()
    end
```

---

## 7. Upgrade System Flow

```mermaid
sequenceDiagram
    participant P as Player
    participant UI as UpgradeShopUI
    participant UM as UpgradeManager
    participant GM as GameManager
    participant SSM as ShopSegmentManager
    participant SHELF as Shelf
    participant CS as CustomerSpawner

    P->>UI: Open upgrade shop
    UI->>UM: GetAvailableUpgrades()
    UM-->>UI: Show upgrades (filtered by tier)

    P->>UI: Click upgrade card
    UI->>UM: CanPurchaseUpgrade(upgrade)?

    alt Can Purchase
        UM-->>UI: Show confirmation dialog
        P->>UI: Confirm purchase
        UI->>GM: SpendMoney(cost)
        UI->>UM: PurchaseUpgrade(upgrade)

        alt Shop Segment Upgrade
            UM->>SSM: UnlockSegment(segmentIndex)
            SSM-->>UM: Segment unlocked
        else Shelf Capacity Upgrade
            UM->>SHELF: IncreaseCapacity(all shelves)
            SHELF-->>UM: Capacity increased
        else Customer Count Upgrade
            UM->>CS: AddBonusCustomers(count)
            CS-->>UM: Bonus applied
        end

        UM-->>UI: Purchase complete
        UI-->>P: Show success feedback
    else Cannot Purchase
        UM-->>UI: Show error (tier locked / insufficient funds)
    end
```

---

## 8. Financial System Flow

```mermaid
stateDiagram-v2
    [*] --> NormalOperation

    state NormalOperation {
        [*] --> TrackDays
        TrackDays --> RentDue : 7 days passed
        RentDue --> CheckMoney

        state CheckMoney {
            [*] --> HasMoney
            [*] --> NoMoney
        }

        HasMoney --> PayRent : Player pays
        NoMoney --> OfferLoan

        OfferLoan --> TakeLoan : Player accepts
        TakeLoan --> TrackDays

        OfferLoan --> GameOver : Player refuses
        PayRent --> NextMonth
        NextMonth --> TrackDays
    }

    state LoanActive {
        [*] --> DailyPayment
        DailyPayment --> CanPay
        DailyPayment --> CannotPay

        CanPay --> LoanPaid : Final payment
        CanPay --> DailyPayment : Continue

        CannotPay --> GameOver : Default
    }

    TakeLoan --> LoanActive
    LoanPaid --> NormalOperation

    GameOver --> [*]
```

---

## 9. UI System Architecture

```mermaid
graph TB
    subgraph "Core UI"
        MD[MoneyDisplay]
        RCD[RentCountdownUI]
        CUI[CustomerCounterUI]
        PUI[ProgressionUI]
        PH[PhaseIndicatorUI]
    end

    subgraph "Menu Systems"
        MM[MainMenuManager]
        PM[PauseMenuUI]
    end

    subgraph "Shop Management UI"
        OM[OrderMenu]
        OMI[OrderMenuItem]
        RUI[RestockUIManager]
        RIB[RestockItemButton]
    end

    subgraph "Financial UI"
        RPU[RentPaymentUI]
        LUI[LoanUI]
        LSU[LoanStatusUI]
    end

    subgraph "Progression UI"
        USU[UpgradeShopUI]
        UC[UpgradeCard]
    end

    subgraph "Summary UI"
        EODS[EndOfDaySummaryUI]
        GOU[GameOverUI]
    end

    subgraph "In-World UI"
        DB[DialogueBubble]
    end

    MD --> GameManager
    RCD --> FinancialManager
    CUI --> CustomerSpawner
    PUI --> ProgressionManager
    PH --> DayManager

    OM --> SupplyChainManager
    OMI --> SupplyChainManager
    RUI --> InventoryManager
    RIB --> InventoryManager

    RPU --> FinancialManager
    LUI --> FinancialManager
    LSU --> FinancialManager

    USU --> UpgradeManager
    UC --> UpgradeManager

    EODS --> DayManager
    GOU --> FinancialManager

    DB --> DialogueManager
```

---

## 10. File Organization

```
DungeonMart3D/
├── Assets/
│   ├── Scenes/
│   │   ├── MainMenu.unity          # Entry point
│   │   └── GameScene.unity         # Main gameplay
│   │
│   ├── Scripts/
│   │   ├── Singletons/             # Core manager systems
│   │   │   ├── GameManager.cs      # Money & core state
│   │   │   ├── DayManager.cs       # Day/phase control
│   │   │   ├── FinancialManager.cs # Rent, loans, fail states
│   │   │   ├── ProgressionManager.cs # Tier progression
│   │   │   ├── UpgradeManager.cs   # Upgrade system
│   │   │   ├── ShopSegmentManager.cs # Shop expansion
│   │   │   ├── SupplyChainManager.cs # Orders & deliveries
│   │   │   ├── InventoryManager.cs # Item storage
│   │   │   ├── CustomerSpawner.cs  # Customer waves
│   │   │   ├── AudioManager.cs     # Sound & music
│   │   │   ├── DialogueManager.cs  # Customer speech
│   │   │   ├── RestockUIManager.cs # Restock interface
│   │   │   ├── PauseManager.cs     # Pause system
│   │   │   └── DebugInputManager.cs # Debug controls
│   │   │
│   │   ├── SOs/                    # Data definitions
│   │   │   ├── ItemDataSO.cs
│   │   │   ├── CustomerTypeDataSO.cs
│   │   │   ├── ProgressionDataSO.cs
│   │   │   ├── UpgradeDataSO.cs
│   │   │   └── ShelfTypeDataSO.cs
│   │   │
│   │   ├── UI/                     # UI controllers
│   │   │   ├── MoneyDisplay.cs
│   │   │   ├── RentPaymentUI.cs
│   │   │   ├── LoanUI.cs
│   │   │   ├── UpgradeShopUI.cs
│   │   │   ├── EndOfDaySummaryUI.cs
│   │   │   ├── GameOverUI.cs
│   │   │   └── ...
│   │   │
│   │   ├── Editor/                 # Unity editor tools
│   │   │   └── ItemDataImporter.cs # CSV importer
│   │   │
│   │   └── (Core Game Scripts)
│   │       ├── PlayerController.cs # Player movement
│   │       ├── Customer.cs         # Customer AI
│   │       ├── Shelf.cs            # Shelf system
│   │       ├── ShelfSlot.cs        # Shelf slot logic
│   │       ├── CheckoutCounter.cs  # Payment system
│   │       ├── DeliveryBox.cs      # Delivery interaction
│   │       ├── Item.cs             # Item data container
│   │       └── OrderMenu.cs        # Order interface
│   │
│   ├── Resources/                  # ScriptableObject assets
│   │   ├── Items/                  # 23 ItemDataSO assets
│   │   ├── CustomerTypes/          # 3 CustomerTypeDataSO assets
│   │   ├── Progression/            # 5 ProgressionDataSO assets
│   │   └── Upgrades/               # 8+ UpgradeDataSO assets
│   │
│   ├── Prefabs/                    # Reusable game objects
│   │   ├── Customers/              # 48 SPUM character variants
│   │   ├── Shelves/                # Small/Medium/Big shelf prefabs
│   │   ├── Items/                  # 3D item model prefabs
│   │   └── UI/                     # UI panel prefabs
│   │
│   ├── Materials/                  # Rendering materials
│   ├── Textures/                   # Image assets
│   └── Audio/                      # Sound effects & music
│       ├── SFX/
│       └── Music/
│
├── CLAUDE.md                       # Development guide
├── README.md                       # Game design document
└── ARCHITECTURE.md                 # This file
```

---

## 11. Key Design Patterns

### Singleton Pattern
All manager classes use the singleton pattern for global access:
```csharp
public static ClassName Instance;
private void Awake() {
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;
}
```

### Event-Driven Architecture
Managers communicate through events to reduce coupling:
```csharp
// GameManager
public event System.Action<int, int> OnMoneyChanged;

// DayManager
public event System.Action<GamePhase> OnPhaseChanged;
```

### ScriptableObject Data Pattern
All game data stored in ScriptableObjects for data-driven design:
- Items, Customer Types, Progression Tiers, Upgrades, Shelf Types

### Component-Based Design
- Small, focused components (PlayerController, Customer, Shelf)
- Inspector-assigned references (no GetComponent() searches)
- Prefer SerializeField over public fields

---

## 12. Game Phases Overview

| Phase | Duration | Player Actions | Systems Active |
|-------|----------|----------------|----------------|
| **Morning** | Variable | Open delivery boxes, restock shelves | SupplyChainManager, InventoryManager, Shelf system |
| **Business** | Auto-ends when done | Serve customers, monitor stock | CustomerSpawner, Customer AI, CheckoutCounter |
| **End of Day** | Variable | Review stats, place orders, pay rent | OrderMenu, FinancialManager, ProgressionManager |

---

## 13. Progression System Design

```mermaid
graph LR
    T0[Tier 0: Street Vendor<br/>$0] --> T1[Tier 1: Shop Owner<br/>$1,500]
    T1 --> T2[Tier 2: Merchant<br/>$4,000]
    T2 --> T3[Tier 3: Trade Baron<br/>$8,000]
    T3 --> T4[Tier 4: Tycoon<br/>$15,000]

    style T0 fill:#888
    style T1 fill:#4a9eff
    style T2 fill:#9d4aff
    style T3 fill:#ff4a9d
    style T4 fill:#ffd700
```

**Tier Benefits:**
- Each tier unlocks new upgrades in the shop
- Higher tiers allow more shop expansions
- Progression tracked via lifetime revenue (not current money)

---

## 14. Upgrade Categories

| Category | Effect Type | Examples |
|----------|-------------|----------|
| **Shelves** | Capacity, Expansion | Efficient Shelving (+2 items/slot), Shelf Expansion (+2 shelves) |
| **Operations** | Automation, Speed | Express Checkout (-25% time), Auto-Restock (morning automation) |
| **Customer Flow** | Volume, Revenue | Extended Hours (+2 customers/day), Bulk Deals (10% discount) |

---

## 15. Technical Notes

### Unity Version
- Unity 6.2 (modern APIs required)
- Use `FindFirstObjectByType<T>()` instead of deprecated `FindObjectOfType<T>()`

### 3D Navigation
- NavMesh-based pathfinding for customer AI
- Top-down camera perspective
- Player movement on XZ plane (Y locked)
- Rigidbody physics with gravity disabled

### WebGL Optimization
- Optimized rendering for browser deployment
- Object pooling for frequently spawned objects (customers, delivery boxes)
- Efficient shelf slot management

### Debug System
- All debug keys wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD`
- Automatic exclusion from release builds
- Centralized in `DebugInputManager.cs`

---

## 16. Known Issues & Limitations

### High Priority
- **Game Over UI Input Blocking**: Input stops working when game over screen appears (under investigation)

### Medium Priority
- **Upgrade Effect Testing**: Backend complete but not fully tested in gameplay
- Effects to test: Shop segments, shelf capacity, customer bonuses, checkout speed

### Future Enhancements
- Tutorial/help system
- Settings menu (audio, keybinds, graphics)
- More item variety
- Customer patience system
- Special customer orders
- Multiple shop floors

---

## 17. Development Workflow

1. **Scripts Only**: Claude writes C# scripts exclusively
2. **Unity Setup**: Human performs Unity Editor work (GameObject creation, Inspector setup)
3. **Read-Only Inspection**: Claude can read scene/prefab files to verify setup
4. **No Asset Modification**: Claude does NOT create/modify Unity files (.unity, .prefab, etc.)

---

## Conclusion

DungeonMart3D uses a clean, event-driven architecture with singleton managers coordinating between systems. The progression system provides long-term goals, while the day cycle creates short-term gameplay loops. ScriptableObjects enable data-driven design, allowing easy balancing and content creation without code changes.

**Current Status**: Core systems complete, Phase 1.2 (Upgrade Shop) complete, Phase 1.3 (Testing) pending.
