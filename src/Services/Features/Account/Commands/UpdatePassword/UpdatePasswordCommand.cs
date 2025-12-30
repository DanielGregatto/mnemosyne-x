using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using MediatR;
using System;

namespace Services.Features.Account.Commands.UpdatePassword
{
    public class UpdatePasswordCommand : IRequest<Result<string>>
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
