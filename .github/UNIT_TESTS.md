# 🧪 Unit Test Examples - Cart & Order Services

## Setup (using Moq & xUnit)

```bash
dotnet add package Moq --version 4.18.0
dotnet add package xunit --version 2.4.2
dotnet add package xunit.runner.visualstudio --version 2.4.5
```

---

## Test Case 1: CartService - Add to Cart (Happy Path)

```csharp
[Fact]
public async Task AddToCartAsync_ValidRequest_ShouldAddProductToCart()
{
    // Arrange
    var userId = Guid.NewGuid();
    var productId = Guid.NewGuid();
    
    var product = new Product
    {
        Id = productId,
        ProductName = "Vợt Badminton",
        Price = 500000,
        StockQuantity = 10
    };
    
    var cart = new Cart
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        CartItems = new List<CartItem>()
    };
    
    var request = new AddToCartRequest
    {
        ProductId = productId,
        Quantity = 2
    };
    
    // Mock repositories
    var cartRepoMock = new Mock<ICartRepository>();
    var cartItemRepoMock = new Mock<ICartItemRepository>();
    var productRepoMock = new Mock<IProductRepository>();
    var mapperMock = new Mock<IMapper>();
    var unitOfWorkMock = new Mock<IUnitOfWork>();
    
    cartRepoMock
        .Setup(r => r.GetByUserIdWithIncludesAsync(userId))
        .ReturnsAsync(cart);
    
    cartItemRepoMock
        .Setup(r => r.GetByCartIdAndProductIdAsync(cart.Id, productId))
        .ReturnsAsync((CartItem)null);
    
    productRepoMock
        .Setup(r => r.GetByIdAsync(productId))
        .ReturnsAsync(product);
    
    unitOfWorkMock.Setup(u => u.CartRepository).Returns(cartRepoMock.Object);
    unitOfWorkMock.Setup(u => u.CartItemRepository).Returns(cartItemRepoMock.Object);
    unitOfWorkMock.Setup(u => u.ProductRepository).Returns(productRepoMock.Object);
    
    var service = new CartService(unitOfWorkMock.Object, mapperMock.Object);
    
    // Act
    var result = await service.AddToCartAsync(userId, request);
    
    // Assert
    Assert.NotNull(result);
    cartItemRepoMock.Verify(r => r.CreateAsync(It.IsAny<CartItem>()), Times.Once);
    unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
}
```

---

## Test Case 2: CartService - Add to Cart (Insufficient Stock)

```csharp
[Fact]
public async Task AddToCartAsync_InsufficientStock_ShouldThrowException()
{
    // Arrange
    var userId = Guid.NewGuid();
    var productId = Guid.NewGuid();
    
    var product = new Product
    {
        Id = productId,
        ProductName = "Vợt Badminton",
        Price = 500000,
        StockQuantity = 2  // Only 2 items
    };
    
    var cart = new Cart
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        CartItems = new List<CartItem>()
    };
    
    var request = new AddToCartRequest
    {
        ProductId = productId,
        Quantity = 5  // Requesting 5
    };
    
    var cartRepoMock = new Mock<ICartRepository>();
    var productRepoMock = new Mock<IProductRepository>();
    var unitOfWorkMock = new Mock<IUnitOfWork>();
    var mapperMock = new Mock<IMapper>();
    
    cartRepoMock
        .Setup(r => r.GetByUserIdWithIncludesAsync(userId))
        .ReturnsAsync(cart);
    
    productRepoMock
        .Setup(r => r.GetByIdAsync(productId))
        .ReturnsAsync(product);
    
    unitOfWorkMock.Setup(u => u.CartRepository).Returns(cartRepoMock.Object);
    unitOfWorkMock.Setup(u => u.ProductRepository).Returns(productRepoMock.Object);
    
    var service = new CartService(unitOfWorkMock.Object, mapperMock.Object);
    
    // Act & Assert
    await Assert.ThrowsAsync<Exception>(
        () => service.AddToCartAsync(userId, request)
    );
}
```

---

## Test Case 3: OrderService - Checkout (Successful)

```csharp
[Fact]
public async Task CheckoutAsync_ValidCart_ShouldCreateOrder()
{
    // Arrange
    var userId = Guid.NewGuid();
    var productId = Guid.NewGuid();
    
    var product = new Product
    {
        Id = productId,
        ProductName = "Vợt Badminton",
        Price = 500000,
        StockQuantity = 10,
        Category = new Category { CategoryName = "Sports" }
    };
    
    var cartItem = new CartItem
    {
        Id = Guid.NewGuid(),
        CartId = Guid.NewGuid(),
        ProductId = productId,
        Quantity = 2,
        Product = product
    };
    
    var cart = new Cart
    {
        Id = cartItem.CartId,
        UserId = userId,
        CartItems = new List<CartItem> { cartItem }
    };
    
    var request = new CheckoutRequest
    {
        DeliveryAddress = "123 Đường ABC",
        DeliveryLatitude = 21.0285,
        DeliveryLongitude = 105.8542,
        PaymentMethod = "COD"
    };
    
    // Mock all repositories and UnitOfWork
    var cartRepoMock = new Mock<ICartRepository>();
    var cartItemRepoMock = new Mock<ICartItemRepository>();
    var orderRepoMock = new Mock<IOrderRepository>();
    var orderDetailRepoMock = new Mock<IOrderDetailRepository>();
    var productRepoMock = new Mock<IProductRepository>();
    var mapperMock = new Mock<IMapper>();
    var unitOfWorkMock = new Mock<IUnitOfWork>();
    
    cartRepoMock
        .Setup(r => r.GetByUserIdWithIncludesAsync(userId))
        .ReturnsAsync(cart);
    
    productRepoMock
        .Setup(r => r.GetByIdAsync(productId))
        .ReturnsAsync(product);
    
    var createdOrder = new Order
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        DeliveryAddress = request.DeliveryAddress,
        TotalAmount = 1000000,
        OrderStatus = "Pending",
        PaymentStatus = "Pending",
        OrderDetails = new List<OrderDetail>()
    };
    
    orderRepoMock
        .Setup(r => r.GetByIdWithIncludesAsync(It.IsAny<Guid>()))
        .ReturnsAsync(createdOrder);
    
    mapperMock
        .Setup(m => m.Map<OrderResponse>(createdOrder))
        .Returns(new OrderResponse
        {
            Id = createdOrder.Id,
            UserId = createdOrder.UserId,
            TotalAmount = createdOrder.TotalAmount,
            OrderStatus = createdOrder.OrderStatus
        });
    
    unitOfWorkMock.Setup(u => u.CartRepository).Returns(cartRepoMock.Object);
    unitOfWorkMock.Setup(u => u.CartItemRepository).Returns(cartItemRepoMock.Object);
    unitOfWorkMock.Setup(u => u.OrderRepository).Returns(orderRepoMock.Object);
    unitOfWorkMock.Setup(u => u.OrderDetailRepository).Returns(orderDetailRepoMock.Object);
    unitOfWorkMock.Setup(u => u.ProductRepository).Returns(productRepoMock.Object);
    
    var service = new OrderService(unitOfWorkMock.Object, mapperMock.Object);
    
    // Act
    var result = await service.CheckoutAsync(userId, request);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(createdOrder.Id, result.Id);
    Assert.Equal("Pending", result.OrderStatus);
    
    // Verify all operations were called
    orderRepoMock.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Once);
    orderDetailRepoMock.Verify(r => r.CreateAsync(It.IsAny<OrderDetail>()), Times.Once);
    unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
}
```

---

## Test Case 4: OrderService - Checkout (Empty Cart)

```csharp
[Fact]
public async Task CheckoutAsync_EmptyCart_ShouldThrowException()
{
    // Arrange
    var userId = Guid.NewGuid();
    var cart = new Cart
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        CartItems = new List<CartItem>()  // Empty
    };
    
    var request = new CheckoutRequest
    {
        DeliveryAddress = "123 Đường ABC",
        PaymentMethod = "COD"
    };
    
    var cartRepoMock = new Mock<ICartRepository>();
    var unitOfWorkMock = new Mock<IUnitOfWork>();
    var mapperMock = new Mock<IMapper>();
    
    cartRepoMock
        .Setup(r => r.GetByUserIdWithIncludesAsync(userId))
        .ReturnsAsync(cart);
    
    unitOfWorkMock.Setup(u => u.CartRepository).Returns(cartRepoMock.Object);
    
    var service = new OrderService(unitOfWorkMock.Object, mapperMock.Object);
    
    // Act & Assert
    await Assert.ThrowsAsync<Exception>(
        () => service.CheckoutAsync(userId, request)
    );
}
```

---

## Test Case 5: OrderService - Cancel Order (with Restock)

```csharp
[Fact]
public async Task CancelOrderAsync_ValidOrder_ShouldRestoreStock()
{
    // Arrange
    var orderId = Guid.NewGuid();
    var productId = Guid.NewGuid();
    
    var product = new Product
    {
        Id = productId,
        ProductName = "Vợt Badminton",
        StockQuantity = 7  // After order of 3
    };
    
    var orderDetail = new OrderDetail
    {
        Id = Guid.NewGuid(),
        OrderId = orderId,
        ProductId = productId,
        Quantity = 3,
        UnitPrice = 500000
    };
    
    var order = new Order
    {
        Id = orderId,
        UserId = Guid.NewGuid(),
        OrderStatus = "Pending",
        OrderDetails = new List<OrderDetail> { orderDetail }
    };
    
    var orderRepoMock = new Mock<IOrderRepository>();
    var orderDetailRepoMock = new Mock<IOrderDetailRepository>();
    var productRepoMock = new Mock<IProductRepository>();
    var mapperMock = new Mock<IMapper>();
    var unitOfWorkMock = new Mock<IUnitOfWork>();
    
    orderRepoMock
        .Setup(r => r.GetByIdAsync(orderId))
        .ReturnsAsync(order);
    
    orderDetailRepoMock
        .Setup(r => r.GetByOrderIdAsync(orderId))
        .ReturnsAsync(new List<OrderDetail> { orderDetail });
    
    productRepoMock
        .Setup(r => r.GetByIdAsync(productId))
        .ReturnsAsync(product);
    
    var cancelledOrder = new Order
    {
        Id = orderId,
        OrderStatus = "Cancelled",
        OrderDetails = new List<OrderDetail> { orderDetail }
    };
    
    orderRepoMock
        .Setup(r => r.GetByIdWithIncludesAsync(orderId))
        .ReturnsAsync(cancelledOrder);
    
    mapperMock
        .Setup(m => m.Map<OrderResponse>(cancelledOrder))
        .Returns(new OrderResponse
        {
            Id = orderId,
            OrderStatus = "Cancelled"
        });
    
    unitOfWorkMock.Setup(u => u.OrderRepository).Returns(orderRepoMock.Object);
    unitOfWorkMock.Setup(u => u.OrderDetailRepository).Returns(orderDetailRepoMock.Object);
    unitOfWorkMock.Setup(u => u.ProductRepository).Returns(productRepoMock.Object);
    
    var service = new OrderService(unitOfWorkMock.Object, mapperMock.Object);
    
    // Act
    var result = await service.CancelOrderAsync(orderId);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Cancelled", result.OrderStatus);
    
    // Verify stock was restored
    Assert.Equal(10, product.StockQuantity);  // 7 + 3 = 10
    productRepoMock.Verify(r => r.Update(product), Times.Once);
    unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
}
```

---

## Test Case 6: OrderService - Update Order Status

```csharp
[Fact]
public async Task UpdateOrderStatusAsync_ValidStatus_ShouldUpdateOrder()
{
    // Arrange
    var orderId = Guid.NewGuid();
    var order = new Order
    {
        Id = orderId,
        UserId = Guid.NewGuid(),
        OrderStatus = "Pending"
    };
    
    var orderRepoMock = new Mock<IOrderRepository>();
    var orderDetailRepoMock = new Mock<IOrderDetailRepository>();
    var mapperMock = new Mock<IMapper>();
    var unitOfWorkMock = new Mock<IUnitOfWork>();
    
    orderRepoMock
        .Setup(r => r.GetByIdAsync(orderId))
        .ReturnsAsync(order);
    
    var updatedOrder = new Order
    {
        Id = orderId,
        OrderStatus = "Confirmed"
    };
    
    orderRepoMock
        .Setup(r => r.GetByIdWithIncludesAsync(orderId))
        .ReturnsAsync(updatedOrder);
    
    mapperMock
        .Setup(m => m.Map<OrderResponse>(updatedOrder))
        .Returns(new OrderResponse
        {
            Id = orderId,
            OrderStatus = "Confirmed"
        });
    
    unitOfWorkMock.Setup(u => u.OrderRepository).Returns(orderRepoMock.Object);
    unitOfWorkMock.Setup(u => u.OrderDetailRepository).Returns(orderDetailRepoMock.Object);
    
    var service = new OrderService(unitOfWorkMock.Object, mapperMock.Object);
    
    // Act
    var result = await service.UpdateOrderStatusAsync(orderId, "Confirmed");
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Confirmed", result.OrderStatus);
    orderRepoMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Once);
}
```

---

## Integration Test Example (with Real Database)

```csharp
[Fact]
public async Task CheckoutAsync_WithRealDatabase_ShouldCreateOrderAndReduceStock()
{
    // Arrange - Use test database or in-memory database
    var options = new DbContextOptionsBuilder<BadmintonBooking_PRM393Context>()
        .UseInMemoryDatabase(databaseName: "test_checkout_db")
        .Options;
    
    using (var context = new BadmintonBooking_PRM393Context(options))
    {
        // Create test data
        var product = new Product
        {
            Id = Guid.NewGuid(),
            ProductName = "Test Product",
            Price = 100000,
            StockQuantity = 10
        };
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Test User",
            Email = "test@example.com"
        };
        
        context.Products.Add(product);
        context.Users.Add(user);
        context.SaveChanges();
        
        // Create cart with item
        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = Guid.NewGuid(),
            ProductId = product.Id,
            Quantity = 3
        };
        
        var cart = new Cart
        {
            Id = cartItem.CartId,
            UserId = user.Id,
            CartItems = new List<CartItem> { cartItem }
        };
        
        context.Carts.Add(cart);
        context.SaveChanges();
        
        // Create service with real UnitOfWork
        var unitOfWork = new UnitOfWork.UnitOfWork(context);
        var mapper = new AutoMapper.Mapper(new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()));
        var service = new OrderService(unitOfWork, mapper);
        
        var request = new CheckoutRequest
        {
            DeliveryAddress = "Test Address",
            PaymentMethod = "COD"
        };
        
        // Act
        var result = await service.CheckoutAsync(user.Id, request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Pending", result.OrderStatus);
        
        // Verify stock was reduced
        var updatedProduct = context.Products.FirstOrDefault(p => p.Id == product.Id);
        Assert.Equal(7, updatedProduct.StockQuantity);  // 10 - 3 = 7
        
        // Verify cart was cleared
        var updatedCart = context.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.Id == cart.Id);
        Assert.Empty(updatedCart.CartItems);
    }
}
```

---

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "CartServiceTests"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

---

## Mocking Best Practices

✅ **DO:**
- Mock external dependencies (IRepository, IMapper)
- Test one thing per test
- Use meaningful test names
- Verify behavior, not implementation details
- Create test data builders for complex objects

❌ **DON'T:**
- Mock the class you're testing (SUT - System Under Test)
- Have multiple assertions for different scenarios
- Create tests with random data
- Test private methods directly
- Make tests dependent on each other

---

