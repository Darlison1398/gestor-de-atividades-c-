using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using GestorAtividades.Validators;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GestorAtividades.Models
{

    public class Atividade
    {
        public int Id { get; set;}
        public string Titulo { get; set;}

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DataInicio { get; set;}

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DataConclusao { get; set;}
        public string Descricao { get; set;}
        public StatusAtividade Status { get; set;}
        public DateTime DataRegistro { get; set; } = TimeHelper.NowInBrazil();
        public int UserId { get; set; } 

        [ValidateNever]
        public Usuario User { get; set; }
    }
    
}