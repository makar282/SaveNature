namespace SaveNature.Models
{
    public class QrCodeRequest
    {
        public string? QrRaw { get; set; }
        public string? QrUrl { get; set; }
        public required string UserName { get; set; }
    }
}
