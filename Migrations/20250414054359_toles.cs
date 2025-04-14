using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace San_Agustin_Final.Migrations
{
    /// <inheritdoc />
    public partial class toles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Reservas_IdReserva",
                table: "Contratos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contratos",
                table: "Contratos");

            migrationBuilder.RenameTable(
                name: "Contratos",
                newName: "Contrato");

            migrationBuilder.RenameIndex(
                name: "IX_Contratos_IdReserva",
                table: "Contrato",
                newName: "IX_Contrato_IdReserva");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contrato",
                table: "Contrato",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contrato_Reservas_IdReserva",
                table: "Contrato",
                column: "IdReserva",
                principalTable: "Reservas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contrato_Reservas_IdReserva",
                table: "Contrato");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contrato",
                table: "Contrato");

            migrationBuilder.RenameTable(
                name: "Contrato",
                newName: "Contratos");

            migrationBuilder.RenameIndex(
                name: "IX_Contrato_IdReserva",
                table: "Contratos",
                newName: "IX_Contratos_IdReserva");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contratos",
                table: "Contratos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Reservas_IdReserva",
                table: "Contratos",
                column: "IdReserva",
                principalTable: "Reservas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
