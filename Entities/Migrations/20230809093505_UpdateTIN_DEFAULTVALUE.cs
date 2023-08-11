using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTIN_DEFAULTVALUE : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) {
            string sp_InsertPerson = @"UPDATE [dbo].[Persons] SET [Persons].TIN='AAA123'";
            migrationBuilder.Sql(sp_InsertPerson);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            string sp_InsertPerson = @"UPDATE [dbo].[Persons] SET [Persons].TIN = ''";
            migrationBuilder.Sql(sp_InsertPerson);
        }
    }
}
