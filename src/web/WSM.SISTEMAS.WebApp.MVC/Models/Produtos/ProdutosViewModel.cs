namespace WSM.SISTEMAS.WebApp.MVC.Models.Produtos
{
    public class ProdutosViewModel
    {
        public IEnumerable<ProductItemViewModel> Produtos { get; set; } = [];
        public IEnumerable<string> Categorias { get; set; } = [];
        public string CategoriaSelecionada { get; set; } = "Todos";
    }
}
