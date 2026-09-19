using PiscinerosAPI.Models;

namespace PiscinerosAPI.Services
{
    public interface IEmailService
    {
        Task EnviarReciboAsync(Visita visita);
    }
}
