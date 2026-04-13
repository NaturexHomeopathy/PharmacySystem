using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacySystem.Migrations
{
    /// <inheritdoc />
    public partial class Completion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("3693569f-7a79-4d82-83f7-8d61d741b970"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Address", "CreatedAt", "Email", "FirstName", "IsEnabled", "IsOnline", "LastName", "MobileNumber", "PasswordHash", "RefreshToken", "RefreshTokenExpiryTime", "Role" },
                values: new object[] { new Guid("4814ee9b-3b05-44ee-94b9-ce124a09fa3b"), null, new DateTime(2026, 4, 9, 9, 8, 50, 696, DateTimeKind.Utc).AddTicks(7598), "Rohit@123", "Rohit", true, false, "Bargode", null, "$2a$11$c1QJNTgkDmSgyP6KPitXOOG.yPYt13ovPIT/tRu/7xY5fvz/0RHn2", null, null, "SuperAdmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("4814ee9b-3b05-44ee-94b9-ce124a09fa3b"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Address", "CreatedAt", "Email", "FirstName", "IsEnabled", "IsOnline", "LastName", "MobileNumber", "PasswordHash", "RefreshToken", "RefreshTokenExpiryTime", "Role" },
                values: new object[] { new Guid("3693569f-7a79-4d82-83f7-8d61d741b970"), null, new DateTime(2026, 4, 9, 9, 4, 16, 73, DateTimeKind.Utc).AddTicks(2026), "Rohit@123", "Rohit", false, false, "Bargode", null, "$2a$11$h2bkmoc7kqIeH1afTT..AuDPUqlyhFGToHG3yB0cPos4meIJZJI/y", null, null, "SuperAdmin" });
        }
    }
}
