using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SISSI_IA.Application.DTOs;
using SISSI_IA.Application.Services;
using SISSI_IA.Domain.Entities;
using SISSI_IA.Domain.Enums;
using SISSI_IA.Infrastructure.Persistence;
using Xunit;

namespace SISSI_IA.Tests;

public class AfiliadoServiceTests
{
    // Método auxiliar para generar un contexto de base de datos en memoria aislado por prueba
    private ApplicationDbContext ObtenerContextoEnMemoria()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Nombre único para evitar contaminación de datos
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task RegistrarAfiliado_DeberiaGuardarCorrectamente_CuandoDatosSonValidos()
    {
        // Arrange
        using var context = ObtenerContextoEnMemoria();
        var servicio = new AfiliadoService(context);
        var dto = new RegistrarAfiliadoDto(
            "CC", "111222333", "Carlos", "Restrepo",
            new DateTime(1995, 1, 1), "3001112233", "carlos@test.com",
            "Sura", "Protección", "Sura", "Compensar"
        );

        // Act
        var resultado = await servicio.RegistrarAfiliadoAsync(dto);

        // Assert
        Assert.True(resultado.Exito);
        var afiliadoEnBd = await context.Afiliados.FirstOrDefaultAsync(a => a.NumeroDocumento == "111222333");
        Assert.NotNull(afiliadoEnBd);
        Assert.Equal(EstadoAfiliado.Activo, afiliadoEnBd.Estado);
    }

    [Fact]
    public async Task RegistrarAfiliado_DeberiaFallar_CuandoElDocumentoYaExiste()
    {
        // Arrange
        using var context = ObtenerContextoEnMemoria();
        // Insertamos un afiliado previo de control
        context.Afiliados.Add(new Afiliado { TipoDocumento = "CC", NumeroDocumento = "12345", Nombres = "Existente", EPS = "Sura" });
        await context.SaveChangesAsync();

        var servicio = new AfiliadoService(context);
        var dtoDuplicado = new RegistrarAfiliadoDto(
            "CC", "12345", "Nuevo", "Intento",
            new DateTime(1990, 5, 5), "311", "test@test.com",
            "Sanitas", "Porvenir", "Positiva", null
        );

        // Act
        var resultado = await servicio.RegistrarAfiliadoAsync(dtoDuplicado);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Contains("ya se encuentra registrado", resultado.Mensaje);
    }

    [Fact]
    public async Task TrasladoEps_DeberiaActualizarEps_CuandoElAfiliadoEstaActivo()
    {
        // Arrange
        using var context = ObtenerContextoEnMemoria();
        var afiliado = new Afiliado { TipoDocumento = "CC", NumeroDocumento = "456", Nombres = "Luis", EPS = "Sanitas", Estado = EstadoAfiliado.Activo };
        context.Afiliados.Add(afiliado);
        await context.SaveChangesAsync();

        var servicio = new AfiliadoService(context);
        var dtoTraslado = new TrasladoEpsDto("CC", "456", "Sura");

        // Act
        var resultado = await servicio.RegistrarTrasladoEpsAsync(dtoTraslado);

        // Assert
        Assert.True(resultado.Exito);
        var afiliadoModificado = await context.Afiliados.FirstAsync(a => a.NumeroDocumento == "456");
        Assert.Equal("Sura", afiliadoModificado.EPS);
    }

    [Fact]
    public async Task TrasladoEps_DeberiaFallar_CuandoElAfiliadoEstaInactivo()
    {
        // Arrange
        using var context = ObtenerContextoEnMemoria();
        // Creamos un afiliado INACTIVO
        var afiliadoInactivo = new Afiliado { TipoDocumento = "CC", NumeroDocumento = "789", Nombres = "Diana", EPS = "Sanitas", Estado = EstadoAfiliado.Inactivo };
        context.Afiliados.Add(afiliadoInactivo);
        await context.SaveChangesAsync();

        var servicio = new AfiliadoService(context);
        var dtoTraslado = new TrasladoEpsDto("CC", "789", "Salud Total");

        // Act
        var resultado = await servicio.RegistrarTrasladoEpsAsync(dtoTraslado);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Contains("no se encuentra activo", resultado.Mensaje);

        // Comprobar que la EPS no cambió
        var afiliadoSinCambio = await context.Afiliados.FirstAsync(a => a.NumeroDocumento == "789");
        Assert.Equal("Sanitas", afiliadoSinCambio.EPS);
    }
}