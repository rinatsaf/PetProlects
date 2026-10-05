using IdentityService.Domain;
using MediatR;

namespace IdentityService.Application.Commands;

public sealed record RegisterCommand(string Email, string Password, string Role = "Customer") : IRequest<AuthResponseDto>;