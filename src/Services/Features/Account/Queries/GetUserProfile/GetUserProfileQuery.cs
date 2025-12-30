using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using Domain.DTO.Responses;
using MediatR;
using System;

namespace Services.Features.Account.Queries.GetUserProfile
{
    public class GetUserProfileQuery : IRequest<Result<ProfileDto>>
    {
        public Guid UserId { get; set; }
    }
}
