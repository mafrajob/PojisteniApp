using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace PojisteniApp2.Models
{
    public class Person
    {
        public int PersonId { get; set; }

        [Display(Name = "Jméno")]
        [Required(ErrorMessage = "Vyplňte jméno")]
        [StringLength(50, ErrorMessage = "Maximální délka jména je 50 znaků")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Příjmení")]
        [Required(ErrorMessage = "Vyplňte příjmení")]
        [StringLength(50, ErrorMessage = "Maximální délka příjmení je 50 znaků")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Celé jméno")]
        public string FullName { get { return $"{LastName} {FirstName}"; } }

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Vyplňte e-mail")]
        [StringLength(150, ErrorMessage = "Maximální délka e-mailu je 150 znaků")]
        [EmailAddress(ErrorMessage = "Neplatná e-mailová adresa")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telefon")]
        [Required(ErrorMessage = "Vyplňte telefon")]
        [StringLength(50, ErrorMessage = "Maximální délka telefonu je 50 znaků")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Ulice a číslo popisné")]
        [Required(ErrorMessage = "Vyplňte ulici")]
        [StringLength(150, ErrorMessage = "Maximální délka ulice je 150 znaků")]
        public string Street { get; set; } = string.Empty;

        [Display(Name = "Město")]
        [Required(ErrorMessage = "Vyplňte město")]
        [StringLength(100, ErrorMessage = "Maximální délka města je 100 znaků")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "PSČ")]
        [Required(ErrorMessage = "Vyplňte PSČ")]
        [StringLength(50, ErrorMessage = "Maximální délka PSČ je 50 znaků")]
        public string PostalCode { get; set; } = string.Empty;

        [Display(Name = "Bydliště")]
        public string Address { get { return $"{Street}, {City}"; } }

        [Display(Name = "Celé jméno a bydliště")]
        public string FullNameWithAddress { get { return $"{FullName} ({Address})"; } }

        public List<Insurance> Insurances { get; set; } = new List<Insurance>();

        [Display(Name = "Profilový obrázek")]
        public byte[] ImageData { get; set; } = Array.Empty<byte>();

		[StringLength(450)]
		public string? AuthorId { get; set; }
    }
}
