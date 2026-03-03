using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.DbContexts;

namespace Infrastructure.Repositories;

public class CourtImageRepository : GenericRepository<CourtImage>, ICourtImageRepository
{
    public CourtImageRepository(BadmintonBooking_PRM393Context context) : base(context)
    {
    }
}
