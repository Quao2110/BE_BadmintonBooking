using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CourtRepository : GenericRepository<Court>, ICourtRepository
{
    private readonly BadmintonBooking_PRM393Context _context;

    public CourtRepository(BadmintonBooking_PRM393Context context) : base(context)
    {
        _context = context;
    }

    public override async Task<IEnumerable<Court>> GetAllAsync()
    {
        return await _context.Courts
            .Include(c => c.CourtImages)
            .ToListAsync();
    }

    public override async Task<Court?> GetByIdAsync(Guid? id)
    {
        return await _context.Courts
            .Include(c => c.CourtImages)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
