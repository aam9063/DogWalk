using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DogWalk_Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionCarritoYRelaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "ItemsCarrito",
                newName: "ArticuloId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsCarrito_ItemId",
                table: "ItemsCarrito",
                newName: "IX_ItemsCarrito_ArticuloId");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("8f7779b5-e30e-4e38-bdf4-79c533696187"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 30, 40, 619, DateTimeKind.Utc).AddTicks(4415));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("95b2b3ff-f0c1-4819-a842-0d0b6e111c0d"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 30, 40, 619, DateTimeKind.Utc).AddTicks(4421));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c2fbf3a7-adfa-4ac4-b384-14874661c995"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 30, 40, 619, DateTimeKind.Utc).AddTicks(4419));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("7a5d6a55-43d0-4825-b5c0-7ce22ebd142c"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 30, 40, 619, DateTimeKind.Utc).AddTicks(4638));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("d21b1406-2dce-4cd9-9b0f-e1c366ac6c4c"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 30, 40, 619, DateTimeKind.Utc).AddTicks(4643));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("dbc1c3f6-6230-46c9-a344-7d5d647738be"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 30, 40, 619, DateTimeKind.Utc).AddTicks(4632));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("e0da5de2-b1e3-4c4d-b03d-b35c7f12c5d7"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 30, 40, 619, DateTimeKind.Utc).AddTicks(4639));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ArticuloId",
                table: "ItemsCarrito",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsCarrito_ArticuloId",
                table: "ItemsCarrito",
                newName: "IX_ItemsCarrito_ItemId");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("8f7779b5-e30e-4e38-bdf4-79c533696187"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 15, 1, 200, DateTimeKind.Utc).AddTicks(2239));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("95b2b3ff-f0c1-4819-a842-0d0b6e111c0d"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 15, 1, 200, DateTimeKind.Utc).AddTicks(2245));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c2fbf3a7-adfa-4ac4-b384-14874661c995"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 15, 1, 200, DateTimeKind.Utc).AddTicks(2243));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("7a5d6a55-43d0-4825-b5c0-7ce22ebd142c"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 15, 1, 200, DateTimeKind.Utc).AddTicks(2412));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("d21b1406-2dce-4cd9-9b0f-e1c366ac6c4c"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 15, 1, 200, DateTimeKind.Utc).AddTicks(2416));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("dbc1c3f6-6230-46c9-a344-7d5d647738be"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 15, 1, 200, DateTimeKind.Utc).AddTicks(2406));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("e0da5de2-b1e3-4c4d-b03d-b35c7f12c5d7"),
                column: "CreadoEn",
                value: new DateTime(2025, 5, 9, 15, 15, 1, 200, DateTimeKind.Utc).AddTicks(2415));
        }
    }
}
