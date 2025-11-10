# 📋 TÓM TẮT TỔNG HỢP DỰ ÁN COSMETICSHOP

## 🎯 1. TỔNG QUAN DỰ ÁN

### 1.1 Mô Tả
**CosmeticShop** là hệ thống website bán mỹ phẩm trực tuyến (E-commerce) được xây dựng bằng:
- **Backend**: Java 17, Jakarta EE 11 (Servlets, JSP)
- **Database**: Microsoft SQL Server
- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Build Tool**: Maven
- **Payment**: VNPay Gateway integration
- **Email**: Jakarta Mail (SMTP)

### 1.2 Mục Đích
Hệ thống cung cấp đầy đủ chức năng cho:
- **Khách hàng**: Mua sắm, thanh toán, theo dõi đơn hàng
- **Quản trị viên**: Quản lý sản phẩm, đơn hàng, người dùng, mã giảm giá

---

## 👥 2. CÁC ACTOR VÀ VAI TRÒ

### 2.1 Guest User (Khách chưa đăng nhập)
- Duyệt sản phẩm
- Xem chi tiết sản phẩm
- Tìm kiếm sản phẩm
- Xem bộ sưu tập

### 2.2 Authenticated User (Người dùng đã đăng nhập)
- Tất cả quyền của Guest
- Quản lý tài khoản
- Quản lý giỏ hàng
- Thanh toán đơn hàng
- Xem lịch sử đơn hàng
- Quản lý địa chỉ giao hàng
- Áp dụng mã giảm giá
- Xem voucher của mình

### 2.3 Admin (Quản trị viên)
- Tất cả quyền của User
- Quản lý sản phẩm (CRUD)
- Quản lý danh mục sản phẩm
- Quản lý đơn hàng
- Quản lý người dùng
- Quản lý mã giảm giá
- Xem báo cáo thống kê
- Quản lý banner

---

## 🔄 3. CÁC USE CASE CHÍNH

### 3.1 Authentication & Account Management

#### UC-001: Đăng Ký Tài Khoản
**Actor**: Guest User  
**Luồng chính**:
1. User truy cập trang đăng ký
2. Nhập thông tin (email, username, password)
3. Hệ thống validate và kiểm tra email trùng lặp
4. Tạo tài khoản với role = "USER"
5. Redirect đến trang đăng nhập

**Controller**: `signup.java`  
**DAO**: `UserDB.signup()`

#### UC-002: Đăng Nhập
**Actor**: User  
**Luồng chính**:
1. User nhập email và password
2. Hệ thống validate credentials
3. Tạo session (timeout: 30 phút)
4. Lưu user object vào session
5. Redirect đến trang chủ

**Controller**: `login.java`  
**DAO**: `UserDB.getUserByEmail()`

#### UC-003: Quên Mật Khẩu
**Actor**: User  
**Luồng chính**:
1. User nhập email
2. Hệ thống tạo reset token
3. Gửi email chứa link reset
4. User click link và đặt mật khẩu mới
5. Cập nhật mật khẩu trong database

**Controllers**: `PasswordResetRequest.java`, `PasswordReset.java`  
**Util**: `EmailUtil.java`

---

### 3.2 Product Browsing & Shopping

#### UC-004: Duyệt Sản Phẩm
**Actor**: Guest, User  
**Luồng chính**:
1. User truy cập trang bộ sưu tập
2. Hệ thống load danh sách sản phẩm
3. Hiển thị sản phẩm với ảnh, tên, giá
4. User có thể tìm kiếm, lọc theo danh mục
5. Click vào sản phẩm để xem chi tiết

**Controller**: `ProductController.java`  
**DAO**: `ProductDB.getAllProducts()`, `ProductDB.searchProducts()`

#### UC-005: Xem Chi Tiết Sản Phẩm
**Actor**: Guest, User  
**Luồng chính**:
1. User click vào sản phẩm
2. Load thông tin sản phẩm (name, price, description, stock)
3. Load tất cả ảnh sản phẩm (gallery)
4. Hiển thị thông tin chi tiết
5. User có thể thêm vào giỏ hàng (nếu đã đăng nhập)

**Controller**: `productdetail.java`  
**DAO**: `ProductDB.getById()`, `ProductDB.getProductImages()`

#### UC-006: Thêm Vào Giỏ Hàng
**Actor**: Authenticated User  
**Luồng chính**:
1. User click "Thêm vào giỏ hàng"
2. Hệ thống lấy/cập nhật cart của user
3. Kiểm tra sản phẩm đã có trong cart chưa
4. Nếu có → Tăng quantity
5. Nếu chưa → Thêm item mới
6. Kiểm tra số lượng không vượt quá stock
7. Lưu vào database

**Controller**: `addToCart.java`  
**DAO**: `CartDB.addItem()`, `CartDB.getCartByUserId()`

#### UC-007: Quản Lý Giỏ Hàng
**Actor**: Authenticated User  
**Chức năng**:
- Xem giỏ hàng
- Cập nhật số lượng
- Xóa sản phẩm
- Chọn/bỏ chọn sản phẩm để thanh toán

**Controllers**: `cart.java`, `UpdateCartQuantity.java`, `removeFromCart.java`, `UpdateCartSelection.java`

---

### 3.3 Checkout & Payment

#### UC-008: Thanh Toán (Checkout)
**Actor**: Authenticated User  
**Luồng chính**:
1. User click "Thanh toán" từ giỏ hàng
2. Load các items đã chọn
3. Load địa chỉ giao hàng của user
4. Load phương thức vận chuyển
5. User chọn/điền địa chỉ giao hàng
6. User chọn phương thức vận chuyển
7. User có thể áp dụng mã giảm giá
8. Hệ thống tính tổng tiền (subtotal + shipping - discount)
9. User xác nhận đơn hàng
10. Tạo đơn hàng và order details
11. Redirect đến phương thức thanh toán

**Controller**: `Checkout.java`  
**DAO**: `CartDB.getCartItems()`, `ShippingAddressDB.getByUserId()`, `ShippingMethodDB.getAll()`, `OrderDB.createOrder()`

#### UC-009: Thanh Toán VNPay
**Actor**: User, VNPay Gateway  
**Luồng chính**:
1. User chọn "Thanh toán VNPay"
2. Hệ thống tạo payment request
3. Ký tên (signature) cho request
4. Redirect user đến trang VNPay
5. User thanh toán trên VNPay
6. VNPay redirect về `/payment/vnpay/return`
7. Hệ thống verify signature
8. Kiểm tra payment status
9. Nếu thành công → Update order status = "PAID"
10. Redirect đến trang xác nhận

**Controllers**: `VnPayCreate.java`, `VnPayReturn.java`, `VnPayIpn.java`  
**Util**: `VnPayConfig.java`

#### UC-010: Thanh Toán Ngân Hàng
**Actor**: Authenticated User  
**Luồng chính**:
1. User chọn "Thanh toán chuyển khoản"
2. Hiển thị thông tin tài khoản ngân hàng
3. User xác nhận đã chuyển khoản
4. Tạo đơn hàng với payment_status = "PENDING"
5. Admin sẽ xác nhận thanh toán sau

**Controller**: `BankPayment.java`

---

### 3.4 Order Management

#### UC-011: Xem Lịch Sử Đơn Hàng
**Actor**: Authenticated User  
**Luồng chính**:
1. User click "Lịch sử đơn hàng"
2. Hệ thống load tất cả đơn hàng của user
3. Hiển thị danh sách với: Order ID, Ngày, Tổng tiền, Status
4. User có thể click để xem chi tiết

**Controller**: `OrderHistory.java`  
**DAO**: `OrderDB.getOrdersByUserId()`

#### UC-012: Xem Chi Tiết Đơn Hàng
**Actor**: Authenticated User  
**Luồng chính**:
1. User click vào đơn hàng
2. Load thông tin đơn hàng (order details)
3. Hiển thị: Sản phẩm, Số lượng, Giá, Địa chỉ giao hàng, Phương thức thanh toán, Trạng thái

**Controller**: `OrderDetail.java`  
**DAO**: `OrderDB.getOrderById()`, `OrderDB.getOrderDetails()`

---

### 3.5 Discount & Voucher System

#### UC-013: Áp Dụng Mã Giảm Giá
**Actor**: Authenticated User  
**Luồng chính**:
1. User nhập mã giảm giá tại checkout
2. Hệ thống validate:
   - Mã có tồn tại không?
   - Mã còn hiệu lực không?
   - User đã có voucher này chưa?
   - Đã đạt minimum order chưa?
3. Nếu hợp lệ → Tính discount amount
4. Áp dụng vào tổng tiền
5. Hiển thị discount info

**Controller**: `ApplyPromotion.java`  
**DAO**: `DiscountDB.getByCode()`, `DiscountDB.validateDiscount()`

#### UC-014: Xem Voucher Của Tôi
**Actor**: Authenticated User  
**Luồng chính**:
1. User click "My Vouchers"
2. Load các voucher đã được gán cho user
3. Hiển thị: Mã, Mô tả, Giá trị, Trạng thái (UNUSED/USED/EXPIRED), Ngày hết hạn

**Controller**: `DiscountController.java` (action=myDiscounts)  
**DAO**: `DiscountDB.getUserVouchers()`

#### UC-015: Tự Động Gán Voucher (System)
**Actor**: System (Trigger)  
**Luồng chính**:
1. User hoàn thành đơn hàng
2. Trigger `tr_OrderCreated_AssignVouchers` được kích hoạt
3. Gọi stored procedure `sp_CheckAndAssignVouchers`
4. Kiểm tra điều kiện:
   - TOTAL_SPENT: Tổng tiền đã chi >= giá trị
   - ORDER_COUNT: Số đơn hàng >= giá trị
   - FIRST_ORDER: Đây là đơn hàng đầu tiên
5. Nếu đủ điều kiện → Gán voucher cho user
6. User nhận voucher trong "My Discounts"

**Database**: Trigger + Stored Procedure

---

### 3.6 Admin Functions

#### UC-016: Quản Lý Sản Phẩm
**Actor**: Admin  
**Chức năng**:
- Xem danh sách sản phẩm
- Tạo sản phẩm mới
- Sửa sản phẩm
- Xóa sản phẩm
- Upload nhiều ảnh cho sản phẩm
- Tìm kiếm sản phẩm

**Controller**: `Admin.java` (action=products), `ProductController.java`  
**DAO**: `ProductDB.getAllProducts()`, `ProductDB.createProduct()`, `ProductDB.updateProduct()`, `ProductDB.deleteProduct()`

#### UC-017: Quản Lý Đơn Hàng
**Actor**: Admin  
**Chức năng**:
- Xem tất cả đơn hàng
- Lọc theo status (PENDING, PROCESSING, SHIPPED, DELIVERED, CANCELLED)
- Lọc theo ngày (Today, Date Range)
- Xem chi tiết đơn hàng
- Cập nhật order status
- Nhập tracking number (khi SHIPPED)

**Controller**: `Admin.java` (action=orders), `OrderManagement.java`  
**DAO**: `OrderDB.getAllOrders()`, `OrderDB.getOrdersByStatus()`, `OrderDB.getOrdersByDate()`, `OrderDB.updateOrderStatus()`

#### UC-018: Quản Lý Người Dùng
**Actor**: Admin  
**Chức năng**:
- Xem danh sách users
- Xem chi tiết user
- Thay đổi role (USER/ADMIN)
- Khóa/Mở khóa user

**Controller**: `Admin.java` (action=users)  
**DAO**: `UserDB.getAllUsers()`, `UserDB.updateUserRole()`

#### UC-019: Quản Lý Mã Giảm Giá
**Actor**: Admin  
**Chức năng**:
- Tạo mã giảm giá mới
- Sửa mã giảm giá
- Xóa mã giảm giá
- Thiết lập điều kiện tự động gán
- Xem thống kê sử dụng

**Controller**: `Admin.java` (action=discounts), `DiscountController.java`  
**DAO**: `DiscountDB.createDiscount()`, `DiscountDB.updateDiscount()`, `DiscountDB.deleteDiscount()`

#### UC-020: Dashboard & Báo Cáo
**Actor**: Admin  
**Chức năng**:
- Xem doanh thu hôm nay
- Xem số đơn mới
- Xem số khách mới
- Xem sản phẩm hết hàng
- Xem biểu đồ doanh thu 7 ngày gần nhất

**Controller**: `Admin.java` (action=dashboard)  
**DAO**: `OrderDB.getTodayRevenue()`, `OrderDB.getTodayNewOrders()`, `UserDB.getTodayNewUsers()`, `ProductDB.getLowStockCount()`

---

### 3.7 Contact & Support

#### UC-021: Liên Hệ
**Actor**: User, Guest  
**Luồng chính**:
1. User điền form liên hệ
2. Gửi thông tin liên hệ
3. Lưu vào database
4. Admin xem và phản hồi

**Controller**: `lienhe.java`, `lienheManager.java`  
**DAO**: `lienheDAO.java`

---

## 🏗️ 4. KIẾN TRÚC HỆ THỐNG

### 4.1 Kiến Trúc Tổng Thể (MVC Pattern)

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│  JSP Pages (View/, admin/)              │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│         Controller Layer                │
│  Servlets (Controller/*)                │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│         Filter Layer                    │
│  AdminAuthFilter (Security)             │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│         Service Layer                   │
│  DAO Classes (DAO/*)                    │
└────────────────┬────────────────────────┘
                 │ JDBC
                 ▼
┌─────────────────────────────────────────┐
│         Database Layer                  │
│  SQL Server Database                    │
└─────────────────────────────────────────┘
```

### 4.2 Các Layer Chi Tiết

#### Presentation Layer (View)
- **User Pages**: `home.jsp`, `product-detail.jsp`, `cart.jsp`, `checkout.jsp`, `my-orders.jsp`, etc.
- **Admin Pages**: `admin/dashboard.jsp`, `admin/manage-products.jsp`, `admin/manage-orders.jsp`, etc.
- **Includes**: `header.jspf`, `footer.jspf`

#### Controller Layer
- **27 Servlets** xử lý các request từ client
- Mỗi servlet xử lý một chức năng cụ thể
- Sử dụng `@WebServlet` annotation để mapping URL

#### Service Layer (DAO)
- **11 DAO Classes** để truy cập database
- Mỗi DAO class tương ứng với một entity chính
- Sử dụng `PreparedStatement` để tránh SQL injection

#### Database Layer
- **13 Tables** chính
- **4 Stored Procedures** cho business logic phức tạp
- **4 Triggers** cho tự động hóa

---

## 🗄️ 5. DATABASE SCHEMA

### 5.1 Các Bảng Chính

#### Users
```sql
- user_id (PK)
- username (unique)
- email (unique)
- phone
- password
- role (USER/ADMIN)
- reset_token
- reset_token_expiry
- date_create
- avatar_url
```

#### Products
```sql
- product_id (PK)
- name
- price
- stock
- description
- image_url
- category_id (FK)
```

#### ProductImages
```sql
- image_id (PK)
- product_id (FK)
- image_url
- image_order
- created_at
```

#### Categories
```sql
- category_id (PK)
- name
- description
```

#### Orders
```sql
- order_id (PK)
- user_id (FK)
- order_date
- total_amount
- shipping_address_id (FK)
- shipping_method_id (FK)
- shipping_cost
- payment_method
- payment_status
- order_status (PENDING/PROCESSING/SHIPPED/DELIVERED/CANCELLED)
- tracking_number
- discount_id (FK)
- discount_amount
- notes
```

#### OrderDetails
```sql
- order_detail_id (PK)
- order_id (FK)
- product_id (FK)
- quantity
- price
```

#### Carts
```sql
- cart_id (PK)
- user_id (FK)
- created_at
- updated_at
```

#### CartItems
```sql
- cart_item_id (PK)
- cart_id (FK)
- product_id (FK)
- quantity
- price
- is_selected
- added_at
```

#### Discounts
```sql
- discount_id (PK)
- code (unique)
- name
- description
- discount_type (PERCENTAGE/FIXED_AMOUNT)
- discount_value
- min_order_amount
- max_discount_amount
- usage_limit
- used_count
- start_date
- end_date
- is_active
- condition_type (TOTAL_SPENT/ORDER_COUNT/FIRST_ORDER/SPECIAL_EVENT)
- condition_value
- special_event
- auto_assign
- assign_date
```

#### UserVouchers
```sql
- user_voucher_id (PK)
- user_id (FK)
- discount_id (FK)
- status (UNUSED/USED/EXPIRED)
- assigned_date
- used_date
- order_id (FK)
```

#### ShippingAddresses
```sql
- address_id (PK)
- user_id (FK)
- full_name
- phone
- address
- city
- district
- ward
- is_default
- created_at
```

#### ShippingMethods
```sql
- method_id (PK)
- name
- description
- cost
- estimated_days
- is_active
```

#### Banners
```sql
- banner_id (PK)
- image_path
- target_url
- is_active
- display_order
- created_at
```

#### Comments
```sql
- comment_id (PK)
- product_id (FK)
- user_id (FK)
- content
- rating
- created_at
```

#### CommentReplies
```sql
- reply_id (PK)
- comment_id (FK)
- user_id (FK)
- content
- created_at
```

### 5.2 Stored Procedures

1. **sp_CheckAndAssignVouchers**
   - Tự động gán voucher cho user dựa trên điều kiện
   - Kiểm tra: TOTAL_SPENT, ORDER_COUNT, FIRST_ORDER

2. **sp_AssignSpecialEventVouchers**
   - Gán voucher sự kiện cho tất cả user
   - Được gọi khi có sự kiện đặc biệt

3. **sp_UpdateExpiredVouchers**
   - Cập nhật voucher hết hạn
   - Set status = EXPIRED cho các voucher quá hạn

4. **sp_CalculateCartTotal**
   - Tính tổng tiền giỏ hàng
   - Bao gồm: subtotal, shipping, discount

### 5.3 Triggers

1. **tr_OrderCreated_AssignVouchers**
   - Kích hoạt khi tạo đơn hàng mới
   - Tự động gán voucher nếu user đủ điều kiện

2. **tr_UserVoucherUsed_UpdateDiscount**
   - Kích hoạt khi sử dụng voucher
   - Cập nhật số lượng voucher đã dùng

3. **tr_CartItemsUpdated_UpdateCartTime**
   - Kích hoạt khi cập nhật cart items
   - Cập nhật thời gian updated_at của cart

4. **tr_CartItemsInsert_CheckStock**
   - Kích hoạt khi thêm item vào cart
   - Kiểm tra tồn kho trước khi thêm

---

## 🔧 6. CÁC THÀNH PHẦN QUAN TRỌNG

### 6.1 Controllers (27 Servlets)

#### Authentication & Account
- `login.java` - Đăng nhập
- `signup.java` - Đăng ký
- `logout.java` - Đăng xuất
- `AccountManagement.java` - Quản lý tài khoản
- `ChangePassword.java` - Đổi mật khẩu
- `PasswordResetRequest.java` - Yêu cầu reset mật khẩu
- `PasswordReset.java` - Reset mật khẩu
- `AvatarUpload.java` - Upload avatar

#### Products & Shopping
- `ProductController.java` - CRUD sản phẩm (admin)
- `productdetail.java` - Chi tiết sản phẩm
- `cart.java` - Giỏ hàng
- `addToCart.java` - Thêm vào giỏ
- `removeFromCart.java` - Xóa khỏi giỏ
- `UpdateCartQuantity.java` - Cập nhật số lượng
- `UpdateCartSelection.java` - Cập nhật lựa chọn

#### Orders & Payment
- `Checkout.java` - Thanh toán
- `ShippingAddress.java` - Quản lý địa chỉ giao hàng
- `BankPayment.java` - Thanh toán ngân hàng
- `VnPayCreate.java` - Tạo giao dịch VNPay
- `VnPayReturn.java` - Xử lý VNPay return
- `VnPayIpn.java` - Xử lý VNPay IPN
- `PaymentCallback.java` - Callback tổng quát
- `OrderHistory.java` - Lịch sử đơn hàng
- `OrderDetail.java` - Chi tiết đơn hàng
- `ApplyPromotion.java` - Áp dụng mã giảm giá

#### Admin
- `Admin.java` - Controller admin chính (Dashboard, Products, Orders, Users, Categories, Discounts, Banners)
- `OrderManagement.java` - Quản lý đơn hàng
- `DiscountController.java` - Quản lý mã giảm giá

#### Utilities
- `Image.java` - Phục vụ hình ảnh
- `lienhe.java` - Liên hệ
- `lienheManager.java` - Quản lý liên hệ (admin)
- `UpdateLienheStatus.java` - Cập nhật trạng thái liên hệ

#### Comments
- `CommentServlet.java` - Quản lý bình luận

### 6.2 DAO Classes (11 Classes)

- `DBConnect.java` - Kết nối database (Singleton pattern)
- `UserDB.java` - CRUD users, authentication
- `ProductDB.java` - CRUD products, search, get images
- `CategoryDB.java` - CRUD categories
- `OrderDB.java` - CRUD orders, order details, statistics
- `CartDB.java` - Quản lý giỏ hàng
- `DiscountDB.java` - Quản lý mã giảm giá, vouchers
- `ShippingAddressDB.java` - Quản lý địa chỉ giao hàng
- `ShippingMethodDB.java` - Quản lý phương thức giao hàng
- `BannerDB.java` - Quản lý banner
- `CommentDB.java` - Quản lý bình luận
- `lienheDAO.java` - Xử lý liên hệ

### 6.3 Model Classes (18 Entities)

#### User Models
- `user.java` - User entity với role support
- `UserDiscountAssign.java` - Voucher đã gán cho user

#### Product Models
- `Product.java` - Product entity với multiple images
- `Category.java` - Category entity
- `Banner.java` - Banner entity

#### Order Models
- `Order.java` - Order entity
- `OrderDetail.java` - Order detail item
- `OrderItemSummary.java` - Summary cho UI

#### Cart Models
- `Cart.java` - Cart entity
- `CartItems.java` - Cart item entity
- `CheckoutItem.java` - Checkout item

#### Discount Models
- `Discount.java` - Discount/Voucher entity

#### Shipping Models
- `ShippingAddress.java` - Shipping address entity

#### Comment Models
- `Comment.java` - Comment entity
- `CommentMedia.java` - Comment media entity
- `CommentReply.java` - Comment reply entity
- `ReplyMedia.java` - Reply media entity

#### Contact Models
- `lienhe.java` - Contact message entity

### 6.4 Utilities (4 Classes)

- `EmailUtil.java` - Gửi email (password reset)
- `CartCookieUtil.java` - Quản lý giỏ hàng qua cookie (guest users)
- `VnPayConfig.java` - Cấu hình tích hợp VNPay
- `PaymentClient.java` - Client xử lý payment callbacks

### 6.5 Filters (1 Class)

- `AdminAuthFilter.java` - Filter bảo vệ các trang admin, kiểm tra authentication và role

---

## 🔄 7. LUỒNG DỮ LIỆU CHÍNH

### 7.1 User Authentication Flow
```
User → LoginServlet → UserDB → Database
     ← Session ← User Object ←
```

### 7.2 Product Browsing Flow
```
User → ProductController → ProductDB → Database
     ← Product List ←
```

### 7.3 Shopping Cart Flow
```
User → AddToCartServlet → CartDB → Database
     ← Cart Updated ←
```

### 7.4 Checkout Flow
```
User → CheckoutServlet → CartDB → Get Cart Items
                     → ShippingAddressDB → Get Addresses
                     → ShippingMethodDB → Get Methods
                     → DiscountDB → Validate Discount
                     → OrderDB → Create Order
     ← Order Created ←
```

### 7.5 Payment Flow (VNPay)
```
User → VnPayCreateServlet → VnPayConfig → Build Payment URL
     ← Redirect to VNPay ←
User → Pay on VNPay
VNPay → VnPayReturnServlet → Verify Signature → Update Order
     ← Order Confirmation ←
```

### 7.6 Admin Flow
```
Admin → AdminAuthFilter → Check Authentication & Role
     ← Allow/Deny ←
Admin → AdminServlet → ProductDB/OrderDB/UserDB → Database
     ← Data ←
```

---

## 🔐 8. BẢO MẬT

### 8.1 Authentication
- Session-based authentication
- Session timeout: 30 phút
- Password hashing (trong database)

### 8.2 Authorization
- Role-based access control (USER/ADMIN)
- `AdminAuthFilter` bảo vệ các trang admin
- Kiểm tra role trước khi truy cập admin functions

### 8.3 Input Validation
- Server-side validation
- SQL injection prevention (PreparedStatement)
- XSS prevention (JSTL escaping)

### 8.4 Session Security
- Session timeout
- HTTPS recommended
- Cookie security

---

## 📊 9. THỐNG KÊ DỰ ÁN

- **Controllers**: 27 servlets
- **Models**: 18 entities
- **DAOs**: 11 classes
- **Views**: 35+ JSP pages
- **Database Tables**: 13+ tables
- **Stored Procedures**: 4
- **Triggers**: 4
- **Utilities**: 4 classes
- **Filters**: 1 class

---

## 🚀 10. DEPLOYMENT

### 10.1 Requirements
- Java 17+
- Maven 3.6+
- SQL Server
- Jakarta EE compatible server (Tomcat 10+, GlassFish, etc.)

### 10.2 Build
```bash
mvn clean package
```

### 10.3 Deploy
- Copy WAR file từ `target/` đến thư mục webapps của Tomcat
- Hoặc sử dụng IDE để chạy trực tiếp

### 10.4 Configuration
- Database connection: `DAO/DBConnect.java`
- Email config: `email-config.properties`
- VNPay config: `Util/VnPayConfig.java`

---

## 📝 11. TÀI LIỆU THAM KHẢO

- `PROJECT_DOCUMENTATION.md` - Tài liệu chi tiết đầy đủ
- `ARCHITECTURE.md` - Kiến trúc hệ thống
- `USECASE_AND_DATAFLOW.md` - Use cases và luồng dữ liệu
- `ROLE_SYSTEM_GUIDE.md` - Hướng dẫn hệ thống phân quyền
- `README_VI.md` - Hướng dẫn sử dụng

---

**Phiên bản**: 1.0  
**Cập nhật**: 2024  
**Tác giả**: SWP Project Team

