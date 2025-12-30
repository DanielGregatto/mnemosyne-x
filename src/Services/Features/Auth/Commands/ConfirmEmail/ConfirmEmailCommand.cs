using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using MediatR;
using System;

namespace Services.Features.Auth.Commands.ConfirmEmail
{
    public class ConfirmEmailCommand : IRequest<Result<UriBuilder>>
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
