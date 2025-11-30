using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestorAtividades.Models
{

    [Index(nameof(Email), IsUnique = true)]
    public class Usuario
    {
        public int Id { get; set; }

        [Display(Name = "Nome completo")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O {0} deve ter entre {2} e {1} caracteres.")]
        [RegularExpression(@"^[A-Za-zÀ-ÖØ-öø-ÿ' \-]+$", ErrorMessage = "O {0} deve conter apenas letras, espaços, hífens ou apóstrofos.")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "O campo {0} deve conter um email válido.")]
        [Display(Name = "Email/Gmail")]
        public string? Email { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "A senha deve ter no mínimo 5 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{5,}$", 
            ErrorMessage = "A senha deve conter pelo menos uma letra, um número e um símbolo especial.")]
        public string? Senha { get; set; }

        public string? SenhaHash { get; set; }
        public DateTime DataRegistro { get; set; } = TimeHelper.NowInBrazil();

         public ICollection<Atividade> Atividades { get; set; } = new List<Atividade>();
    }
}