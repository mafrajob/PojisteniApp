namespace PojisteniApp.Models
{
    public class Insurance
    {
        public int InsuranceId { get; set; }

        public InsuranceType InsuranceType { get; set; } = new InsuranceType();

        public int InsuranceAmount { get; set; }

        public string InsuranceSubject { get; set; } = string.Empty;

        public DateTime ValidFrom { get; set; } = DateTime.MinValue;

        public DateTime ValidUntil { get; set; } = DateTime.MaxValue;
    }
}
