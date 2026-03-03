using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.DbContexts;

namespace Infrastructure.Repositories;

public class CourtRepository : GenericRepository<Court>, ICourtRepository
{
    public CourtRepository(BadmintonBooking_PRM393Context context) : base(context)
    {
    }
}
