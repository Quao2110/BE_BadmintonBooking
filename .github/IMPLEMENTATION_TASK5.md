# 🛒 Task 5 Implementation - Cart & Order Management

## ✅ Hoàn thành các yêu cầu

### 📦 DTOs (Data Transfer Objects)

#### RequestDTOs:
- **AddToCartRequest**: Thêm sản phẩm vào giỏ (ProductId, Quantity)
- **UpdateCartItemRequest**: Cập nhật số lượng trong giỏ
- **CheckoutRequest**: Chốt đơn với địa chỉ và phương thức thanh toán
- **UpdateOrderStatusRequest**: Cập nhật trạng thái đơn hàng

#### ResponseDTOs:
- **CartItemResponse**: Chi tiết một item trong giỏ (với ProductResponse và SubTotal)
- **CartResponse**: Giỏ hàng hoàn chỉnh (Items + TotalPrice)
- **OrderDetailResponse**: Chi tiết một dòng trong đơn hàng
- **OrderResponse**: Đơn hàng hoàn chỉnh (với UserInfo + OrderDetails)

---

### 🗄️ Database Layer (Repositories)

#### CartRepository & ICartRepository
- `GetByUserIdWithIncludesAsync()`: Lấy giỏ hàng kèm tất cả chi tiết sản phẩm
- `GetCartByIdWithIncludesAsync()`: Lấy giỏ hàng theo ID

#### CartItemRepository & ICartItemRepository
- `GetByCartIdAndProductIdAsync()`: Kiểm tra sản phẩm đã có trong giỏ?
- `GetByCartIdAsync()`: Lấy tất cả items của một giỏ hàng

#### OrderRepository & IOrderRepository
- `GetByUserIdAsync()`: Lấy tất cả đơn hàng của user
- `GetByIdWithIncludesAsync()`: Lấy đơn hàng chi tiết kèm OrderDetails
- `GetPagedAsync()`: Phân trang đơn hàng (hỗ trợ filter theo status, payment status)

#### OrderDetailRepository & IOrderDetailRepository
- `GetByOrderIdAsync()`: Lấy tất cả chi tiết của một đơn hàng

---

### 🔧 Business Logic (Services)

#### CartService & ICartService
**Chức năng chính:**
- `GetCartAsync()`: Lấy giỏ hàng (tạo mới nếu không tồn tại)
- `AddToCartAsync()`: Thêm sản phẩm (kiểm tra stock, cộng số lượng nếu trùng)
- `UpdateCartItemAsync()`: Cập nhật số lượng (kiểm tra stock)
- `DeleteCartItemAsync()`: Xóa một item khỏi giỏ
- `ClearCartAsync()`: Xóa toàn bộ giỏ hàng

**Validation & Checks:**
✓ Kiểm tra số lượng > 0
✓ Kiểm tra tồn kho sản phẩm
✓ Cầp nhật UpdatedAt timestamp

---

#### OrderService & IOrderService
**Chức năng chính:**
1. **CheckoutAsync()** - ⭐ **CRITICAL - USES DATABASE TRANSACTION**
   - Kiểm tra giỏ hàng không rỗng
   - Xác thực địa chỉ giao hàng
   - ✅ **Stock Validation**: Duyệt từng item, kiểm tra StockQuantity
   - ✅ **Create Order**: Tạo bản ghi Order
   - ✅ **Create OrderDetails**: Copy dữ liệu từ CartItems sang OrderDetails (lưu giá chốt)
   - ✅ **Update Stock**: Trừ StockQuantity của từng sản phẩm
   - ✅ **Clear Cart**: Xóa tất cả items trong giỏ
   - ✅ **Transaction Handling**: Sử dụng `_unitOfWork.CommitAsync()` để đảm bảo ACID properties
   - Nếu bất kỳ bước nào thất bại → Rollback tất cả

2. **GetOrderByIdAsync()**: Lấy thông tin đơn hàng chi tiết
3. **GetOrdersByUserAsync()**: Lấy tất cả đơn hàng của user
4. **GetOrdersPagedAsync()**: Phân trang đơn hàng (admin, support)
5. **UpdateOrderStatusAsync()**: Cập nhật trạng thái (Pending → Confirmed → Shipping → Delivered)
6. **CancelOrderAsync()**: Hủy đơn hàng
   - ✅ **Restock Logic**: Cộng lại tồn kho khi hủy
   - Kiểm tra không hủy đơn đã giao (Delivered)

**Order Status Flow:**
```
Pending → Confirmed → Shipping → Delivered
   ↓
Cancelled (có restock)
```

**Payment Status:**
- Pending: Chờ thanh toán
- Paid: Đã thanh toán
- Failed: Thanh toán thất bại

---

### 🎯 API Endpoints (Controllers)

#### CartController (`/api/cart`)
```
GET    /api/cart                    → Lấy giỏ hàng
POST   /api/cart/add                → Thêm sản phẩm
PUT    /api/cart/item/{cartItemId}  → Cập nhật số lượng
DELETE /api/cart/item/{cartItemId}  → Xóa một item
DELETE /api/cart/clear              → Xóa toàn bộ giỏ
```

#### OrderController (`/api/orders`)
```
POST   /api/orders/checkout                    → Chốt đơn (Checkout)
GET    /api/orders/{id}                        → Lấy chi tiết đơn hàng
GET    /api/orders/my-orders                   → Lấy đơn hàng cá nhân
GET    /api/orders                             → Phân trang đơn hàng (hỗ trợ filter)
PUT    /api/orders/{id}/status                 → Cập nhật trạng thái
POST   /api/orders/{id}/cancel                 → Hủy đơn hàng
```

---

### 🔐 Security & Authorization
- ✅ Tất cả endpoints được bảo vệ bằng `[Authorize]`
- ✅ Lấy UserId từ JWT Token `ClaimTypes.NameIdentifier`
- ✅ Kiểm tra quyền sở hữu (User chỉ được quản lý giỏ/đơn của họ)
- ✅ Validate input trước khi xử lý

---

### 📊 AutoMapper Mappings
```csharp
CreateMap<CartItem, CartItemResponse>();
CreateMap<Cart, CartResponse>();
CreateMap<OrderDetail, OrderDetailResponse>();
CreateMap<Order, OrderResponse>();
```

---

### 🔌 Dependency Injection Configuration
Đã thêm vào `DependencyInjection.cs`:
```csharp
services.AddScoped<ICartService, CartService>();
services.AddScoped<IOrderService, OrderService>();
```

UnitOfWork cũng được cập nhật với:
- `ICartRepository CartRepository`
- `ICartItemRepository CartItemRepository`
- `IOrderRepository OrderRepository`
- `IOrderDetailRepository OrderDetailRepository`

---

## 🔄 Database Transaction Flow (CheckoutAsync)

```
START TRANSACTION
  ├─ 1️⃣ Validate Cart
  ├─ 2️⃣ Check Stock untuk tất cả items
  ├─ 3️⃣ Create Order record
  ├─ 4️⃣ Create OrderDetail records (copy từ CartItems)
  ├─ 5️⃣ Update Product.StockQuantity
  ├─ 6️⃣ Clear CartItems
  └─ 7️⃣ COMMIT
ROLLBACK nếu bất kỳ bước nào fail
```

---

## 📝 Trạng thái Implementation

- ✅ Task 5.1: Cart Management (đầy đủ)
- ✅ Task 5.2: Checkout Logic + Transaction (đầy đủ)
- ✅ Task 5.3: Order History & Status Management (đầy đủ)
- ⏳ Task 5.4: Payment Integration (VNPAY/MoMo) - Sẽ implement sau

---

## 🚀 Tiếp theo (Task 5.4)
- Tích hợp VNPAY Payment Gateway
- Tạo Payment Service với IPaymentService
- Implement Webhook IPN handler
- Cập nhật Order.PaymentStatus khi nhận callback từ provider

