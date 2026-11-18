namespace GestorAtividades.Models
{
    /// <summary>
    /// Entidade de persistência. DataInicio/DataConclusao são só datas (coluna date, sem hora nem fuso);
    /// DataRegistro é o instante do cadastro em UTC. Descricao é HTML sanitizado.
    /// Formulários usam AtividadeFormViewModel e telas usam AtividadeViewModel.
    /// </summary>
    public class Atividade
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public DateOnly DataInicio { get; set; }
        public DateOnly DataConclusao { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public StatusAtividade Status { get; set; }
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }

        public Usuario User { get; set; } = null!;
    }
}
