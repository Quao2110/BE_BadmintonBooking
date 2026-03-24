# 🔄 Database Transaction & Stock Management

## 📋 Checkout Process - Detailed Flow

### ⚡ Transaction Safety (ACID Compliance)

Checkout operation là một transaction nguyên tử (Atomic), đảm bảo:
- **A (Atomicity)**: Tất cả các thao tác hoàn tất hoặc tất cả đều rollback
- **C (Consistency)**: Dữ liệu luôn ở trạng thái hợp lệ
- **I (Isolation)**: Các transaction đồng thời không can thiệp
- **D (Durability)**: Dữ liệu được lưu trữ vĩnh viễn

---

## 🔐 Critical Sections

### 1. Stock Validation Phase
```csharp
// Validate stock for all items BEFORE creating order
foreach (var cartItem in cart.CartItems)
{
    if (cartItem.Product.StockQuantity < cartItem.Quantity)
        throw new Exception($"Insufficient stock for {cartItem.Product.ProductName}");
}
```

**Lý do:**
- ❌ Không nên tạo Order xong rồi mới check stock
- ✅ Phải check stock trước để tránh "over-selling"
- Nếu fail → Đơn hàng không được tạo

---

### 2. Order Creation
```csharp
var order = new Order
{
    Id = Guid.NewGuid(),
    UserId = userId,
    DeliveryAddress = request.DeliveryAddress,
    PaymentMethod = request.PaymentMethod,
    PaymentStatus = "Pending",      // Chưa thanh toán
    OrderStatus = "Pending",         // Chờ xử lý
    OrderDate = DateTime.UtcNow,
    TotalAmount = 0  // Sẽ tính sau
};

await _unitOfWork.OrderRepository.CreateAsync(order);
```

---

### 3. OrderDetails Creation & Stock Update
```csharp
decimal totalAmount = 0;
foreach (var cartItem in cart.CartItems)
{
    var product = await _unitOfWork.ProductRepository.GetByIdAsync(cartItem.ProductId);
    var unitPrice = product.Price;  // ⭐ IMPORTANT: Lưu giá tại thời điểm mua
    var quantity = cartItem.Quantity ?? 0;

    // 1. Create OrderDetail (giống CartItem nhưng với UnitPrice cố định)
    var orderDetail = new OrderDetail
    {
        Id = Guid.NewGuid(),
        OrderId = order.Id,
        ProductId = cartItem.ProductId,
        Quantity = quantity,
        UnitPrice = unitPrice  // ⭐ Giá không thay đổi
    };
    await _unitOfWork.OrderDetailRepository.CreateAsync(orderDetail);

    // 2. Update Stock (Cực kỳ quan trọng!)
    product.StockQuantity = (product.StockQuantity ?? 0) - quantity;
    _unitOfWork.ProductRepository.Update(product);

    totalAmount += (unitPrice * quantity);
}

// Update Order with total
order.TotalAmount = totalAmount;
_unitOfWork.OrderRepository.Update(order);
```

---

### 4. Cart Cleanup
```csharp
foreach (var cartItem in cart.CartItems)
{
    _unitOfWork.CartItemRepository.Delete(cartItem);
}

cart.UpdatedAt = DateTime.UtcNow;
_unitOfWork.CartRepository.Update(cart);
```

---

### 5. Transaction Commit
```csharp
try
{
    await _unitOfWork.CommitAsync();  // Commit all changes
    // Success
}
catch
{
    // Rollback tự động (nếu bất kỳ step nào fail)
    throw;
}
```

---

## 🔄 Stock Management Operations

### Adding to Cart
```csharp
// Check before adding
if (product.StockQuantity < request.Quantity)
    throw new Exception("Insufficient stock");

cartItem.Quantity = (cartItem.Quantity ?? 0) + request.Quantity;
// Stock không thay đổi cho đến khi checkout
```

---

### Checkout (Reduce Stock)
```csharp
// Stock bị trừ khi Order được tạo
product.StockQuantity = (product.StockQuantity ?? 0) - quantity;
```

**Scenario:**
```
Before Checkout:
  Product A: StockQuantity = 10
  CartItem:  Quantity = 3

After Checkout:
  Product A: StockQuantity = 7 (10 - 3)
  Order:     3 items reserved
```

---

### Cancel Order (Restock)
```csharp
// Hoàn lại stock khi order bị hủy
foreach (var detail in orderDetails)
{
    product.StockQuantity = (product.StockQuantity ?? 0) + detail.Quantity;
}
```

**Rules:**
- ✅ Chỉ restock nếu orderStatus != "Cancelled" (tránh double restock)
- ❌ Không restock cho đơn đã giao (Delivered)
- ✅ Restock tự động khi user hoặc admin hủy

---

### Update Order Status
```csharp
// Pending → Confirmed → Shipping → Delivered
// Mọi status đều có thể quay lại Cancelled

// Nếu chuyển sang Cancelled:
if (newStatus == "Cancelled" && order.OrderStatus != "Cancelled")
{
    // Restock all items
}
```

---

## 🚨 Race Condition Prevention

### Problem
Hai users A và B cùng lúc checkout sản phẩm cuối cùng:

```
User A: Check stock = 1 ✓ (nhìn thấy còn 1)
User B: Check stock = 1 ✓ (nhìn thấy còn 1)
User A: Create order, stock → 0
User B: Create order... ❌ Quá stock!
```

### Solution
- ✅ Database Transaction cô lập các checkout (Isolation)
- ✅ MSSQL sử dụng `Default (READ_COMMITTED)` isolation level
- ✅ Check stock lần thứ 2 trong transaction (trong task 5.2)

---

## 🛡️ Data Consistency Checks

### Pre-Checkout Validation
```csharp
// 1. Cart not empty
if (!cart.CartItems.Any())
    throw new Exception("Cart is empty");

// 2. Delivery address required
if (string.IsNullOrWhiteSpace(request.DeliveryAddress))
    throw new Exception("Delivery address required");

// 3. All products exist
foreach (var item in cart.CartItems)
{
    if (item.Product == null)
        throw new Exception("Product not found");
}

// 4. Stock available
foreach (var item in cart.CartItems)
{
    if (item.Product.StockQuantity < item.Quantity)
        throw new Exception("Insufficient stock");
}
```

---

## 📊 Database Tables Affected During Checkout

### Orders (Insert)
```sql
INSERT INTO Orders (Id, UserId, DeliveryAddress, ...)
VALUES (new_guid, user_id, '...')
```

### OrderDetails (Insert multiple)
```sql
INSERT INTO OrderDetails (Id, OrderId, ProductId, Quantity, UnitPrice)
VALUES (guid1, order_id, prod_id_1, 2, 500000),
       (guid2, order_id, prod_id_2, 1, 300000)
```

### Products (Update - Stock reduction)
```sql
UPDATE Products
SET StockQuantity = StockQuantity - @quantity
WHERE Id = @productId
```

### CartItems (Delete)
```sql
DELETE FROM CartItems
WHERE CartId = @cartId
```

### Carts (Update)
```sql
UPDATE Carts
SET UpdatedAt = GETUTCDATE()
WHERE Id = @cartId
```

---

## 🔄 Transaction Rollback Scenarios

### Scenario 1: Stock Check Fails
```
1. Begin Transaction
2. Validate cart ✓
3. Check stock ❌ → NOT ENOUGH
4. ROLLBACK (nothing created)
5. Return error to user
```

### Scenario 2: Database Error
```
1. Begin Transaction
2. Create Order ✓
3. Create OrderDetail ✓
4. Update Product ❌ (Connection lost)
5. ROLLBACK (Order & OrderDetail deleted, stock unchanged)
6. Return error to user
```

### Scenario 3: Success
```
1. Begin Transaction
2. All operations ✓
3. COMMIT
4. All changes persistent
5. Return success to user
```

---

## ✅ Testing Scenarios

### Test 1: Sufficient Stock
```
Product A: Stock = 10
Order: Quantity = 3
Expected: Stock = 7, Order created ✓
```

### Test 2: Insufficient Stock
```
Product A: Stock = 2
Order: Quantity = 3
Expected: Error thrown, stock = 2, order NOT created ✓
```

### Test 3: Concurrent Orders
```
Product A: Stock = 1
User 1 checkout: Quantity = 1 → Success, Stock = 0
User 2 checkout: Quantity = 1 → Error (stock insufficient) ✓
```

### Test 4: Cancel & Restock
```
Product A: Stock = 7 (after order of 3)
Cancel order
Expected: Stock = 10, OrderStatus = Cancelled ✓
```

---

## 📈 Performance Considerations

### Indexes (Recommended)
```sql
-- Speed up order lookups
CREATE INDEX idx_Orders_UserId ON Orders(UserId);
CREATE INDEX idx_Orders_OrderStatus ON Orders(OrderStatus);

-- Speed up cart queries
CREATE INDEX idx_CartItems_CartId ON CartItems(CartId);

-- Speed up product stock checks
CREATE INDEX idx_Products_StockQuantity ON Products(StockQuantity);
```

### Query Optimization
- ✅ Use `.Include()` to load related entities (avoid N+1 queries)
- ✅ Use paging for large result sets
- ✅ Batch updates when possible

---

