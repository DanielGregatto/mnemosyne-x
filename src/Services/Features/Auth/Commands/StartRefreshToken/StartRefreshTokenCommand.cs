using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using Identity.Model.Responses;
using MediatR;
using System;

namespace Services.Features.Auth.Commands.StartRefreshToken
{
    public class StartRefreshTokenCommand : IRequest<Result<LoginDto>>
    {
        public Guid UserId { get; set; }
    }
}
