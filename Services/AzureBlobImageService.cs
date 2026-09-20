using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace PiscinerosAPI.Services
{
    public class AzureBlobImageService : IImageService
    {
        private readonly string _connectionString;

        public AzureBlobImageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureStorage") 
                ?? throw new InvalidOperationException("Falta ConnectionStrings:AzureStorage");
        }

        public async Task<string> SubirImagenAsync(IFormFile archivo, string carpeta)
        {
            if (archivo == null || archivo.Length == 0)
                throw new ArgumentException("El archivo está vacío o es nulo.");

            // El nombre del contenedor en Azure Storage suele estar en minúsculas y sin espacios
            // "fotos" es un buen nombre de contenedor.
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(carpeta.ToLower());

            // Crea el contenedor si no existe (con acceso público de lectura para las imágenes)
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Genera un nombre único
            var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            var blobClient = blobContainerClient.GetBlobClient(nombreArchivo);

            using (var stream = archivo.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = archivo.ContentType });
            }

            return blobClient.Uri.ToString();
        }
    }
}
