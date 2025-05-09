using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DogWalk_Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionDetalleFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemCarrito_Servicio",
                table: "ItemsCarrito");

            migrationBuilder.DropColumn(
                name: "TipoItem",
                table: "ItemsCarrito");

            migrationBuilder.DropColumn(
                name: "MonedaPrecio",
                table: "DetallesFactura");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "DetallesFactura");

            migrationBuilder.DropColumn(
                name: "TipoItem",
                table: "DetallesFactura");

            migrationBuilder.RenameColumn(
                name: "MonedaSubtotal",
                table: "DetallesFactura",
                newName: "Moneda");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "DetallesFactura",
                newName: "ArticuloId");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesFactura_ItemId",
                table: "DetallesFactura",
                newName: "IX_DetallesFactura_ArticuloId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Moneda",
                table: "DetallesFactura",
                newName: "MonedaSubtotal");

            migrationBuilder.RenameColumn(
                name: "ArticuloId",
                table: "DetallesFactura",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesFactura_ArticuloId",
                table: "DetallesFactura",
                newName: "IX_DetallesFactura_ItemId");

            migrationBuilder.AddColumn<string>(
                name: "TipoItem",
                table: "ItemsCarrito",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MonedaPrecio",
                table: "DetallesFactura",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                defaultValue: "EUR");

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "DetallesFactura",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoItem",
                table: "DetallesFactura",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("8f7779b5-e30e-4e38-bdf4-79c533696187"),
                column: "CreadoEn",
                value: new DateTime(2025, 4, 19, 17, 23, 38, 140, DateTimeKind.Utc).AddTicks(2101));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("95b2b3ff-f0c1-4819-a842-0d0b6e111c0d"),
                column: "CreadoEn",
                value: new DateTime(2025, 4, 19, 17, 23, 38, 140, DateTimeKind.Utc).AddTicks(2109));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c2fbf3a7-adfa-4ac4-b384-14874661c995"),
                column: "CreadoEn",
                value: new DateTime(2025, 4, 19, 17, 23, 38, 140, DateTimeKind.Utc).AddTicks(2107));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("7a5d6a55-43d0-4825-b5c0-7ce22ebd142c"),
                column: "CreadoEn",
                value: new DateTime(2025, 4, 19, 17, 23, 38, 140, DateTimeKind.Utc).AddTicks(2450));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("d21b1406-2dce-4cd9-9b0f-e1c366ac6c4c"),
                column: "CreadoEn",
                value: new DateTime(2025, 4, 19, 17, 23, 38, 140, DateTimeKind.Utc).AddTicks(2455));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("dbc1c3f6-6230-46c9-a344-7d5d647738be"),
                column: "CreadoEn",
                value: new DateTime(2025, 4, 19, 17, 23, 38, 140, DateTimeKind.Utc).AddTicks(2445));

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: new Guid("e0da5de2-b1e3-4c4d-b03d-b35c7f12c5d7"),
                column: "CreadoEn",
                value: new DateTime(2025, 4, 19, 17, 23, 38, 140, DateTimeKind.Utc).AddTicks(2453));

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCarrito_Servicio",
                table: "ItemsCarrito",
                column: "ItemId",
                principalTable: "Servicios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
