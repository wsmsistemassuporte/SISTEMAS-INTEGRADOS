using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace WSM.SISTEMAS.WebApp.MVC.Extensions
{
    public class CpfAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var cpf = value?.ToString();
            return CpfValido(cpf) ? ValidationResult.Success : new ValidationResult("CPF em formato inválido");
        }

        private static bool CpfValido(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
            {
                return false;
            }

            var numeros = new string(cpf.Where(char.IsDigit).ToArray());
            if (numeros.Length != 11 || numeros.Distinct().Count() == 1)
            {
                return false;
            }

            var primeiroDigito = CalcularDigito(numeros, 9, 10);
            var segundoDigito = CalcularDigito(numeros, 10, 11);

            return numeros[9] == primeiroDigito && numeros[10] == segundoDigito;
        }

        private static char CalcularDigito(string numeros, int quantidade, int pesoInicial)
        {
            var soma = 0;
            for (var i = 0; i < quantidade; i++)
            {
                soma += (numeros[i] - '0') * (pesoInicial - i);
            }

            var resto = soma % 11;
            var digito = resto < 2 ? 0 : 11 - resto;
            return (char)('0' + digito);
        }
    }

    public class CpfAttributeAdapter : AttributeAdapterBase<CpfAttribute>
    {
        public CpfAttributeAdapter(CpfAttribute attribute, IStringLocalizer? stringLocalizer) : base(attribute, stringLocalizer)
        {
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-cpf", GetErrorMessage(context));
        }

        public override string GetErrorMessage(ModelValidationContextBase validationContext)
        {
            return "CPF em formato inválido";
        }
    }

    public class CpfValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
    {
        private readonly IValidationAttributeAdapterProvider _baseProvider = new ValidationAttributeAdapterProvider();

        public IAttributeAdapter? GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer)
        {
            if (attribute is CpfAttribute cpfAttribute)
            {
                return new CpfAttributeAdapter(cpfAttribute, stringLocalizer);
            }

            return _baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
        }
    }
}
