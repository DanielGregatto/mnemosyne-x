using Identity.Model.Requests;
using System.ComponentModel.DataAnnotations;

public class CustomForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; }
}
