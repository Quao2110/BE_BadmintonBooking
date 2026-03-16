# 🛒 Cart & Order Management System - Backend Implementation

> **Complete implementation of Task 5: Cart, Order & Payment Flow**  
> Status: ✅ **COMPLETE** (Tasks 5.1, 5.2, 5.3)  
> Date: March 15, 2026

---

## 📖 Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Features](#features)
4. [API Endpoints](#api-endpoints)
5. [Getting Started](#getting-started)
6. [Database Schema](#database-schema)
7. [Implementation Details](#implementation-details)
8. [Testing](#testing)
9. [Documentation](#documentation)
10. [Next Steps](#next-steps)

---

## 🎯 Overview

This implementation provides a complete e-commerce cart and order management system with:
- **Shopping Cart Management** - Add, update, remove products
- **Atomic Checkout** - Transaction-safe order creation
- **Stock Management** - Automatic stock reduction and restoration
- **Order Tracking** - View and manage orders
- **Status Management** - Update order status with automatic restock

### Key Achievements
✅ Full ACID compliance in checkout process  
✅ Race condition prevention with database transactions  
✅ Clean architecture with separation of concerns  
✅ Comprehensive error handling  
✅ Security with JWT authorization  
✅ Well-documented API and code

---

## 🏗️ Architecture

### Layered Architecture
```
┌─────────────────────────────────────┐
│      Presentation Layer             │
│    (Controllers/API Endpoints)      │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      Application Layer              │
│  (Services/Business Logic/AutoMapper)
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      Domain Layer                   │
│      (Interfaces/DTOs)              │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│    Infrastructure Layer             │
│  (Repositories/DbContext/Migrations)│
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      Database (SQL Server)          │
└─────────────────────────────────────┘
```

### Design Patterns Used
- **Repository Pattern** - Abstraction over data access
- **Unit of Work Pattern** - Transaction management
- **Service Pattern** - Business logic encapsulation
- **DTO Pattern** - Data transfer objects
- **Dependency Injection** - Loose coupling
- **Mapper Pattern** - Entity to DTO conversion

---

## ✨ Features

### 🛒 Cart Management
- [x] Get shopping cart
- [x] Add products to cart
- [x] Update product quantities
- [x] Remove items from cart
- [x] Clear entire cart
- [x] Real-time stock validation
- [x] Automatic cart creation per user

### 📦 Order Management
- [x] Atomic checkout with database transaction
- [x] Create orders from cart items
- [x] Automatic stock reduction
- [x] Save unit price at purchase time
- [x] View order details
- [x] Track order history
- [x] Filter orders by status
- [x] Update order status
- [x] Cancel orders with auto-restock
- [x] Paged order listings

### 🔐 Security & Validation
- [x] JWT Authorization on all endpoints
- [x] User ownership verification
- [x] Input validation (DTO validation)
- [x] Stock availability checks
- [x] Transaction rollback on errors
- [x] Meaningful error messages

### 📊 Transaction Safety
- [x] ACID compliance
- [x] Atomicity - all or nothing
- [x] Consistency - valid state maintained
- [x] Isolation - concurrent request safety
- [x] Durability - persistent changes

---

## 🔌 API Endpoints

### Cart Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/cart` | Get user's shopping cart |
| POST | `/api/cart/add` | Add product to cart |
| PUT | `/api/cart/item/{id}` | Update item quantity |
| DELETE | `/api/cart/item/{id}` | Remove item from cart |
| DELETE | `/api/cart/clear` | Clear entire cart |

### Order Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/orders/checkout` | Create order (atomic transaction) |
| GET | `/api/orders/{id}` | Get order details |
| GET | `/api/orders/my-orders` | Get user's orders |
| GET | `/api/orders` | List orders (paginated, filterable) |
| PUT | `/api/orders/{id}/status` | Update order status |
| POST | `/api/orders/{id}/cancel` | Cancel order (auto-restock) |

**Full API documentation:** See [`API_ENDPOINTS.md`](.github/API_ENDPOINTS.md)

---

## 🚀 Getting Started

### Prerequisites
- .NET 6+ SDK
- SQL Server
- Visual Studio / VS Code

### Step 1: Clone Repository
```bash
git clone <repository-url>
cd BE_BadmintonBooking
```

### Step 2: Update Database Connection
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=PRM393;Trusted_Connection=true;"
  }
}
```

### Step 3: Apply Migrations
```bash
cd Infrastructure
dotnet ef database update
```

### Step 4: Run Application
```bash
cd WebAPI
dotnet run
```

### Step 5: Test Endpoints
```bash
# Get cart
curl -H "Authorization: Bearer {token}" https://localhost:5001/api/cart

# Add to cart
curl -X POST https://localhost:5001/api/cart/add \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"productId":"...", "quantity":2}'

# Checkout
curl -X POST https://localhost:5001/api/orders/checkout \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"deliveryAddress":"123 Đường ABC","paymentMethod":"COD"}'
```

---

## 🗄️ Database Schema

### Entity Relationships
```
User
 ├─ Cart (1:1)
 │   └─ CartItems (1:N)
 │        └─ Product (N:1)
 └─ Orders (1:N)
      └─ OrderDetails (1:N)
           └─ Product (N:1)
```

### Key Tables
- **Carts** - Shopping cart per user
- **CartItems** - Products in cart (quantity only, no price)
- **Orders** - Customer orders (with total amount & status)
- **OrderDetails** - Order line items (with saved unit price)
- **Products** - Products with stock quantity

### Important Fields
```sql
-- Carts
Cart.UserId -> Links to User
Cart.UpdatedAt -> Timestamp

-- CartItems
CartItem.CartId -> Links to Cart
CartItem.ProductId -> Links to Product
CartItem.Quantity -> Number of items

-- Orders
Order.UserId -> Links to User
Order.OrderStatus -> Pending/Confirmed/Shipping/Delivered/Cancelled
Order.PaymentStatus -> Pending/Paid/Failed
Order.OrderDate -> When order was created
Order.TotalAmount -> Total price (sum of OrderDetails)

-- OrderDetails
OrderDetail.OrderId -> Links to Order
OrderDetail.ProductId -> Links to Product
OrderDetail.UnitPrice -> Price at purchase time (frozen)
OrderDetail.Quantity -> Number of items

-- Products
Product.StockQuantity -> Current stock (reduced after order)
```

---

## 📝 Implementation Details

### CartService
Manages shopping cart operations with stock validation.

**Key Methods:**
- `GetCartAsync()` - Retrieve or create user's cart
- `AddToCartAsync()` - Add product with stock check
- `UpdateCartItemAsync()` - Update quantity
- `DeleteCartItemAsync()` - Remove item
- `ClearCartAsync()` - Empty cart

### OrderService
Handles checkout and order management with atomic transactions.

**Key Methods:**
- `CheckoutAsync()` - **⭐ ATOMIC TRANSACTION**
  1. Validate cart & address
  2. Check stock availability
  3. Create Order + OrderDetails
  4. Update Product stock
  5. Clear CartItems
  6. COMMIT/ROLLBACK

- `UpdateOrderStatusAsync()` - Update order status with restock handling
- `CancelOrderAsync()` - Cancel order and restore stock
- `GetOrdersByUserAsync()` - Retrieve user's orders
- `GetOrdersPagedAsync()` - Paginated order list (admin)

### Checkout Transaction Flow
```
BEGIN TRANSACTION
├─ Validate cart (not empty)
├─ Validate address
├─ Check stock (all items)
├─ CREATE Order
├─ CREATE OrderDetails (copy from cart)
├─ UPDATE Products (reduce stock)
├─ DELETE CartItems
├─ UPDATE Cart (UpdatedAt)
└─ COMMIT (or ROLLBACK on error)
```

---

## 🧪 Testing

### Unit Tests
Comprehensive unit tests using Moq and xUnit.

**Test Coverage:**
- ✅ Add to cart (success & failure)
- ✅ Update cart (success & failure)
- ✅ Checkout (success, empty cart, insufficient stock)
- ✅ Order status update
- ✅ Order cancellation with restock

**Run Tests:**
```bash
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
```

**Test Examples:** See [`UNIT_TESTS.md`](.github/UNIT_TESTS.md)

### Integration Tests
Test with real database to ensure transaction safety.

### Manual Testing
Use Postman or cURL to test endpoints:
```bash
# Test workflow
1. Add product to catalog
2. Add to cart
3. View cart
4. Update quantity
5. Checkout
6. View order
7. Cancel order (verify stock restored)
```

---

## 📚 Documentation Files

| File | Purpose |
|------|---------|
| [`IMPLEMENTATION_TASK5.md`](.github/IMPLEMENTATION_TASK5.md) | Complete implementation details |
| [`API_ENDPOINTS.md`](.github/API_ENDPOINTS.md) | API reference with examples |
| [`TRANSACTION_STOCK_MANAGEMENT.md`](.github/TRANSACTION_STOCK_MANAGEMENT.md) | Database transactions & stock management |
| [`QUICK_START.md`](.github/QUICK_START.md) | Setup and testing guide |
| [`UNIT_TESTS.md`](.github/UNIT_TESTS.md) | Unit test examples |
| [`COMPLETION_SUMMARY.md`](.github/COMPLETION_SUMMARY.md) | Project completion summary |

---

## 🔄 Workflow Examples

### Example 1: Complete Purchase Flow
```
1. User browses products
2. POST /api/cart/add → Add Vợt Badminton (Qty: 2)
3. GET /api/cart → View cart (SubTotal: 1,000,000)
4. POST /api/orders/checkout → Create order
   - Order created with status "Pending"
   - Stock reduced: 10 → 8
   - Cart cleared automatically
5. GET /api/orders/my-orders → View orders
6. Payment notification received
7. PUT /api/orders/{id}/status → Update to "Confirmed"
```

### Example 2: Cancellation & Restock
```
1. User has 3 pending orders
2. POST /api/orders/{orderId}/cancel → Cancel one order
   - OrderStatus → "Cancelled"
   - Stock restored: 7 → 10
   - Cart remains empty (can add new items)
3. User can place new orders with restored stock
```

### Example 3: Admin Order Management
```
1. GET /api/orders?page=1&orderStatus=Pending → List pending orders
2. PUT /api/orders/{id}/status → Update to "Confirmed"
3. PUT /api/orders/{id}/status → Update to "Shipping"
4. PUT /api/orders/{id}/status → Update to "Delivered"
```

---

## ⚡ Performance Optimizations

### Database Optimizations
- ✅ Eager loading with `.Include()` - prevents N+1 queries
- ✅ Paging for large result sets
- ✅ Indexes on foreign keys (recommended)
- ✅ Efficient filtering with LINQ

### Caching (Optional)
- Consider caching product stock for frequently accessed items
- Cache user cart in Redis for faster access

### Batch Operations
- Bulk insert OrderDetails when needed
- Batch update stock for multiple products

---

## 🚨 Error Handling

### Common Error Scenarios

| Scenario | HTTP | Message |
|----------|------|---------|
| Cart empty | 400 | "Cart is empty" |
| Insufficient stock | 400 | "Insufficient stock for {product}" |
| Product not found | 404 | "Product not found" |
| Invalid address | 400 | "Delivery address required" |
| Unauthorized | 401 | "Unauthorized" |
| Invalid status | 400 | "Invalid order status" |
| Cannot cancel | 400 | "Cannot cancel delivered order" |

### Error Response Format
```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

---

## 🔐 Security Considerations

### ✅ Implemented
- JWT Bearer token authorization
- User ownership verification
- Input validation with DTOs
- SQL injection prevention (EF Core)
- Transaction isolation

### 🔄 To Consider
- Rate limiting on checkout endpoint
- Payment method validation
- Fraud detection
- Audit logging
- Data encryption at rest

---

## 📈 Monitoring & Logging

### Recommended Logging
```csharp
_logger.LogInformation("Order {OrderId} created for User {UserId}", orderId, userId);
_logger.LogWarning("Stock insufficient for Product {ProductId}", productId);
_logger.LogError("Checkout failed for User {UserId}: {Exception}", userId, ex);
```

### Metrics to Track
- Orders per day
- Average order value
- Stock depletion rate
- Cancellation rate
- Transaction success rate

---

## 🚀 Next Steps (Task 5.4)

### Payment Integration (VNPAY/MoMo)
- [ ] Create IPaymentService interface
- [ ] Implement VNPAY payment gateway
- [ ] Implement MoMo payment gateway
- [ ] Create webhook handler for payment callbacks
- [ ] Update Order.PaymentStatus on successful payment
- [ ] Send confirmation notifications

### Example Flow
```
1. POST /api/orders/checkout → Create order (PaymentStatus: Pending)
2. POST /api/payments/create-vnpay-link → Get payment URL
3. User completes payment on VNPAY
4. VNPAY sends IPN webhook → Update PaymentStatus: Paid
5. Send order confirmation email
```

### Payment Service Endpoints
```
POST /api/payments/vnpay-link → Create VNPAY payment URL
POST /api/payments/momo-link → Create MoMo payment URL
POST /api/payments/vnpay-ipn → Handle VNPAY webhook
POST /api/payments/momo-ipn → Handle MoMo webhook
```

---

## 📋 Checklist Before Production

- [ ] All endpoints tested with Postman
- [ ] Database migrations applied
- [ ] JWT token working correctly
- [ ] Error handling verified
- [ ] Transaction rollback tested
- [ ] Stock consistency verified
- [ ] Concurrent orders tested
- [ ] Performance load tested
- [ ] Security review completed
- [ ] Logging configured
- [ ] Database backups set up
- [ ] Monitoring alerts configured

---

## 🙋 FAQ

**Q: Why use transactions for checkout?**  
A: Ensures data consistency. If any step fails, all changes are rolled back, preventing data corruption and overselling.

**Q: What if payment is declined after order creation?**  
A: Order remains with PaymentStatus: Pending. Admin can cancel it, which restores stock automatically.

**Q: Can users modify orders after checkout?**  
A: No. Orders are immutable except for status updates and cancellation. This maintains data integrity.

**Q: How is stock consistency maintained?**  
A: Stock is locked during transaction, preventing simultaneous purchases of the same item.

**Q: What happens to old carts?**  
A: Not deleted. They remain for historical reference but are not displayed after checkout.

---

## 📞 Support & Questions

For issues or questions:
1. Review the relevant documentation file
2. Check error message and logs
3. Verify JWT token is valid
4. Ensure database migrations are applied
5. Test with curl or Postman first

---

## 📄 License

This project is part of PRM393 Badminton Booking System.

---

## ✍️ Authors

- **Hùng** - Backend implementation (Cart, Order, Payment Flow)

---

**Last Updated:** March 15, 2026  
**Status:** ✅ COMPLETE (Tasks 5.1-5.3)  
**Version:** 1.0.0

