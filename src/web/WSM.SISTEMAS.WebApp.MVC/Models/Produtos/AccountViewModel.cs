namespace WSM.SISTEMAS.WebApp.MVC.Models.Produtos
{
    /// <summary>
    /// Perfil simples do usuário logado, exibido em Account/Index.
    /// </summary>
    public class AccountViewModel
    {
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = [];
    }
}
