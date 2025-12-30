using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using Identity.Model.Responses;
using MediatR;

namespace Services.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<Result<RegisterDto>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string ConfirmationBaseUrl { get; set; }
    }
}
