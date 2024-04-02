using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorDatasource.Server.Migrations.EntityObject
{
    /// <inheritdoc />
    public partial class AddedSearchQueryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "RowVersion",
            //    table: "LocalizedProperty");

            //migrationBuilder.DropColumn(
            //    name: "RowVersion",
            //    table: "LocaleStringResource");

            //migrationBuilder.DropColumn(
            //    name: "RowVersion",
            //    table: "Language");

            migrationBuilder.CreateTable(
                name: "SearchQueries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Keyword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchQueries", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchQueries");

            //migrationBuilder.AddColumn<byte[]>(
            //    name: "RowVersion",
            //    table: "LocalizedProperty",
            //    type: "rowversion",
            //    rowVersion: true,
            //    nullable: true);

            //migrationBuilder.AddColumn<byte[]>(
            //    name: "RowVersion",
            //    table: "LocaleStringResource",
            //    type: "rowversion",
            //    rowVersion: true,
            //    nullable: true);

            //migrationBuilder.AddColumn<byte[]>(
            //    name: "RowVersion",
            //    table: "Language",
            //    type: "rowversion",
            //    rowVersion: true,
            //    nullable: true);
        }
    }
}
