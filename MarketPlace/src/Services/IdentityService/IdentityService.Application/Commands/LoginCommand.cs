using MediatR;

namespace IdentityService.Application.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;