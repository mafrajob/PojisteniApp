using System.ComponentModel.DataAnnotations;

namespace PojisteniApp.Models
{
    public class Insurance
    {
        public int InsuranceId { get; set; }
        
        [Display(Name = "Pojištění")]
        public InsuranceType InsuranceType { get; set; } = new InsuranceType();

        [Display(Name = "Částka")]
        public int InsuranceAmount { get; set; }

        [Display(Name = "Předmět pojištění")]
        [Required(ErrorMessage = "Vyplňte předmět pojištění")]
        [StringLength(150, ErrorMessage = "Maximální délka předmětu pojištění je 150 znaků")]
        public string InsuranceSubject { get; set; } = string.Empty;

        [Display(Name = "Platnost od")]
        [Required(ErrorMessage = "Vyplňte platnost od")]
        public DateTime ValidFrom { get; set; } = DateTime.MinValue;

        [Display(Name = "Platnost do")]
        [Required(ErrorMessage = "Vyplňte platnost do")]
        public DateTime ValidTo { get; set; } = DateTime.MaxValue;
    }
}
