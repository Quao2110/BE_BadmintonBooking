using Application.DTOs.ResponseDTOs.User;
using Application.DTOs.ResponseDTOs.Category;
using Application.DTOs.ResponseDTOs.Product;
using Application.DTOs.ResponseDTOs.ProductImage;
using Application.DTOs.ResponseDTOs.Service;
using Application.DTOs.ResponseDTOs.Court;
using Application.DTOs.ResponseDTOs.CourtImage;
using Application.DTOs.ResponseDTOs.Shop;
using Application.DTOs.ResponseDTOs.Notification;
using Application.DTOs.ResponseDTOs.Booking;
using Application.DTOs.ResponseDTOs.Cart;
using Application.DTOs.ResponseDTOs.Order;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponse>();
            CreateMap<Category, CategoryResponse>();
            CreateMap<Service, ServiceResponse>();
            CreateMap<ProductImage, ProductImageResponse>();
            CreateMap<Product, ProductResponse>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null));

            CreateMap<Court, CourtResponse>();
            CreateMap<CourtImage, CourtImageResponse>();
            CreateMap<Shop, ShopResponse>()
                .ForMember(dest => dest.ImageUrls,
                    opt => opt.MapFrom(src => src.ShopImages.Select(si => si.ImageUrl).ToList()));
            CreateMap<Notification, NotificationResponse>();
            CreateMap<Booking, BookingHistoryItemResponse>()
                .ForMember(dest => dest.CourtName,
                    opt => opt.MapFrom(src => src.Court != null ? src.Court.CourtName : string.Empty))
                .ForMember(dest => dest.TotalPrice,
                    opt => opt.MapFrom(src => src.TotalPrice ?? 0))
                .ForMember(dest => dest.IsPaid,
                    opt => opt.MapFrom(src => src.IsPaid ?? false));

            // Cart mappings
            CreateMap<CartItem, CartItemResponse>();
            CreateMap<Cart, CartResponse>();

            // Order mappings
            CreateMap<OrderDetail, OrderDetailResponse>();
            CreateMap<Order, OrderResponse>();
        }
    }
}
