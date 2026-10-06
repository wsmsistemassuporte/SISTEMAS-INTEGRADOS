namespace WSM.SISTEMAS.WebApp.MVC.Models.Usuarios
{
    /// <summary>
    /// Resultado das operações de escrita na gestão de usuários (autenticar.api /api/users).
    /// </summary>
    public class UsuariosResult
    {
        public bool Sucesso { get; set; }
        public int StatusCode { get; set; }
        public List<string> Erros { get; set; } = [];
    }
}
