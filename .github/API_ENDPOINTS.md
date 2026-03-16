# 🛒 API Endpoints - Cart & Order Management

## Base URL
```
https://api.badminton-booking.com
```

---

## 🛒 Cart Management (`/api/cart`)

### 1. Get Cart
```
GET /api/cart
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Cart retrieved successfully.",
  "data": {
    "id": "guid",
    "userId": "guid",
    "updatedAt": "2024-03-15T10:30:00Z",
    "cartItems": [
      {
        "id": "guid",
        "productId": "guid",
        "quantity": 2,
        "product": {
          "id": "guid",
          "productName": "Vợt Badminton Pro",
          "price": 500000,
          "imageUrl": "...",
          "stockQuantity": 10
        },
        "subTotal": 1000000
      }
    ],
    "totalPrice": 1000000
  }
}
```

---

### 2. Add to Cart
```
POST /api/cart/add
Authorization: Bearer {token}
Content-Type: application/json

{
  "productId": "guid",
  "quantity": 2
}
```

**Response:** CartResponse (như trên)

---

### 3. Update Cart Item Quantity
```
PUT /api/cart/item/{cartItemId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "quantity": 5
}
```

**Response:** CartResponse

---

### 4. Delete Cart Item
```
DELETE /api/cart/item/{cartItemId}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Cart item deleted successfully."
}
```

---

### 5. Clear Cart
```
DELETE /api/cart/clear
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Cart cleared successfully."
}
```

---

## 📦 Order Management (`/api/orders`)

### 1. Checkout (Create Order)
```
POST /api/orders/checkout
Authorization: Bearer {token}
Content-Type: application/json

{
  "deliveryAddress": "123 Đường Lê Lợi, Hà Nội",
  "deliveryLatitude": 21.0285,
  "deliveryLongitude": 105.8542,
  "paymentMethod": "COD"
}
```

**Payment Methods:**
- `COD`: Cash on Delivery (Thanh toán khi nhận hàng)
- `VNPAY`: VNPAY Payment Gateway
- `MOMO`: Momo Digital Wallet

**Response:**
```json
{
  "success": true,
  "message": "Order created successfully.",
  "data": {
    "id": "guid",
    "userId": "guid",
    "deliveryAddress": "123 Đường Lê Lợi, Hà Nội",
    "deliveryLatitude": 21.0285,
    "deliveryLongitude": 105.8542,
    "totalAmount": 1000000,
    "paymentMethod": "COD",
    "paymentStatus": "Pending",
    "orderStatus": "Pending",
    "orderDate": "2024-03-15T10:35:00Z",
    "orderDetails": [
      {
        "id": "guid",
        "orderId": "guid",
        "productId": "guid",
        "quantity": 2,
        "unitPrice": 500000,
        "subTotal": 1000000,
        "product": {
          "id": "guid",
          "productName": "Vợt Badminton Pro",
          "price": 500000
        }
      }
    ],
    "user": {
      "id": "guid",
      "fullName": "Nguyễn Văn A",
      "email": "user@example.com"
    }
  }
}
```

---

### 2. Get Order by ID
```
GET /api/orders/{orderId}
Authorization: Bearer {token}
```

**Response:** OrderResponse (như trên)

---

### 3. Get My Orders
```
GET /api/orders/my-orders
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Orders retrieved successfully.",
  "data": [
    { OrderResponse },
    { OrderResponse }
  ]
}
```

---

### 4. Get Orders (Paged - Admin)
```
GET /api/orders?page=1&pageSize=10&userId=guid&orderStatus=Pending&paymentStatus=Paid
Authorization: Bearer {token}
```

**Query Parameters:**
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)
- `userId`: Filter by user ID (optional)
- `orderStatus`: Filter by order status - `Pending`, `Confirmed`, `Shipping`, `Delivered`, `Cancelled` (optional)
- `paymentStatus`: Filter by payment status - `Pending`, `Paid`, `Failed` (optional)

**Response:**
```json
{
  "success": true,
  "message": "Orders retrieved successfully.",
  "data": {
    "items": [
      { OrderResponse },
      { OrderResponse }
    ],
    "page": 1,
    "pageSize": 10,
    "totalItems": 50,
    "totalPages": 5
  }
}
```

---

### 5. Update Order Status (Admin)
```
PUT /api/orders/{orderId}/status
Authorization: Bearer {token}
Content-Type: application/json

{
  "newStatus": "Confirmed"
}
```

**Valid Status Values:**
- `Pending`: Chờ duyệt
- `Confirmed`: Đã duyệt
- `Shipping`: Đang giao hàng
- `Delivered`: Đã giao
- `Cancelled`: Đã hủy

**Response:** OrderResponse

---

### 6. Cancel Order
```
POST /api/orders/{orderId}/cancel
Authorization: Bearer {token}
```

**Rules:**
- ❌ Không hủy được đơn đã giao (Delivered)
- ✅ Tự động hoàn lại tồn kho (Restock)
- ✅ Chuyển trạng thái sang `Cancelled`

**Response:**
```json
{
  "success": true,
  "message": "Order cancelled successfully.",
  "data": {
    "id": "guid",
    "orderStatus": "Cancelled",
    ...
  }
}
```

---

## ⚠️ Error Responses

### Bad Request (400)
```json
{
  "success": false,
  "message": "Cart is empty. Cannot proceed with checkout."
}
```

### Unauthorized (401)
```json
{
  "success": false,
  "message": "Unauthorized: Invalid user ID in token."
}
```

### Not Found (404)
```json
{
  "success": false,
  "message": "Order not found."
}
```

---

## 🔐 Authentication
- Tất cả endpoints yêu cầu JWT Token trong header `Authorization: Bearer {token}`
- Token được cấp từ endpoint `/api/auth/login`
- Token chứa `sub` claim là userId (GUID)

---

## 📊 Status Codes
- `200 OK`: Request thành công
- `400 Bad Request`: Dữ liệu invalid, validation failed
- `401 Unauthorized`: Token không hợp lệ hoặc hết hạn
- `404 Not Found`: Resource không tồn tại
- `500 Internal Server Error`: Server error

---

## 🧪 Testing with cURL

### Add to Cart
```bash
curl -X POST https://api.badminton-booking.com/api/cart/add \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "550e8400-e29b-41d4-a716-446655440000",
    "quantity": 2
  }'
```

### Checkout
```bash
curl -X POST https://api.badminton-booking.com/api/orders/checkout \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "deliveryAddress": "123 Đường Lê Lợi, Hà Nội",
    "deliveryLatitude": 21.0285,
    "deliveryLongitude": 105.8542,
    "paymentMethod": "COD"
  }'
```

### Get Orders
```bash
curl -X GET "https://api.badminton-booking.com/api/orders?page=1&pageSize=10" \
  -H "Authorization: Bearer {token}"
```

---

