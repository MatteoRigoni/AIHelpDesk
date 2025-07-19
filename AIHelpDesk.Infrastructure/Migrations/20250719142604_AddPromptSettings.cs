using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIHelpDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ChatLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PromptSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemPrompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Temperature = table.Column<double>(type: "float", nullable: false),
                    MaxTokens = table.Column<int>(type: "int", nullable: false),
                    TopP = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptSettings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ChatLogs");
        }
    }
}
