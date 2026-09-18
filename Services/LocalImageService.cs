using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PiscinerosAPI.Services
{
    public class LocalImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalImageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> SubirImagenAsync(IFormFile archivo, string subCarpeta = "fotos")
        {
            if (archivo == null || archivo.Length == 0)
                throw new ArgumentException("No se envió ninguna imagen.");

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), subCarpeta);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(archivo.FileName);
            if (string.IsNullOrEmpty(extension)) extension = ".jpg"; // fallback

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, nombreArchivo);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Generar URL local
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                throw new InvalidOperationException("HttpContext is not available to generate the URL.");
            }

            var baseUrl = $"{request.Scheme}://{request.Host.Value}";
            return $"{baseUrl}/{subCarpeta}/{nombreArchivo}";
        }
    }
}
