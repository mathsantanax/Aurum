using Aurum.Domain.Entities;
using Aurum.Domain.Repositories;
using Aurum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;
        public RefreshTokenRepository(AppDbContext db) => _db = db;


        public async Task AddAsync(RefreshToken token)
        {
            await _db.RefreshTokens.AddAsync(token);
            await _db.SaveChangesAsync();
        }


        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
        }


        public async Task UpdateAsync(RefreshToken token)
        {
            _db.RefreshTokens.Update(token);
            await _db.SaveChangesAsync();
        }
    }
}
