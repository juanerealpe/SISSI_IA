using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using SISSI_IA.Application.DTOs;
using SISSI_IA.Application.Interfaces;
using SISSI_IA.Application.Services;
using SISSI_IA.Infrastructure.Persistence;
using System;
using System.Net.Http;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. REGISTRO DE SERVICIOS TRADICIONALES
// =========================================================================
builder.Services.AddOpenApi(); // Soporte nativo de OpenAPI/Swagger en .NET 10

// Conexión a base de datos SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Mapeo de interfaz para Clean Architecture
builder.Services.AddScoped<ISissiDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// Registro de servicio tradicional de afiliados
builder.Services.AddScoped<IAfiliadoService, AfiliadoService>();

// =========================================================================
// 2. CONFIGURACIÓN DIRECTA PARA GITHUB MODELS ($0 TIER) - HOST ACTUALIZADO
// =========================================================================
var githubToken = builder.Configuration["AI:OpenAI:ApiKey"] ?? string.Empty;


builder.Services.AddHttpClient("GitHubModels", client =>
{
    // APLICADO: URL Base unificada con el prefijo /inference requerido por GitHub
    client.BaseAddress = new Uri("https://models.github.ai/inference/");
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", githubToken);
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.ParseAdd("SISSI_IA_Backend/1.0");
});

// Construcción de la aplicación
var app = builder.Build();

// =========================================================================
// 3. PIPELINE HTTP & ENTORNO DE PRUEBAS SCALAR
// =========================================================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "SISSI IA - Panel de Pruebas de Backend (Scalar)";
        options.Theme = ScalarTheme.Purple;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

// Semilla de datos automática (Seed Data de Colombia)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    DbInitializer.Seed(context);
}

// =========================================================================
// 4. CAPA DE ENDPOINTS DE NEGOCIO (MINIMAL APIS)
// =========================================================================

// Endpoint: Registrar Afiliado
app.MapPost("/api/afiliados/registrar", async (RegistrarAfiliadoDto dto, IAfiliadoService service) =>
{
    var resultado = await service.RegistrarAfiliadoAsync(dto);
    return resultado.Exito
        ? Results.CreatedAtRoute("ConsultarAfiliado", new { tipoDoc = dto.TipoDocumento, numDoc = dto.NumeroDocumento }, resultado)
        : Results.BadRequest(resultado);
})
.WithName("RegistrarAfiliado")
.WithOpenApi(op => { op.Summary = "Registra un nuevo afiliado al sistema general de seguridad social en Colombia."; return op; });

// Endpoint: Traslado de EPS
app.MapPut("/api/afiliados/traslado-eps", async (TrasladoEpsDto dto, IAfiliadoService service) =>
{
    var resultado = await service.RegistrarTrasladoEpsAsync(dto);
    return resultado.Exito
        ? Results.Ok(resultado)
        : Results.BadRequest(resultado);
})
.WithName("TrasladoEps")
.WithOpenApi(op => { op.Summary = "Realiza la novedad de traslado de EPS para un afiliado activo."; return op; });

// Endpoint: Consultar Afiliado por Documento
app.MapGet("/api/afiliados/consultar/{tipoDoc}/{numDoc}", async (string tipoDoc, string numDoc, IAfiliadoService service) =>
{
    var resultado = await service.ConsultarAfiliadoAsync(tipoDoc, numDoc);
    return resultado != null
        ? Results.Ok(resultado)
        : Results.NotFound(new { Mensaje = $"No se encontró ningún afiliado con {tipoDoc} {numDoc}." });
})
.WithName("ConsultarAfiliado")
.WithOpenApi(op => { op.Summary = "Consulta el estado actual y las entidades (EPS, AFP, ARL) adscritas de un afiliado."; return op; });

// =========================================================================
// 5. ENDPOINT CONVERSACIONAL INTEGRADO (MIGRACIÓN GITHUB MODELS COMPLETADA)
// =========================================================================
app.MapPost("/api/chat/interactuar", async (ChatRequest request, IHttpClientFactory httpClientFactory) =>
{
    string contenidoCrudo = string.Empty;
    try
    {
        var client = httpClientFactory.CreateClient("GitHubModels");

        var payload = new
        {
            model = "gpt-4o-mini", // Capa gratuita de desarrollo
            messages = new[]
            {
                new { role = "system", content = "Eres SISSI_IA, un asistente experto en seguridad social colombiana." },
                new { role = "user", content = request.MensajeUsuario }
            },
            temperature = 0.7
        };

        // RUTA CORRECTA: Al concatenarse con la base, golpeará /inference/chat/completions perfectamente
        var respuesta = await client.PostAsJsonAsync("chat/completions", payload);
        contenidoCrudo = await respuesta.Content.ReadAsStringAsync();

        if (!respuesta.IsSuccessStatusCode)
        {
            return Results.Json(new
            {
                Error = $"GitHub Models rechazó la petición con código {respuesta.StatusCode}",
                RespuestaServidor = contenidoCrudo
            }, statusCode: (int)respuesta.StatusCode);
        }

        // Si la respuesta es el texto ordinario "OK", lo capturamos antes del serializador
        if (contenidoCrudo.Trim() == "OK")
        {
            return Results.Json(new { Error = "El servidor de GitHub respondió un mensaje de control 'OK' plano. Revisa el mapeo de URL." }, statusCode: 401);
        }

        using var jsonDocument = System.Text.Json.JsonDocument.Parse(contenidoCrudo);
        var raiz = jsonDocument.RootElement;

        var textoIA = raiz.GetProperty("choices")[0]
                          .GetProperty("message")
                          .GetProperty("content")
                          .GetString();

        return Results.Ok(new { RespuestaSissi = textoIA });
    }
    catch (System.Text.Json.JsonException jsonEx)
    {
        return Results.Json(new
        {
            Error = "El servidor devolvió un formato no esperado por el serializador.",
            TextoCrudoRecibido = contenidoCrudo,
            Detalle = jsonEx.Message
        }, statusCode: 502);
    }
    catch (Exception ex)
    {
        return Results.Json(new { Error = $"Fallo interno en el servidor: {ex.Message}" }, statusCode: 500);
    }
})
.WithName("InteractuarConSissi")
.WithOpenApi(op => { op.Summary = "Endpoint conversacional puro y enrutado bajo el estándar models.github.ai."; return op; });


app.Run();

// =========================================================================
// DTO LOCAL DE TRASPASO PARA EVITAR BUGS DE SERIALIZACIÓN EN MINIMAL APIS
// =========================================================================
public record ChatRequest(string MensajeUsuario);
