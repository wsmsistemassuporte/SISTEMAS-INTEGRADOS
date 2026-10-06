using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WSM.SISTEMAS.WebApp.MVC.Models.Produtos
{
    public class ProductFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(200, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 2)]
        [Display(Name = "Nome do Produto")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(1000, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 2)]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Ativo?")]
        public bool Ativo { get; set; } = true;

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [Range(0.01, 999999.99, ErrorMessage = "O valor deve ser entre R$ 0,01 e R$ 999.999,99")]
        [Display(Name = "Preço (R$)")]
        public decimal Valor { get; set; }

        [Display(Name = "Preço (R$)")]
        public decimal Preco
        {
            get => Valor;
            set => Valor = value;
        }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [Range(0, 100000, ErrorMessage = "O quantidade em estoque deve ser entre 0 e 100.000")]
        [Display(Name = "Quantidade em Estoque")]
        public int Estoque { get; set; }

        [Display(Name = "Quantidade em Estoque")]
        public int QuantidadeEstoque
        {
            get => Estoque;
            set => Estoque = value;
        }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [Display(Name = "Categoria")]
        public string TipoProduto { get; set; } = string.Empty;

        [Display(Name = "Caminho da Imagem Atual")]
        public string? Imagem { get; set; }

        [Display(Name = "Upload de Nova Imagem")]
        public IFormFile? ImagemUpload { get; set; }

        public ProductItemViewModel ToProduto()
        {
            return new ProductItemViewModel
            {
                Id = Id ?? Guid.Empty,
                Nome = Nome,
                Descricao = Descricao,
                Imagem = Imagem,
                TipoProduto = TipoProduto,
                Preco = Preco,
                Valor = Valor,
                QuantidadeEstoque = Estoque,
                Estoque = Estoque,
                Ativo = Ativo
            };
        }

        public static ProductFormViewModel FromProduto(ProductItemViewModel p)
        {
            return new ProductFormViewModel
            {
                Id = p.Id,
                Nome = p.Nome ?? string.Empty,
                Descricao = p.Descricao ?? string.Empty,
                Imagem = p.Imagem,
                TipoProduto = p.TipoProduto ?? string.Empty,
                Preco = p.Preco ?? p.Valor,
                Valor = p.Preco ?? p.Valor,
                Estoque = p.Estoque,
                QuantidadeEstoque = p.Estoque,
                Ativo = p.Ativo
            };
        }
    }
}
