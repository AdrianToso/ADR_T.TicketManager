﻿using MediatR;
using ADR_T.TicketManager.Application.DTOs;

namespace ADR_T.TicketManager.Application.Features.Users.Queries.GetUserById
{
    public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;
}