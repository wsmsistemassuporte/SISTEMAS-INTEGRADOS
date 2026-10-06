using Microsoft.AspNetCore.Identity;

namespace WSM.SISTEMAS.Identidade.API.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string Nome { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public DateTime? UltimoLoginEm { get; set; }
    }
}
