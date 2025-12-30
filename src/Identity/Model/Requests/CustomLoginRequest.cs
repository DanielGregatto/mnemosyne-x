using System.ComponentModel.DataAnnotations;

namespace Identity.Model.Requests
{
    public class CustomRegisterRequest
    {
        [Required(ErrorMessage = "Preencha o campo de e-mail")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Preencha o campo de senha"), DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords não são iguais.")]
        public string ConfirmPassword { get; set; }
    }
}
