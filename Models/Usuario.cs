using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GestorAtividades.Models
{
    /// <summary>
    /// Entidade de persistência. A validação dos formulários fica nos ViewModels
    /// (RegistrarUsuarioViewModel, EditarPerfilViewModel); os atributos aqui só definem o schema.
    /// </summary>
    [Index(nameof(Email), IsUnique = true)]
    public class Usuario
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        /// <summary>Gravado normalizado (minúsculas, sem espaços). Ver Autenticacao.NormalizarEmail.</summary>
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        public string? SenhaHash { get; set; }

        /// <summary>UTC.</summary>
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;

        public ICollection<Atividade> Atividades { get; set; } = new List<Atividade>();
    }
}
