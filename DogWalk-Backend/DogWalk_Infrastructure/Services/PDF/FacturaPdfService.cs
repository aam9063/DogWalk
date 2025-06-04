using DogWalk_Application.Services;
using DogWalk_Domain.Entities;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DogWalk_Infrastructure.Services.PDF;

public class FacturaPdfService : IFacturaPdfService
{
    private readonly ILogger<FacturaPdfService> _logger;

    public FacturaPdfService(ILogger<FacturaPdfService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GenerarPdfFactura(Factura factura)
    {
        _logger.LogInformation($"Iniciando generación de PDF para factura {factura.Id}");
        
        return await Task.Run(() =>
        {
            byte[] pdfBytes;
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new PdfWriter(memoryStream))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        using (var document = new Document(pdf))
                        {
                            try
                            {
                                // Encabezado
                                document.Add(new Paragraph("DogWalk")
                                    .SetFontSize(20)
                                    .SetBold()
                                    .SetTextAlignment(TextAlignment.CENTER));

                                document.Add(new Paragraph($"Factura #{factura.Id}")
                                    .SetFontSize(14));

                                document.Add(new Paragraph($"Fecha: {factura.FechaFactura:dd/MM/yyyy}")
                                    .SetFontSize(12));

                                // Detalles del cliente
                                document.Add(new Paragraph($"Cliente: {factura.Usuario?.Nombre ?? "N/A"}")
                                    .SetFontSize(12));

                                // Tabla de detalles
                                var table = new Table(4)
                                    .SetWidth(UnitValue.CreatePercentValue(100));

                                // Encabezados de tabla
                                table.AddHeaderCell(new Cell().Add(new Paragraph("Artículo")));
                                table.AddHeaderCell(new Cell().Add(new Paragraph("Cantidad")));
                                table.AddHeaderCell(new Cell().Add(new Paragraph("Precio Unitario")));
                                table.AddHeaderCell(new Cell().Add(new Paragraph("Subtotal")));

                                // Detalles
                                foreach (var detalle in factura.Detalles)
                                {
                                    table.AddCell(new Cell().Add(new Paragraph(detalle.Articulo?.Nombre ?? "N/A")));
                                    table.AddCell(new Cell().Add(new Paragraph(detalle.Cantidad.ToString())));
                                    table.AddCell(new Cell().Add(new Paragraph($"{detalle.PrecioUnitario.Cantidad:C}")));
                                    table.AddCell(new Cell().Add(new Paragraph($"{detalle.Subtotal.Cantidad:C}")));
                                }

                                document.Add(table);

                                // Total
                                document.Add(new Paragraph($"Total: {factura.Total.Cantidad:C}")
                                    .SetFontSize(14)
                                    .SetBold()
                                    .SetTextAlignment(TextAlignment.RIGHT));

                                // Asegurarse de que todo se escriba
                                document.Flush();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error al generar el contenido del PDF para factura {factura.Id}");
                                throw;
                            }
                        }
                    } 
                } 

                pdfBytes = memoryStream.ToArray();
            } 

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                throw new Exception("El PDF generado está vacío");
            }

            return pdfBytes;
        });
    }
}