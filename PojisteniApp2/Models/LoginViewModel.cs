using System.ComponentModel.DataAnnotations;

namespace PojisteniApp2.Models
{
	public class LoginViewModel
	{
		[Display(Name = "E-mail")]
		[Required(ErrorMessage = "Vyplňte e-mail")]
		[EmailAddress(ErrorMessage = "Neplatná e-mailová adresa")]
		public string Email { get; set; } = string.Empty;

        [Display(Name = "Heslo")]
        [Required(ErrorMessage = "Vyplňte heslo")]
		[DataType(DataType.Password)]
		public string Password { get; set; } = string.Empty;

		[Display(Name = "Pamatuj si mě")]
		public bool RememberMe { get; set; }
	}
}
