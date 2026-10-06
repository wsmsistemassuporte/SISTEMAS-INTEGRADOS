namespace WSM.SISTEMAS.WebApp.MVC.Models.Usuarios
{
    /// <summary>
    /// Usuário exibido na listagem administrativa (espelha o UserResponse da autenticar.api).
    /// </summary>
    public class UsuarioItemViewModel
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public List<string> Roles { get; set; } = [];
        public bool Bloqueado { get; set; }

        public bool EhAdmin => Roles.Contains("Admin");
    }
}
