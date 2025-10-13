using Aurum_Application.DTOs;
using Aurum_Domain.Entities;
using Aurum_Domain.Interfaces;
using Aurum_Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.Service
{
    public class TransactionService<T> where T : Transaction
    {
        private readonly ITransactionRepository<T> _repository;

        public TransactionService(ITransactionRepository<T> repository)
        {
            _repository = repository;
        }


    }
}
