using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Applications.DTOs
{
    public record class CreditCartDTO
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O limite de crédito é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O limite de crédito deve ser um valor positivo.")]
        public decimal CreditLimit { get; set; }

        [Required(ErrorMessage = "O dia de vencimento é obrigatório.")]
        [Range(1, 31, ErrorMessage = "O dia de vencimento (DueDay) deve estar entre 1 e 31.")]
        public int DueDay { get; set; }

        [Required(ErrorMessage = "O dia de fechamento é obrigatório.")]
        [Range(1, 31, ErrorMessage = "O dia de fechamento (ClosingDay) deve estar entre 1 e 31.")]
        public int ClosingDay { get; set; }
    }
}
