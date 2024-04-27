using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StudentRestAPI.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    DepartmenId = table.Column<int>(type: "int", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentId);
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "DateOfBirth", "DepartmenId", "Email", "FirstName", "Gender", "LastName", "PhotoPath" },
                values: new object[,]
                {
                    { 1, new DateTime(2001, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "dangduc01@gmail.com", "Dang01", 0, "Duc01", "Images/01.png" },
                    { 2, new DateTime(2001, 2, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "dangduc02@gmail.com", "Dang02", 0, "Duc02", "Images/02.png" },
                    { 3, new DateTime(2001, 3, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "dangduc03@gmail.com", "Dang03", 1, "Duc03", "Images/03.png" },
                    { 4, new DateTime(2001, 4, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "dangduc02@gmail.com", "Dang04", 1, "Duc04", "Images/04.png" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
