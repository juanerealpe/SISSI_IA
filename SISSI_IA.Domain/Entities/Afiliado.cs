using System;
using SISSI_IA.Domain.Enums;
namespace SISSI_IA.Domain.Entities;
public class Afiliado {
    public int Id { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Celular { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string EPS { get; set; } = string.Empty;
    public string AFP { get; set; } = string.Empty;
    public string ARL { get; set; } = string.Empty;
    public string? CajaCompensacion { get; set; }
    public EstadoAfiliado Estado { get; set; } = EstadoAfiliado.Activo;
    public DateTime FechaAfiliacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaRetiro { get; set; }
}