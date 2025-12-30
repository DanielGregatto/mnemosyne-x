using Identity.Model.Requests;
using System.ComponentModel.DataAnnotations;

public class CustomResetPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Token { get; set; }

    [Required, DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required, DataType(DataType.Password), Compare("NewPassword", ErrorMessage = "Senha e confirmação de senha não estão iguais.")]
    public string ConfirmPassword { get; set; }
}
