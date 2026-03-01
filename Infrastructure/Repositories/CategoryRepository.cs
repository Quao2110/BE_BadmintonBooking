using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.DbContexts;

namespace Infrastructure.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(BadmintonBooking_PRM393Context context) : base(context)
    {
    }
}
