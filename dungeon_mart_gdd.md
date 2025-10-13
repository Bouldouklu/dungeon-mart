# Dungeon Mart - Game Design Document

## 🎯 Core Concept
**Tagline**: *"Evil customer service meets corporate absurdity"*

Run a retail store supplying evil customers with weapons, minions, and dungeon equipment. Handle both quick ground-floor shopping while managing corporate bureaucracy gone hilariously wrong.

**Genre**: Business Simulation + Retail Management  
**Platform**: PC/Browser (Unity WebGL)  
**Development**: Several days for playable and fun prototype  
**Target**: Players who enjoy Potion Craft: Alchemist Simulator, Moonlighter, and small business/management sims

---

## 🎮 Core Gameplay Loop *(30-60 seconds)*
1. **Customers enter** with shopping needs 
2. **Browse store**: Customers walk around, examine items on shelves
3. **Grab items**: Pick up weapons, armor, potions, accessories from displays
4. **Checkout process**: Handle transactions, handle returns
5. **Store management**: Restock shelves, manage items returned, pay bills, manage business

---

## 🎥 Camera & Visual Style

### **Camera View**: Angled Top-Down (Moonlighter Style)
- **Angle**: tilt toward player for comfortable viewing
- **Type**: Orthographic camera for clean 2D sprite rendering
- **Benefits**: Clear customer expressions, obvious store layout, simple implementation

### **Art Style**: 
- Corporate retail meets fantasy dungeon aesthetic
- Clean modern store design with awkwardly integrated evil elements
- Fluorescent lighting, corporate signage with fantasy items
- 2D sprites in angled top-down perspective

---

## 🏗️ Store Layout & Inventory

### **Ground Floor Store**: Complete Evil Retail Experience
**Store Sections**:
- **Weapons Corner**: Swords, daggers, bows, wands, clubs, crossbows
- **Armor Section**: Shields, helmets, chainmail, leather armor, gauntlets, boots
- **Magic & Consumables**: Potions, scrolls, spell components, magical herbs
- **Accessories**: Rings, amulets, crowns, belts, cloaks, small enchanted items
- **Trap Supplies**: Rope, spikes, locks, keys, basic mechanical components


### **Customer Shopping Flow**:
- **Enter store** and browse different sections
- **Pick up items** directly from shelves and displays  
- **Carry items** to checkout counter
- **Queue system** when multiple customers shopping
- **Checkout interaction** with payment, and returns

---

## 👥 Customer Types & AI 

### **Customer Archetypes**:
- **Quick Shoppers** (35%): Know exactly what they want, grab and go, small purchases
- **Browsers** (30%): Wander around, examine multiple items, impulse purchases 
- **Big Spenders** (20%): Buy expensive items, multiple purchases, premium customers 
- **more to be defined**

### **Customer Personalities** (used as guides for dialogues implementations): 
- **The Experienced Evil**: *"Just getting my usual sword maintenance kit"*
- **The Nervous Newcomer**: *"Um, is this skeleton... user-friendly?"*
- **The Difficult Customer**: *"This price is outrageous! I demand to speak to a manager!"*
- **The Bulk Buyer**: *"I'll take 10 of these potions. Do you have a volume discount?"*

### **Problem Customers**:
- **Returns & Complaints**: "This goblin is defective", "Sword isn't sharp enough"
- **Policy Arguments**: Warranty disputes, return policy confusion
- **Special Requests**: "Can you gift wrap this cursed dagger?"
- **Price Haggling**: "My evil neighbor got this cheaper at Hell-Mart"

### **Customer AI Behavior**:
- **Enter** with invisible shopping list (1-4 items)
- **Browse** sections relevant to their needs
- **Decision making**: Budget constraints, item comparisons
- **Patience system**: Decreases during long waits, affects satisfaction
- **Checkout behavior**: Join shortest queue, react to service quality

---

## 💼 Business Management Systems

### **Core Business Mechanics**:

**Customer Returns**:
- Absurd return reasons: "Goblin too friendly", "Skeleton fell apart", "Dragon wrong color"
- Corporate policy responses using business speak for evil commerce
- Impact: Lose money but gain comedic content

**Monthly Expenses**:
- **Utility Bills**: Electricity ($127), Magical Energy ($847), Goblin Food ($234)
- **Insurance**: Dragon Liability ($1,200), Cursed Item Containment ($89)
- **Licensing**: Evil Business Permit ($150), Safety Compliance ($75) 
- Must maintain cash flow to avoid closure

**Try Before Buy Demo Area**: - 
- Customers test items for 30-60 seconds
- Outcomes: Happy (guaranteed sale), Neutral (50% chance), Disaster (no sale + lawsuit)
- Comedy potential: Dragons setting things on fire, goblins escaping

**Daily Operations**:
- **Opening/Closing**: Store hours, daily preparation and cleanup
- **Inventory Restocking**: Monitor shelf levels, order from suppliers
- **Customer Service**: Handle complaints, special requests, policy explanations
- **Cash Management**: Daily sales tracking, bank deposits, expense payments

---

## 📈 Progression Systems

### **Store Development**:
- **Physical Expansion**: Additional shelf space, more checkout counters, premium sections
- **Product Variety**: Unlock new item categories, exclusive supplier relationships
- **Store Efficiency**: Self-checkout stations, security systems, express lanes

### **Inventory Management**:
- **Supplier Relationships**: Better prices, exclusive items, bulk discounts, faster deliveries
- **Product Quality**: Higher-tier items with better profit margins
- **Seasonal Stock**: Special event items, holiday rushes, trend-based inventory

### **Customer Service**:
- **Service Quality**: Faster checkout, better customer satisfaction, repeat customers
- **Store Reputation**: Online reviews, word-of-mouth, premium customer attraction
- **Employee Skills**: (If hiring staff) Better service efficiency, problem resolution

### **Business Growth**:
- **Revenue Targets**: Daily, weekly, monthly sales goals
- **Market Expansion**: Serving different evil customer demographics  
- **Corporate Recognition**: "Small Business of the Month" awards, franchise opportunities



---

## 🛠️ Technical Requirements

### **Unity Setup**:
- **2D Sprite-based** rendering with angled top-down camera
- **Orthographic camera** at 45° angle for Moonlighter-style view
- **Simple coordinate system** - no complex isometric math required
- **Sprite sorting** based on Y-position for proper depth

### **Browser Optimization**:
- Target **60fps** performance in WebGL
- **Local save system** (no server required)
- **Mobile-friendly UI** scaling for broader accessibility
- **Quick load times** (<10 seconds to playable)

### **Asset Requirements**:
- Store layout background and tile sprites
- 5-8 customer character types with basic animations
- 20+ inventory item sprites (weapons, armor, potions, etc.)
- UI elements for checkout, underground ordering, business management

---

## 📅 Implementation Plan

### **Prototype V1: Core Retail Operations**
**1st step**:
- Camera setup (angled top-down) and store layout
- Customer spawning, pathfinding, and shelf browsing
- Item & 1 type of shelf mechanic with pickup mechanics - customers grab items from shelves
- Basic inventory tracking and shelf restocking

**2nd step**:
- Checkout counter system with queue management
- Money transactions
- Simple store management: cash flow, daily sales tracking

**Goal**: Customers can browse store, pick up items, and complete purchases

### **Prototype V2: Customer Variety + Business Mechanics (7 hours)**
**1st step**:
- Multiple customer types with different behaviors and personalities
- Store expansion: more shelf space, additional product categories, more shelf type

**2nd step**:
- Customer returns system with absurd corporate policies
- Monthly expense bills and cash flow management
- Inventory management: supplier orders, stock levels, popular items

**Goal**: Complete retail simulation with variety and business pressure

### **Prototype V3: Polish + Corporate Comedy**
**1st step**:
- Corporate dialogue and policy responses for maximum humor
- Store upgrades: better checkout systems, security, customer service tools

**2nd step**:
- Visual and audio polish: animations, sound effects, background music
- Balance testing: customer flow, pricing, difficulty progression
- Bug fixes, optimization, and final WebGL build

**Goal**: Polished retail experience with maximum corporate evil humor

---

## 🎯 Success Metrics

### **Player Experience Goals**:
- **Immediate Clarity**: Understand core concept within 30 seconds
- **Session Length**: 15-30 minute average play sessions
- **Learning Curve**: Master basic operations in 2 minutes, discover depth over hours
- **Humor Delivery**: Players should laugh within first minute of play

### **Technical Goals**:
- **Complete Feature Set**: All core systems working by end of prototype v2
- **Performance**: Smooth 60fps in browser, quick load times
- **Scope Management**: Prioritize core loop over feature creep

---

## 💡 Unique Selling Points

1. **Corporate Evil Satire**: Modern retail culture applied to fantasy evil commerce
2. **Personal Customer Service**: Direct interaction with quirky evil customers
3. **Retail Management Depth**: Inventory, pricing, customer satisfaction, business growth
4. **Emergent Comedy**: Customer personalities and corporate policies create hilarious situations
5. **Cozy Business Sim**: Relaxed pace with optional time pressure from customer queues

---

## 🎭 Core Fantasy

*"I'm running a quirky evil supply store, helping villains find the perfect gear while navigating absurd corporate policies and demanding customers - it's like working retail, but everyone's a cartoon villain!"*

The game succeeds when players feel like small business owners serving a niche market, dealing with the familiar frustrations of retail (difficult customers, inventory management, corporate bureaucracy) but in a delightfully absurd fantasy context that makes every interaction entertaining.

---

## 📝 Development Notes

### **V1 Scope Control Strategy**:
**Focus on Core Retail Experience**:
- Simple ground-floor shopping eliminates complex delivery timers and underground UI
- All customer interactions happen in visible store space
- Inventory management stays straightforward with direct shelf restocking
- Business mechanics provide depth without technical complexity

### **Why This Scope Works**:
1. **Proven Formula**: Traditional retail sim mechanics are well-understood
2. **Immediate Feedback**: All player actions have visible, instant results  
3. **Technical Simplicity**: No elevator systems, delivery timers, or multi-level coordination
4. **Comedy Focus**: More development time available for dialogue and customer personalities
5. **Complete Experience**: Players get full retail management satisfaction without waiting systems

### **Risk Mitigation**:
- **Simple camera system** avoids complex coordinate math
- **Modular customer types** - start with 2-3, add more as time allows
- **Ground floor only** - eliminates complex inventory routing
- **Business humor as strength** - if mechanics are simple, personality and dialogue carry the experience


**Philosophy**: Better to have a simple, polished, hilarious retail experience than a complex, half-finished logistics simulator!

---

## 🚀 Future Features

*The following features are planned for post-prototype expansion:*

### **Underground Storage & Logistics System**:
- **Multi-Level Storage**: Basement levels for large creatures and equipment
- **Delivery System**: Elevator mechanics, customer wait times, underground inventory
- **Large Item Categories**: 
  - Living creatures (goblins, dragons, demons)
  - Furniture (thrones, torture devices, dungeon modules)
  - Bulk corporate orders and custom contracts

### **Advanced Business Mechanics**:
- **Marketing & Advertising**: TV commercials, "Dark Web Ads", "Villain Magazine", "Evil Influencer" partnerships, A/B testing campaigns
- **Financial Systems**: Small business loans, credit ratings, corporate paperwork, monthly payment pressure, repo demons
- **Employee Management**: Hire goblin staff, union negotiations, performance reviews, OSHA compliance violations, workplace safety
- **Franchise Operations**: Multiple store locations, regional management, supply chain coordination across evil territories  
- **Corporate Relations**: B2B partnerships, bulk supplier contracts, evil empire account management

### **Customer Expansion**:
- **Corporate Evil Clients**: Bulk orders, contract negotiations, B2B relationships
- **Consultation Services**: Custom dungeon design, evil lair planning
- **Delivery Services**: Home delivery to customer lairs and fortresses

---

*Last Updated: 05.10.2025*