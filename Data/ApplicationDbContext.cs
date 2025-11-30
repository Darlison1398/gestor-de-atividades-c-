using Microsoft.EntityFrameworkCore;
using GestorAtividades.Models;
using System.Diagnostics;

namespace GestorAtividades.Data
{
    public class ApplicationDbContext : DbContext 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Atividade> Atividades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Atividade>()
                .Property(a => a.Status)
                .HasConversion<string>();
        }


    }
}