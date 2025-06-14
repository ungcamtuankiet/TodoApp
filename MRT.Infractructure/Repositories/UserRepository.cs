﻿using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using MRT.Application.Interfaces;
using MRT.Application.Repositories;
using MRT.Domain.Entities;
using MRT.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<ApplicationUser>, IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext,
            ICurrentTime timeService,
            IClaimsService claimsService)
            : base(dbContext,
                  timeService,
                  claimsService)
        {
            _dbContext = dbContext;
        }
        public async Task<IList<ApplicationUser>> GetALl()
        {
            return await _dbContext.User.Where(u => u.RoleId != 1).ToListAsync();
        }
        public async Task<ApplicationUser> FindByEmail(string email)
        {
            return await _dbContext.User
           .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser> AddAsync(ApplicationUser user)
        {
            await _dbContext.User.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            _dbContext.User.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            return await _dbContext.User.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsAsync(Expression<Func<ApplicationUser, bool>> predicate)
        {
            return await _dbContext.User.AnyAsync(predicate);
        }

        public async Task<ApplicationUser> GetUserById(int userId)
        {
            var user = await _dbContext.User.FindAsync(userId);
            return user;
        }

        public async Task<ApplicationUser> GetAllUserById(Guid id)
        {
            return await _dbContext.User.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<ApplicationUser> GetUserById(Guid userId)
        {
            return await _dbContext.User.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<ApplicationUser> GetUserByVerificationToken(string token)
        {
            return await _dbContext.User.FirstOrDefaultAsync(u => u.VerificationToken == token);
        }
        public async Task<ApplicationUser> GetUserByResetToken(string resetToken)
        {
            return await _dbContext.User.FirstOrDefaultAsync(u => u.ResetToken == resetToken);
        }

        public async Task<List<ApplicationUser>> GetUsersByRole(int role)
        {
            return await _dbContext.User.Where(u => u.RoleId == role).ToListAsync();
        }

        public async Task<ApplicationUser?> FindByLoginAsync(string provider, string key)
        {
            return await _dbContext.User
            .Where(u => u.Provider == provider && u.ProviderKey == key)
            .FirstOrDefaultAsync();
        }
    }
}
