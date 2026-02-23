using Domain.Entities;

namespace Application.Interfaces.IRepositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
