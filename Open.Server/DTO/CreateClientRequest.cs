namespace Open.Server.DTO
{
    public class CreateClientRequest
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string DisplayName { get; set; }
        public string RedirectUri { get; set; }
    }
}
