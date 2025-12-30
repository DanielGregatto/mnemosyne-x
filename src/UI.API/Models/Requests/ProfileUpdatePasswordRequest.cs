using System.ComponentModel.DataAnnotations;

namespace UI.API.Models.Requests
{
    public class ProfileUpdatePasswordRequest
    {
        [Required(ErrorMessage = "O campo 'Senha atual' é obrigatório.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "O campo 'Nova senha' é obrigatório.")]
        [StringLength(100, ErrorMessage = "A nova senha deve ter entre {2} e {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "O campo 'Confirmação de senha' é obrigatório.")]
        [Compare("NewPassword", ErrorMessage = "A confirmação de senha não coincide com a nova senha.")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }
    }
}
