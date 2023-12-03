namespace ToklenAPI.Models.Dtos.JWTToken
{
    public class JWTResult
    {
        public string Email { get; set; }
        public string JWToken { get; set; }
        public DateTime JWTExpires { get; set; }
    }
}
