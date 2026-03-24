# 🚀 Quick Start Guide - Cart & Order Implementation

## 📁 Project Structure

```
Application/
├── DTOs/
│   ├── RequestDTOs/
│   │   ├── Cart/
│   │   │   ├── AddToCartRequest.cs
│   │   │   ├── UpdateCartItemRequest.cs
│   │   │   └── CartListQuery.cs
│   │   └── Order/
│   │       ├── CheckoutRequest.cs
│   │       └── UpdateOrderStatusRequest.cs
│   └── ResponseDTOs/
│       ├── Cart/
│       │   ├── CartItemResponse.cs
│       │   └── CartResponse.cs
│       └── Order/
│           ├── OrderDetailResponse.cs
│           └── OrderResponse.cs
├── Interfaces/
│   ├── IRepositories/
│   │   ├── ICartRepository.cs
│   │   ├── ICartItemRepository.cs
│   │   ├── IOrderRepository.cs
│   │   └── IOrderDetailRepository.cs
│   ├── IServices/
│   │   ├── ICartService.cs
│   │   └── IOrderService.cs
│   └── IUnitOfWork/
│       └── IUnitOfWork.cs (updated)
├── Services/
│   ├── CartService.cs
│   └── OrderService.cs
└── Mapping/
    └── MappingProfile.cs (updated)

Infrastructure/
├── Repositories/
│   ├── CartRepository.cs
│   ├── CartItemRepository.cs
│   ├── OrderRepository.cs
│   └── OrderDetailRepository.cs
├── UnitOfWork/
│   └── UnitOfWork.cs (updated)
└── Configurations/
    └── DependencyInjection.cs (updated)

WebAPI/
└── Controllers/
    ├── CartController.cs
    └── OrderController.cs

Domain/
└── Entities/
    ├── Cart.cs (existing)
    ├── CartItem.cs (existing)
    ├── Order.cs (existing)
    └── OrderDetail.cs (existing)
```

---

## 🔧 Setup & Configuration

### 1. Database Migration (if not already created)
```bash
cd Infrastructure
dotnet ef database update
```

### 2. Verify Entities
Đảm bảo các entities sau tồn tại:
- ✅ `Cart` - table for shopping carts
- ✅ `CartItem` - items in cart
- ✅ `Order` - customer orders
- ✅ `OrderDetail` - order line items

### 3. Dependency Injection
Đã được cấu hình trong `DependencyInjection.cs`:
```csharp
services.AddScoped<ICartService, CartService>();
services.AddScoped<IOrderService, OrderService>();
```

### 4. AutoMapper
Đã được cấu hình trong `MappingProfile.cs`:
```csharp
CreateMap<CartItem, CartItemResponse>();
CreateMap<Cart, CartResponse>();
CreateMap<OrderDetail, OrderDetailResponse>();
CreateMap<Order, OrderResponse>();
```

---

## 🧪 Testing Workflow

### Step 1: Add Product
```bash
POST /api/products
{
  "productName": "Vợt Badminton",
  "price": 500000,
  "stockQuantity": 10
}
→ Save productId
```

### Step 2: Add to Cart
```bash
POST /api/cart/add
{
  "productId": "{productId}",
  "quantity": 2
}
→ Response: CartResponse with items
```

### Step 3: View Cart
```bash
GET /api/cart
→ Response: CartResponse with total price
```

### Step 4: Update Cart Item
```bash
PUT /api/cart/item/{cartItemId}
{
  "quantity": 3
}
→ Response: Updated CartResponse
```

### Step 5: Checkout
```bash
POST /api/orders/checkout
{
  "deliveryAddress": "123 Đường ABC, Hà Nội",
  "deliveryLatitude": 21.0285,
  "deliveryLongitude": 105.8542,
  "paymentMethod": "COD"
}
→ Response: OrderResponse with OrderDetails
→ Cart cleared automatically
→ Stock updated
```

### Step 6: Check Order
```bash
GET /api/orders/{orderId}
→ Response: Full order details
```

### Step 7: View My Orders
```bash
GET /api/orders/my-orders
→ Response: Array of all user's orders
```

### Step 8: Update Order Status (Admin)
```bash
PUT /api/orders/{orderId}/status
{
  "newStatus": "Confirmed"
}
→ Response: Updated OrderResponse
```

### Step 9: Cancel Order
```bash
POST /api/orders/{orderId}/cancel
→ Response: OrderResponse with status=Cancelled
→ Stock automatically restored
```

---

## 🔍 Key Implementation Details

### Cart Management
- **Auto-create**: Cart tự động tạo khi user thêm SP lần đầu
- **Merge quantities**: Nếu SP đã có → cộng số lượng
- **Stock check**: Kiểm tra tồn kho trước khi thêm

### Checkout Transaction
1. Validate cart & address
2. Check stock for all items
3. Create Order & OrderDetails
4. Update Product.StockQuantity
5. Clear CartItems
6. COMMIT (hoặc ROLLBACK nếu fail)

### Order Status Flow
```
Pending → Confirmed → Shipping → Delivered
                ↓
           Cancelled (with restock)
```

### Stock Management
- ✅ Stock giảm khi checkout
- ✅ Stock tăng lại khi cancel order
- ✅ Transaction ensures consistency
- ✅ Cannot cancel delivered orders

---

## ⚠️ Important Notes

### Security
- [ ] Tất cả endpoints cần [Authorize]
- [ ] Verify user ownership (User chỉ quản lý cart/order của họ)
- [ ] Validate payment method

### Data Validation
- [ ] Quantity phải > 0
- [ ] Delivery address không được trống
- [ ] Product phải tồn tại
- [ ] Stock phải đủ

### Error Handling
- [ ] Return appropriate HTTP status codes
- [ ] Include meaningful error messages
- [ ] Log errors for debugging

---

## 📚 Related Documentation

See also:
- [`IMPLEMENTATION_TASK5.md`](./IMPLEMENTATION_TASK5.md) - Full implementation details
- [`API_ENDPOINTS.md`](./API_ENDPOINTS.md) - Complete API reference
- [`TRANSACTION_STOCK_MANAGEMENT.md`](./TRANSACTION_STOCK_MANAGEMENT.md) - Database transactions

---

## 🚀 Next Steps

### Task 5.4: Payment Integration
- [ ] Implement IPaymentService
- [ ] VNPAY integration
- [ ] Webhook handler for payment callbacks
- [ ] Update PaymentStatus in Order

### Example Flow:
```
1. Checkout → OrderStatus=Pending, PaymentStatus=Pending
2. Create payment link with VNPAY
3. User scans QR/enters card details
4. VNPAY sends IPN webhook
5. Update Order: PaymentStatus=Paid
6. Send notification to user
```

---

## 💡 Tips & Best Practices

### Performance
- Use `.Include()` for eager loading
- Use paging for list endpoints
- Create database indexes on foreign keys

### Maintainability
- Keep service logic in Services layer
- Repository pattern for data access
- DTOs for clean API contracts
- AutoMapper for entity mapping

### Testing
- Unit test services with mock repositories
- Integration test with real database
- Test transaction rollback scenarios
- Test concurrent order creation

---

## 📞 Support

For issues or questions about the implementation:
1. Check the error message carefully
2. Review transaction logs
3. Verify database schema
4. Check JWT token expiration
5. Ensure Authorize headers are present

---

