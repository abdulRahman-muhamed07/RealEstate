namespace RealEstate.DTOs.Request
{
    public class RegisterDto
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsCompany { get; set; }



    }
}
