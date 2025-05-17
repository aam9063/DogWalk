using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DogWalk_Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JwtId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("8f7779b5-e30e-4e38-bdf4-79c533696187"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 17, 7, 36, 27, 866, DateTimeKind.Utc).AddTicks(5544));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("95b2b3ff-f0c1-4819-a842-0d0b6e111c0d"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 17, 7, 36, 27, 866, DateTimeKind.Utc).AddTicks(5550));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c2fbf3a7-adfa-4ac4-b384-14874661c995"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 17, 7, 36, 27, 866, DateTimeKind.Utc).AddTicks(5548));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("7a5d6a55-43d0-4825-b5c0-7ce22ebd142c"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 17, 7, 36, 27, 866, DateTimeKind.Utc).AddTicks(5707));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("d21b1406-2dce-4cd9-9b0f-e1c366ac6c4c"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 17, 7, 36, 27, 866, DateTimeKind.Utc).AddTicks(5712));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("dbc1c3f6-6230-46c9-a344-7d5d647738be"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 17, 7, 36, 27, 866, DateTimeKind.Utc).AddTicks(5702));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("e0da5de2-b1e3-4c4d-b03d-b35c7f12c5d7"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 17, 7, 36, 27, 866, DateTimeKind.Utc).AddTicks(5710));

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("8f7779b5-e30e-4e38-bdf4-79c533696187"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 38, 9, 326, DateTimeKind.Utc).AddTicks(970));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("95b2b3ff-f0c1-4819-a842-0d0b6e111c0d"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 38, 9, 326, DateTimeKind.Utc).AddTicks(975));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c2fbf3a7-adfa-4ac4-b384-14874661c995"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 38, 9, 326, DateTimeKind.Utc).AddTicks(974));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("7a5d6a55-43d0-4825-b5c0-7ce22ebd142c"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 38, 9, 326, DateTimeKind.Utc).AddTicks(1117));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("d21b1406-2dce-4cd9-9b0f-e1c366ac6c4c"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 38, 9, 326, DateTimeKind.Utc).AddTicks(1119));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("dbc1c3f6-6230-46c9-a344-7d5d647738be"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 38, 9, 326, DateTimeKind.Utc).AddTicks(1113));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("e0da5de2-b1e3-4c4d-b03d-b35c7f12c5d7"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 38, 9, 326, DateTimeKind.Utc).AddTicks(1118));
        }
    }
}
