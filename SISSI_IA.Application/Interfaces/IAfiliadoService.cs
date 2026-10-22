using System.Threading.Tasks;
using SISSI_IA.Application.DTOs;
namespace SISSI_IA.Application.Interfaces;
public interface IAfiliadoService {
    Task<ResultadoOperacionDto> RegistrarAfiliadoAsync(RegistrarAfiliadoDto dto);
    Task<ResultadoOperacionDto> RegistrarTrasladoEpsAsync(TrasladoEpsDto dto);
    Task<InfoAfiliadoDto?> ConsultarAfiliadoAsync(string tipoDoc, string numDoc);
}