using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorAtividades.Migrations
{
    /// <summary>
    /// DataInicio e DataConclusao passam de timestamp with time zone (UTC) para date, sem hora.
    /// A conversão usa o fuso de Brasília: um ALTER TYPE simples usaria o fuso da sessão (normalmente UTC)
    /// e moveria para o dia seguinte atividades marcadas entre 21h e 23h59 no horário de Brasília.
    /// As horas existentes são descartadas; o Down restaura as datas à meia-noite de Brasília.
    /// </summary>
    public partial class DatasAtividadeSemHora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Atividades"
                    ALTER COLUMN "DataInicio" TYPE date
                        USING ("DataInicio" AT TIME ZONE 'America/Sao_Paulo')::date,
                    ALTER COLUMN "DataConclusao" TYPE date
                        USING ("DataConclusao" AT TIME ZONE 'America/Sao_Paulo')::date;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Atividades"
                    ALTER COLUMN "DataInicio" TYPE timestamp with time zone
                        USING ("DataInicio"::timestamp AT TIME ZONE 'America/Sao_Paulo'),
                    ALTER COLUMN "DataConclusao" TYPE timestamp with time zone
                        USING ("DataConclusao"::timestamp AT TIME ZONE 'America/Sao_Paulo');
                """);
        }
    }
}
