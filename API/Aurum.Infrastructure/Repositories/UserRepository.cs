using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AurumDbContext _context;
        public UserRepository(AurumDbContext context)
        {
            this._context = context;
        }

        public async Task AddUser(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            catch(NpgsqlException ex)
            {
                throw new ArgumentException(ex.Message);
            }
            catch(Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }

        }

        public Task DeleteUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUser(User user)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateUser(User user)
        {
            try
            {
                var existingUser = await _context.Users.FindAsync(user.Guid);

                if (existingUser == null)
                    throw new KeyNotFoundException($"Usuário com ID {user.Guid} não encontrado.");

                _context.Entry(existingUser).CurrentValues.SetValues(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Erro ao atualizar o usuário no banco de dados.", ex);
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException("Erro de comunicação com o banco de dados PostgreSQL.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro inesperado ao atualizar o usuário.", ex);
            }
        }
    }
}
