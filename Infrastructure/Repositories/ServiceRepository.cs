using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.DbContexts;

namespace Infrastructure.Repositories;

public class ServiceRepository : GenericRepository<Service>, IServiceRepository
{
    public ServiceRepository(BadmintonBooking_PRM393Context context) : base(context)
    {
    }
}
