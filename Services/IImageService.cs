using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PiscinerosAPI.Services
{
    public interface IImageService
    {
        Task<string> SubirImagenAsync(IFormFile archivo, string subCarpeta = "fotos");
    }
}
