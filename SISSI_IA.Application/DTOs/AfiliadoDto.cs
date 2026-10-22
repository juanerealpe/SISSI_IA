using System;
namespace SISSI_IA.Application.DTOs;
public record RegistrarAfiliadoDto(string TipoDocumento, string NumeroDocumento, string Nombres, string Apellidos, DateTime FechaNacimiento, string Celular, string Correo, string EPS, string AFP, string ARL, string? CajaCompensacion);
public record TrasladoEpsDto(string TipoDocumento, string NumeroDocumento, string NuevaEPS);
public record ResultadoOperacionDto(bool Exito, string Mensaje);
public record InfoAfiliadoDto(string TipoDocumento, string NumeroDocumento, string NombreCompleto, string Estado, string EPS, string AFP, string ARL);