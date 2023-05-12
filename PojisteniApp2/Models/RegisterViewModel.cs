using System.ComponentModel.DataAnnotations;

namespace PojisteniApp2.Models
{
    public class RegisterViewModel
    {
        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Vyplňte e-mail")]
        [EmailAddress(ErrorMessage = "Neplatná e-mailová adresa")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Heslo")]
        [Required(ErrorMessage = "Vyplňte heslo")]
        [StringLength(100, ErrorMessage = "{0} musí mít délku od {2} do {1} znaků", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Potvrzení hesla")]
        [Compare("Password", ErrorMessage = "Hesla se musí shodovat")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
