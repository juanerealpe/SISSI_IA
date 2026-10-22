using System;
using System.Linq;
using SISSI_IA.Domain.Entities;
using SISSI_IA.Domain.Enums;

namespace SISSI_IA.Infrastructure.Persistence;

public static class DbInitializer
{
    public static void Seed(ApplicationDbContext context)
    {
        // Asegura que la base de datos SQLite esté creada físicamente
        context.Database.EnsureCreated();

        // Si ya existen afiliados, no duplicamos los datos semilla
        if (context.Afiliados.Any())
        {
            return;
        }

        var afiliadosSemilla = new[]
        {
            new Afiliado
            {
                TipoDocumento = "CC",
                NumeroDocumento = "1018456789",
                Nombres = "Juan Carlos",
                Apellidos = "Pérez Restrepo",
                FechaNacimiento = new DateTime(1988, 5, 14),
                Celular = "3104567890",
                Correo = "juan.perez@gmail.com",
                EPS = "Sura",
                AFP = "Protección",
                ARL = "Sura",
                CajaCompensacion = "Compensar",
                Estado = EstadoAfiliado.Activo,
                FechaAfiliacion = DateTime.UtcNow.AddYears(-3)
            },
            new Afiliado
            {
                TipoDocumento = "CC",
                NumeroDocumento = "52345678",
                Nombres = "María Camila",
                Apellidos = "Gómez Buendía",
                FechaNacimiento = new DateTime(1992, 11, 22),
                Celular = "3157891234",
                Correo = "maria.gomez@outlook.com",
                EPS = "Sanitas",
                AFP = "Porvenir",
                ARL = "Positiva",
                CajaCompensacion = "Cafam",
                Estado = EstadoAfiliado.Activo,
                FechaAfiliacion = DateTime.UtcNow.AddYears(-2)
            },
            new Afiliado
            {
                TipoDocumento = "CE",
                NumeroDocumento = "9876543",
                Nombres = "Andrés Eduardo",
                Apellidos = "Mendoza Mendoza",
                FechaNacimiento = new DateTime(1985, 3, 30),
                Celular = "3201234567",
                Correo = "andres.mendoza@yahoo.com",
                EPS = "Salud Total",
                AFP = "Colpensiones",
                ARL = "Bolívar",
                CajaCompensacion = "Colsubsidio",
                Estado = EstadoAfiliado.Inactivo, // Simulamos un afiliado retirado
                FechaAfiliacion = DateTime.UtcNow.AddYears(-4),
                FechaRetiro = DateTime.UtcNow.AddMonths(-2)
            }
        };

        context.Afiliados.AddRange(afiliadosSemilla);
        context.SaveChanges();
    }
}