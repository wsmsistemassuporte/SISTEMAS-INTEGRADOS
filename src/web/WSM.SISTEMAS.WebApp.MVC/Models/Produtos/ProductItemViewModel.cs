namespace WSM.SISTEMAS.WebApp.MVC.Models.Produtos
{
    /// <summary>
    /// Espelho do JSON devolvido pela catalogo.api (GET /catalogo, /catalogo/{id}).
    /// Inclui Quantidade (legado) para round-trip completo no formulário de edição.
    /// </summary>
    public class ProductItemViewModel
    {
        private static readonly System.Globalization.CultureInfo PtBr =
            System.Globalization.CultureInfo.GetCultureInfo("pt-BR");

        /// <summary>Preço formatado em pt-BR, ex.: R$ 2.499,00.</summary>
        public string PrecoFormatado => (Preco ?? 0m).ToString("C2", PtBr);

        /// <summary>Valor da parcela em 12x sem juros, formatado em pt-BR.</summary>
        public string ParcelaFormatada => ((Preco ?? 0m) / 12m).ToString("C2", PtBr);
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public string? Imagem { get; set; }
        public string? TipoProduto { get; set; }
        public int? Quantidade { get; set; }
        public int? QuantidadeEstoque { get; set; }
        public bool Ativo { get; set; } = true;
        public decimal? Preco { get; set; }
        public decimal Valor
        {
            get => Preco ?? 0m;
            set => Preco = value;
        }
        public int Estoque
        {
            get => QuantidadeEstoque ?? Quantidade ?? 0;
            set => QuantidadeEstoque = value;
        }
    }
}
