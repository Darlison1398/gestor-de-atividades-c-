using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestorAtividades.Models
{

    public class Atividade
    {
        public int Id { get; set;}

        public string Titulo { get; set;}

        public DateTime DataInicio { get; set;}

        public DateTime DataConclusao { get; set;}

        public string Descricao { get; set;}

        public StatusAtividade Status { get; set;}

        public DateTime DataRegistro { get; set; } = TimeHelper.NowInBrazil();

        [Required]
        public int UserId { get; set; } 

        public Usuario User { get; set; }


    }
    
}