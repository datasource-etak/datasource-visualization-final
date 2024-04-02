using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorDatasource.Server.Migrations.EntityObject
{
    /// <inheritdoc />
    public partial class NewSharedDatasetFields3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DatasetExists",
                table: "SharedDatasets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatasetExists",
                table: "SharedDatasets");
        }
    }
}
