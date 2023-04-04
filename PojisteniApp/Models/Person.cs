namespace PojisteniApp.Models
{
    public class Person
    {
        public int PersonId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName { get { return $"{FirstName} {LastName}"; } }

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Street { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string PostalCode { get; set; } = string.Empty;

        public string Address { get { return $"{Street}, {City}"; } }

        public List<Insurance> InsuredByInsurances { get; set; } = new List<Insurance>();

        public List<Insurance> PolicyholderOfInsurances { get; set; } = new List<Insurance>();
    }
}
