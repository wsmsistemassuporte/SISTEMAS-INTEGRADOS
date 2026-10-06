namespace WSM.SISTEMAS.WebApp.MVC.Models.Produtos
{
    /// <summary>
    /// Resultado das operações de escrita na catalogo.api (create/update/delete).
    /// </summary>
    public class CatalogoResult
    {
        public bool Sucesso { get; set; }
        public int StatusCode { get; set; }
        public List<string> Erros { get; set; } = [];
    }
}
