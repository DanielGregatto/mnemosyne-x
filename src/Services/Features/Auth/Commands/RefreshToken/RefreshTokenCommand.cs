using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using Identity.Model.Responses;
using MediatR;

namespace Services.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<Result<LoginDto>>
    {
        public string UserId { get; set; }
        public string RefreshToken { get; set; }
    }
}
