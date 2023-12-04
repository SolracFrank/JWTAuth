using Microsoft.AspNetCore.Identity;

namespace Infrastructure.CustomEntities
{
    public class AdvancedUser : IdentityUser
    {
        private int _age;
        public required DateTime BirthDate { get; set; }
        public int Age
        {
            get
            {
                _age = DateTime.Now.Year - BirthDate.Year;
                if (DateTime.Now.DayOfYear < BirthDate.DayOfYear)
                    _age = _age - 1;

                return _age;
            }
        }
    }
}
