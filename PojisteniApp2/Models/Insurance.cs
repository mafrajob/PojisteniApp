using PojisteniApp2.Helpers;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PojisteniApp2.Models
{
    public class Insurance : IAuthor
    {
        [Display(Name = "ID Pojištění")]
        public int InsuranceId { get; set; }

        [Display(Name = "ID Typu pojištění")]
        public int InsuranceTypeId { get; set; }

        [Display(Name = "Typ pojištění")]
        public InsuranceType? InsuranceType { get; set; }

        [Display(Name = "Částka")]
        [Range(1000, 1000000000, ErrorMessage = "{0} musí být mezi {1} a {2}")]
        [DisplayFormat(DataFormatString = "{0:# ### ### ### Kč}")]
        public int InsuranceAmount { get; set; }

        [Display(Name = "Předmět pojištění")]
        [Required(ErrorMessage = "Vyplňte předmět pojištění")]
        [StringLength(150, ErrorMessage = "Maximální délka předmětu pojištění je 150 znaků")]
        public string InsuranceSubject { get; set; } = string.Empty;

        [Display(Name = "Platnost od")]
        [Required(ErrorMessage = "Vyplňte platnost od")]
        [DateRange(nameof(ValidFrom), nameof(ValidTo))]
        [DataType(DataType.Date)]
        public DateTime ValidFrom { get; set; } = DateTime.MinValue;

        [Display(Name = "Platnost do")]
        [Required(ErrorMessage = "Vyplňte platnost do")]
        [DateRange(nameof(ValidFrom), nameof(ValidTo))]
        [DataType(DataType.Date)]
        public DateTime ValidTo { get; set; } = DateTime.MaxValue;

        [Display(Name = "ID Pojištěnce")]
        public int PersonId { get; set; }

        [Display(Name = "Pojištěnec")]
        public Person? Person { get; set; }

        [StringLength(450)]
        public string? AuthorId { get; set; }
    }
}
