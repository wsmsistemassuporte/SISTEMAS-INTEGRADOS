namespace WSM.SISTEMAS.WebApp.MVC.Models
{
    public class ErrorViewModel
    {
        public int ErrorCode { get; set; }
        public string? Titulo { get; set; }
        public string? Mensagem { get; set; }
        public List<string> Erros { get; set; } = new();
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public bool HasErros => Erros.Any();
    }
}
