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

        public async Task DeleteUser(User user)
        {
            try
            {
                if(user == null)
                    throw new ArgumentNullException("Usuário está inválido" + nameof(user));

                var userData = await _context.Users.FirstOrDefaultAsync(u => u.Guid == user.Guid);
                if(userData == null)
                    throw new ArgumentNullException("Usuário não cadastrado");

                _context.Users.Remove(userData);
                await _context.SaveChangesAsync();
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException("Erro de comunicação com o banco de dados PostgreSQL." + ex.Message);
            }
            catch (Exception ex)
            {
               throw new InvalidOperationException("Erro inesperado ao Deletar o usuário." + ex.Message);
            }

        }

        public async Task<User> GetUser(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user), "O objeto de usuário não pode ser nulo.");

                if (user.Guid == Guid.Empty && string.IsNullOrWhiteSpace(user.PhoneNumber))
                    throw new ArgumentException("Você deve informar o GUID ou o número de telefone para buscar o usuário.");

                User? userData = null;

                if (user.Guid != Guid.Empty)
                    userData = await _context.Users
                                    .Where(u => u.Guid.Equals(user.Guid))
                                    .Include(w => w.Wallets)
                                    .FirstOrDefaultAsync(u => u.Guid == user.Guid);

                else if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                    userData = await _context.Users
                                    .Where(u => u.PhoneNumber.Equals(user.PhoneNumber))
                                    .Include(w => w.Wallets)
                                    .FirstOrDefaultAsync(u => u.PhoneNumber == user.PhoneNumber);

                if (userData == null)
                    throw new KeyNotFoundException("Usuário não encontrado.");

                return userData;
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException("Erro de comunicação com o banco de dados PostgreSQL."+ ex.Message);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro inesperado ao buscar o usuário."+ ex.Message);
            }
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
                throw new InvalidOperationException("Erro ao atualizar o usuário no banco de dados."+ ex.Message);
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException("Erro de comunicação com o banco de dados PostgreSQL." + ex.Message);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro inesperado ao atualizar o usuário." + ex.Message);
            }
        }
    }
}
