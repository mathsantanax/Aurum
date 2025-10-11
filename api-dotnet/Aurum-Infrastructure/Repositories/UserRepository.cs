using Aurum_Domain.Entities;
using Aurum_Domain.Interfaces;
using Aurum_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly InfraContext _infraContext;

        public UserRepository(InfraContext infraContext)
        {
            _infraContext = infraContext;
        }

        public async Task AddAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "Usuário não pode ser nulo.");
            try
            {
                await _infraContext.Users.AddAsync(user);
                await _infraContext.SaveChangesAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException("Já existe um usuário com os mesmos dados únicos.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Erro ao salvar o usuário no banco de dados.", ex);
            }
        }

        public async Task DeleteAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "Usuário não pode ser nulo.");

            try
            {
                var existingUser = await _infraContext.Users.FindAsync(user.Id);
                if (existingUser == null)
                    throw new KeyNotFoundException($"Usuário com Id '{user.Id}' não encontrado.");

                _infraContext.Users.Remove(existingUser);
                await _infraContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Erro ao excluir o usuário do banco de dados.", ex);
            }
        }

        public async Task<User> GetUser(Guid guid)
        {
            if (guid == Guid.Empty)
                throw new ArgumentException("O Id do usuário não pode ser vazio.", nameof(guid));

            try
            {
                var user = await _infraContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == guid);
                if (user == null)
                    throw new KeyNotFoundException($"Usuário com Id '{guid}' não encontrado.");

                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao buscar o usuário.", ex);
            }
        }

        public async Task UpdateAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "Usuário não pode ser nulo.");

            try
            {
                var existingUser = await _infraContext.Users.FindAsync(user.Id);
                if (existingUser == null)
                    throw new KeyNotFoundException($"Usuário com Id '{user.Id}' não encontrado.");

                _infraContext.Entry(existingUser).CurrentValues.SetValues(user);
                await _infraContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Erro ao atualizar o usuário no banco de dados.", ex);
            }
        }
    }
}
