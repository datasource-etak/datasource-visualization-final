using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorDatasource.Server.Migrations.EntityObject
{
    /// <inheritdoc />
    public partial class AddedFavoriteDataset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavoriteDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatasetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DatasetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatasetAlias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XAxis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YAxis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChartType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteDatasets", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteDatasets");
        }
    }
}
