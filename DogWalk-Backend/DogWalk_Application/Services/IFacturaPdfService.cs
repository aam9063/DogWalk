using DogWalk_Domain.Entities;

namespace DogWalk_Application.Services;

public interface IFacturaPdfService
{
    Task<byte[]> GenerarPdfFactura(Factura factura);
}