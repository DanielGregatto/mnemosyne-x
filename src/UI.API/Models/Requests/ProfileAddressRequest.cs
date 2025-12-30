using System.ComponentModel.DataAnnotations;

namespace UI.API.Models.Requests
{
    public class ProfileAddressRequest
    {
        [Required(ErrorMessage = "O campo 'CEP' é obrigatório.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "O campo 'CEP' deve conter exatamente 8 números.")]
        public string Cep { get; set; }

        [Required(ErrorMessage = "O campo 'Endereço' é obrigatório.")]
        [StringLength(255, ErrorMessage = "O endereço deve ter no máximo 255 caracteres.")]
        public string Street { get; set; }

        [Required(ErrorMessage = "O campo 'Número' é obrigatório.")]
        public string Number { get; set; }

        [StringLength(255, ErrorMessage = "O complemento deve ter no máximo 255 caracteres.")]
        public string? Complement { get; set; }

        [Required(ErrorMessage = "O campo 'Bairro' é obrigatório.")]
        [StringLength(100, ErrorMessage = "O bairro deve ter no máximo 100 caracteres.")]
        public string Neighborhood { get; set; }

        [Required(ErrorMessage = "O campo 'Cidade' é obrigatório.")]
        [StringLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
        public string City { get; set; }

        [Required(ErrorMessage = "O campo 'Estado' é obrigatório.")]
        [StringLength(2, ErrorMessage = "O estado deve ter 2 caracteres.")]
        public string State { get; set; }
    }
}
