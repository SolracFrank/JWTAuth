namespace Domain.Models.Dtos.IdentityDtos
{
    public class IdentityUserRegisterDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public string Password { get; set; }
    }
}
