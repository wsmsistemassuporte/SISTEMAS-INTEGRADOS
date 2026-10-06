namespace WSM.SISTEMAS.WebApp.MVC.Models
{
    public class ApiErrorResult
    {
        public int StatusCode { get; set; }
        public string MensagemUsuario { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = [];
        public string? TraceId { get; set; }
    }
}
