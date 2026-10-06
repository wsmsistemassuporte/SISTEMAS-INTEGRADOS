using System.ComponentModel.DataAnnotations;

namespace WSM.SISTEMAS.WebApp.MVC.Models.Usuarios
{
    /// <summary>
    /// Formulário de criação/edição/reset de senha de usuário na área admin.
    /// A política de senha do Identity é validada pela API (mensagens pt-BR vêm nos erros).
    /// </summary>
    public class UsuarioFormViewModel
    {
        public Guid? Id { get; set; }

        // Controla se os campos de senha inicial (criação) são exibidos na view
        public bool EhCriacao { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(200, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 2)]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "A senha precisa ter entre {2} e {1} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string? Senha { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem")]
        [Display(Name = "Confirmação da Senha")]
        public string? ConfirmarSenha { get; set; }

        [StringLength(100, ErrorMessage = "A nova senha precisa ter entre {2} e {1} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        public string? NovaSenha { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NovaSenha), ErrorMessage = "As senhas não conferem")]
        [Display(Name = "Confirmação da Nova Senha")]
        public string? ConfirmarNovaSenha { get; set; }

        [Display(Name = "Administrador")]
        public bool EhAdmin { get; set; }

        [Display(Name = "Bloqueado")]
        public bool Bloqueado { get; set; }
    }
}
