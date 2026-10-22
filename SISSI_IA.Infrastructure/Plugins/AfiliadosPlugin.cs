using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SISSI_IA.Application.DTOs;
using SISSI_IA.Application.Interfaces;

namespace SISSI_IA.Infrastructure.Plugins;

public class AfiliadosPlugin
{
    private readonly IAfiliadoService _afiliadoService;

    public AfiliadosPlugin(IAfiliadoService afiliadoService)
    {
        _afiliadoService = afiliadoService;
    }

    [KernelFunction, Description("Realiza el traslado o cambio de EPS de un afiliado activo en el sistema de seguridad social de Colombia.")]
    public async Task<string> TrasladarEps(
        [Description("Tipo de documento de identidad del afiliado (ej: CC, CE, PEP).")] string tipoDocumento,
        [Description("Número de documento o cédula del afiliado.")] string numeroDocumento,
        [Description("Nombre de la nueva EPS a la que se desea trasladar (ej: Sura, Sanitas, Salud Total).")] string nuevaEps)
    {
        var dto = new TrasladoEpsDto(tipoDocumento, numeroDocumento, nuevaEps);
        var resultado = await _afiliadoService.RegistrarTrasladoEpsAsync(dto);

        return resultado.Exito
            ? $"Operación Exitosa: {resultado.Mensaje}"
            : $"Error de Negocio: {resultado.Mensaje}";
    }

    [KernelFunction, Description("Consulta el estado actual, afiliaciones a EPS, AFP, ARL y datos generales de un afiliado por su documento.")]
    public async Task<string> ConsultarAfiliado(
        [Description("Tipo de documento de identidad (ej: CC, CE, PEP).")] string tipoDocumento,
        [Description("Número de documento o cédula del afiliado a consultar.")] string numeroDocumento)
    {
        var info = await _afiliadoService.ConsultarAfiliadoAsync(tipoDocumento, numeroDocumento);
        if (info == null)
        {
            return $"No se encontró ningún afiliado registrado con el documento {tipoDocumento} {numeroDocumento}.";
        }

        return $"Afiliado: {info.NumeroDocumento} {info.NombreCompleto} | Estado: {info.Estado} | EPS: {info.EPS} | AFP: {info.AFP} | ARL: {info.ARL}";
    }
}