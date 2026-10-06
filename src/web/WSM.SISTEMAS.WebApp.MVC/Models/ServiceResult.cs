namespace WSM.SISTEMAS.WebApp.MVC.Models
{
    public class ServiceResult
    {
        public bool Succeeded { get; set; }
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public string MensagemUsuario { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = [];
        public string? TraceId { get; set; }
    }
}
