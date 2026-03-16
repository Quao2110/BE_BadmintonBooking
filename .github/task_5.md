# 🛒 Backend Module: Cart, Order & Payment Flow

**Người thực hiện:** Hùng  
**Mục tiêu:** Xử lý toàn bộ luồng tiền, quản lý giỏ hàng và hoàn tất quy trình chốt đơn hàng (Checkout).

---

## 📋 Danh sách Task (Checklist)

- [ ] **Task 5.1:** API Quản lý Giỏ hàng (CartItems)
- [ ] **Task 5.2:** API Checkout (Xử lý Transaction & Tồn kho)
- [ ] **Task 5.3:** API Lịch sử đơn hàng & Quản trị trạng thái
- [ ] **Task 5.4:** Tích hợp Thanh toán Online (VNPAY/MoMo) & Webhook

---

## 🛠 Chi tiết Kỹ thuật

### 🔹 Task 5.1: Cart Management (Giỏ hàng)
Quản lý các sản phẩm người dùng dự định mua. Dữ liệu lưu tại Database để đồng bộ trên nhiều thiết bị.

| Method | Endpoint | Mô tả |
| :--- | :--- | :--- |
| `GET` | `/api/cart` | Lấy danh sách sản phẩm trong giỏ (kèm tổng tiền). |
| `POST` | `/api/cart/add` | Thêm SP. Nếu trùng `ProductID`, cộng dồn `Quantity`. |
| `PUT` | `/api/cart/update` | Cập nhật số lượng mới. Cần check `Stock` thực tế. |
| `DELETE` | `/api/cart/item/{id}` | Xóa một sản phẩm khỏi giỏ hàng. |

---

### 🔹 Task 5.2: Checkout Logic (Chốt đơn)
Đây là phần quan trọng nhất, yêu cầu sử dụng **Database Transaction** để đảm bảo không mất mát dữ liệu.



**Quy trình xử lý (Backend Workflow):**
1. **Validation:** Kiểm tra giỏ hàng có trống không? Địa chỉ giao hàng có hợp lệ không?
2. **Stock Check:** Duyệt qua từng item trong giỏ, so sánh số lượng đặt mua với `StockQuantity` trong bảng `Products`.
3. **Create Order:** Chèn dữ liệu vào bảng `Orders` (Lấy ID đơn hàng).
4. **Create OrderDetails:** Copy dữ liệu từ giỏ hàng sang bảng chi tiết đơn hàng (Lưu giá chốt tại thời điểm mua).
5. **Update Stock:** Trừ số lượng tồn kho của sản phẩm:  
   $$Stock_{new} = Stock_{current} - Quantity_{order}$$
6. **Clear Cart:** Xóa các item trong giỏ hàng của User sau khi hoàn tất.

---

### 🔹 Task 5.3: Order History & Status Management
* **Customer Side:** Xem danh sách đơn hàng cá nhân, lọc theo trạng thái (Chờ duyệt, Đang giao, Đã hủy).
* **Admin Side:** Cập nhật trạng thái đơn hàng.
    * *Lưu ý:* Nếu Admin hoặc Khách hủy đơn (`Cancelled`), hệ thống phải có logic **cộng lại số lượng vào kho (Restock)**.

---

### 🔹 Task 5.4: Payment Integration (VNPAY/MoMo)
Tích hợp thanh toán điện tử để tối ưu trải nghiệm.

* **Tạo Payment Link:** Sau khi Checkout thành công ở Task 5.2, hệ thống gọi tới Provider (VNPAY) để lấy URL thanh toán và trả về cho Frontend.
* **IPN/Webhook API:** * Nhận thông báo từ Provider khi khách quét mã/nhập thẻ xong.
    * **Verify Signature:** Kiểm tra chữ ký bảo mật để tránh bị hack tiền ảo.
    * **Update Status:** Cập nhật `PaymentStatus = 'Paid'` nếu giao dịch thành công.

---