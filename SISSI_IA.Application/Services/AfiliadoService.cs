using System;
using System.Threading.Tasks;
using SISSI_IA.Application.Interfaces;
using SISSI_IA.Application.DTOs;
using SISSI_IA.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace SISSI_IA.Application.Services;

public class AfiliadoService : IAfiliadoService {

    // Cambiamos el DbContext genérico por nuestra interfaz desacoplada
    private readonly ISissiDbContext _context;

    // Modificamos el constructor para recibir la interfaz
    public AfiliadoService(ISissiDbContext context)
    {
        _context = context;
    }

    public async Task<ResultadoOperacionDto> RegistrarAfiliadoAsync(RegistrarAfiliadoDto dto) {
        var existe = await _context.Afiliados.AnyAsync(a => a.TipoDocumento == dto.TipoDocumento && a.NumeroDocumento == dto.NumeroDocumento);
        if (existe) return new ResultadoOperacionDto(false, $"El afiliado con {dto.TipoDocumento} {dto.NumeroDocumento} ya se encuentra registrado.");

        var nuevo = new Afiliado {
            TipoDocumento = dto.TipoDocumento,
            NumeroDocumento = dto.NumeroDocumento,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            FechaNacimiento = dto.FechaNacimiento,
            Celular = dto.Celular,
            Correo = dto.Correo,
            EPS = dto.EPS,
            AFP = dto.AFP,
            ARL = dto.ARL,
            CajaCompensacion = dto.CajaCompensacion
        };

        _context.Afiliados.Add(nuevo);
        await _context.SaveChangesAsync();
        return new ResultadoOperacionDto(true, $"Afiliado {dto.Nombres} {dto.Apellidos} registrado con éxito en el sistema.");
    }

    public async Task<ResultadoOperacionDto> RegistrarTrasladoEpsAsync(TrasladoEpsDto dto) {
        var afiliado = await _context.Afiliados.FirstOrDefaultAsync(a => a.TipoDocumento == dto.TipoDocumento && a.NumeroDocumento == dto.NumeroDocumento);
        if (afiliado == null) return new ResultadoOperacionDto(false, "No se encontró ningún afiliado con la identificación suministrada.");

        if (afiliado.Estado != SISSI_IA.Domain.Enums.EstadoAfiliado.Activo)
            return new ResultadoOperacionDto(false, "El afiliado debe estar Activo para procesar novedades de traslado.");

        string epsAnterior = afiliado.EPS;
        afiliado.EPS = dto.NuevaEPS;
        await _context.SaveChangesAsync();

        return new ResultadoOperacionDto(true, $"Traslado procesado exitosamente. Cambiado de {epsAnterior} a {dto.NuevaEPS}.");
    }

    public async Task<InfoAfiliadoDto?> ConsultarAfiliadoAsync(string tipoDoc, string numDoc) {
        var a = await _context.Afiliados.FirstOrDefaultAsync(x => x.TipoDocumento == tipoDoc && x.NumeroDocumento == numDoc);
        if (a == null) return null;

        return new InfoAfiliadoDto(a.TipoDocumento, a.NumeroDocumento, $"{a.Nombres} {a.Apellidos}", a.Estado.ToString(), a.EPS, a.AFP, a.ARL);
    }
}