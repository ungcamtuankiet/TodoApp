using MRT.Application;
using MRT.Application.Repositories;
using MRT.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;

        public UnitOfWork(AppDbContext dbContext, IAuthRepository authRepository, IUserRepository userRepository)
        {
            _dbContext = dbContext;
            _authRepository = authRepository;
            _userRepository = userRepository;
        }
        public IAuthRepository authRepository => _authRepository;
        public IUserRepository userRepository => _userRepository;

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
