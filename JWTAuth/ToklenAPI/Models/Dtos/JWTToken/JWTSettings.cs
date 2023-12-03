namespace ToklenAPI.Models.Dtos.JWTToken
{
    public class JWTSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Subject { get; set; }
        public string Key { get; set; }
        public int Duration { get; set; }
    }
}
