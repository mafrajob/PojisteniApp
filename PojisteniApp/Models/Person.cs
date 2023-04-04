namespace PojisteniApp.Models
{
    public class Person
    {
        public int PersonId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string PostalCode { get; set; } = string.Empty;

        public List<Insurance> InsuredByInsurances { get; set; } = new List<Insurance>();

        public List<Insurance> PolicyholderOfInsurances { get; set; } = new List<Insurance>();
    }
}
