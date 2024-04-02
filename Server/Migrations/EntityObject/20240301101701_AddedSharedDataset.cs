using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorDatasource.Server.Migrations.EntityObject
{
    /// <inheritdoc />
    public partial class AddedSharedDataset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedDatasets",
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
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SharedTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsGenerated = table.Column<bool>(type: "bit", nullable: false),
                    NewDatasetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewDatasetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewDatasetAlias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedDatasets", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedDatasets");
        }
    }
}
