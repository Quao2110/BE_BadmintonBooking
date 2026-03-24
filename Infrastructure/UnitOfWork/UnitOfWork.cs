﻿using Application.Interfaces.IRepositories;
using Application.Interfaces.IUnitOfWork;
using Infrastructure.DbContexts;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly BadmintonBooking_PRM393Context _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;
    private IUserRepository? _users;
    private IBookingRepository? _bookings;
    private ICategoryRepository? _categories;
    private IServiceRepository? _services;
    private IProductRepository? _products;
    private IProductImageRepository? _productImages;
    private ICourtRepository? _courts;
    private ICourtImageRepository? _courtImages;
    private IShopRepository? _shops;
    private INotificationRepository? _notifications;
    private ICartRepository? _carts;
    private ICartItemRepository? _cartItems;
    private IOrderRepository? _orders;
    private IOrderDetailRepository? _orderDetails;

    public UnitOfWork(BadmintonBooking_PRM393Context context)
    {
        _context = context;
    }

    public IUserRepository UserRepository => _users ??= new UserRepository(_context);
    public IBookingRepository BookingRepository => _bookings ??= new BookingRepository(_context);
    public ICategoryRepository CategoryRepository => _categories ??= new CategoryRepository(_context);
    public IServiceRepository ServiceRepository => _services ??= new ServiceRepository(_context);
    public IProductRepository ProductRepository => _products ??= new ProductRepository(_context);
    public IProductImageRepository ProductImageRepository => _productImages ??= new ProductImageRepository(_context);
    public ICourtRepository CourtRepository => _courts ??= new CourtRepository(_context);
    public ICourtImageRepository CourtImageRepository => _courtImages ??= new CourtImageRepository(_context);
    public IShopRepository ShopRepository => _shops ??= new ShopRepository(_context);
    public INotificationRepository NotificationRepository => _notifications ??= new NotificationRepository(_context);
    public ICartRepository CartRepository => _carts ??= new CartRepository(_context);
    public ICartItemRepository CartItemRepository => _cartItems ??= new CartItemRepository(_context);
    public IOrderRepository OrderRepository => _orders ??= new OrderRepository(_context);
    public IOrderDetailRepository OrderDetailRepository => _orderDetails ??= new OrderDetailRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task CommitAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackAsync();
            throw; 
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    private async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}
