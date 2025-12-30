using System.ComponentModel.DataAnnotations;

namespace UI.API.Models.Requests
{
    public class ProfilePersonalInfoRequest
    {
        [Required(ErrorMessage = "Preencha o campo de nome.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        [RegularExpression(@"^([A-Za-zÀ-ÖØ-öø-ÿ]{2,} ?)+$", ErrorMessage = "Informe o nome e o sobrenome.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Preencha o campo de telefone.")]
        [Phone(ErrorMessage = "Número de telefone inválido.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Preencha o campo de CPF.")]
        public string CPF_CNPJ { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}