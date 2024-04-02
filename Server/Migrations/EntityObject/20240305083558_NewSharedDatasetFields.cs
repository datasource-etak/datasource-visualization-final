using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorDatasource.Server.Migrations.EntityObject
{
    /// <inheritdoc />
    public partial class NewSharedDatasetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedFilters",
                table: "SharedDatasets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                table: "SharedDatasets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedFilters",
                table: "SharedDatasets");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "SharedDatasets");
        }
    }
}
