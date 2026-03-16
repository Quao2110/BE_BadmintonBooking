using Application.Interfaces.IRepositories;

namespace Application.Interfaces.IUnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserRepository { get; }
    IBookingRepository BookingRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    IServiceRepository ServiceRepository { get; }
    IProductRepository ProductRepository { get; }
    IProductImageRepository ProductImageRepository { get; }
    ICourtRepository CourtRepository { get; }
    ICourtImageRepository CourtImageRepository { get; }
    IShopRepository ShopRepository { get; }
    INotificationRepository NotificationRepository { get; }
    ICartRepository CartRepository { get; }
    ICartItemRepository CartItemRepository { get; }
    IOrderRepository OrderRepository { get; }
    IOrderDetailRepository OrderDetailRepository { get; }
    Task<int> SaveChangesAsync();
    Task CommitAsync();
}
