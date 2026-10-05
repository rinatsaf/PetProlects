using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Tickets;
using Application.Exceptions;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests;

public class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _ticketRepo = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly TicketService _sut;

    public TicketServiceTests()
    {
        _sut = new TicketService(_ticketRepo.Object, _mapper.Object, _currentUser.Object);
    }

    private static readonly CurrentUserInfo AdminUser = new() { IsAuthenticated = true, UserId = 1, Role = UserRole.Admin };
    private static CurrentUserInfo CustomerUser => new() { IsAuthenticated = true, UserId = 3, Role = UserRole.Customer };

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CustomerUser);
        _ticketRepo.Setup(x => x.GetByIdAsync(99, CustomerUser, It.IsAny<CancellationToken>())).ReturnsAsync((Ticket?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99));
    }
    
    [Fact]
    public async Task GetByOrderAsync_ReturnsTickets()
    {
        var tickets = new List<Ticket> { new() { Id = 1, TicketCode = "T1" } };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CustomerUser);
        _ticketRepo.Setup(x => x.GetByOrderAsync(1, It.IsAny<CurrentUserInfo>(), It.IsAny<CancellationToken>())).ReturnsAsync(tickets);
        _mapper.Setup(x => x.Map<IReadOnlyList<TicketDto>>(tickets)).Returns(new List<TicketDto> { new() { Id = 1 } });

        var result = await _sut.GetByOrderAsync(1);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAvailableBySessionAsync_ReturnsAvailable()
    {
        _ticketRepo.Setup(x => x.GetAvailableBySessionAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ticket>());
        _mapper.Setup(x => x.Map<IReadOnlyList<TicketDto>>(It.IsAny<List<Ticket>>()))
            .Returns(new List<TicketDto>());

        var result = await _sut.GetAvailableBySessionAsync(1);
        Assert.Empty(result);
    }

    [Fact]
    public async Task MarkUsedAsync_WhenCustomer_ThrowsForbidden()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CustomerUser);

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.MarkUsedAsync(1));
    }

    [Fact]
    public async Task MarkUsedAsync_WhenAdminAndActive_MarksAsUsed()
    {
        var ticket = new Ticket { Id = 1, TicketCode = "T1", Status = TicketStatus.Active };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _ticketRepo.Setup(x => x.GetByIdAsync(1, AdminUser, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _ticketRepo.Setup(x => x.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _mapper.Setup(x => x.Map<TicketDto>(ticket)).Returns(new TicketDto { Id = 1, Status = "Used" });

        var result = await _sut.MarkUsedAsync(1);
        Assert.Equal("Used", result.Status);
    }

    [Fact]
    public async Task MarkUsedAsync_WhenAlreadyUsed_ThrowsConflict()
    {
        var ticket = new Ticket { Id = 1, TicketCode = "T1", Status = TicketStatus.Used };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _ticketRepo.Setup(x => x.GetByIdAsync(1, AdminUser, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.MarkUsedAsync(1));
    }

    [Fact]
    public async Task MarkUsedByCodeAsync_WithEmptyCode_ThrowsValidation()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        await Assert.ThrowsAsync<ValidationException>(() => _sut.MarkUsedByCodeAsync("   "));
    }
    
    [Fact]
    public async Task MarkUsedByCodeAsync_WhenNotFound_ThrowsNotFound()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _ticketRepo.Setup(x => x.GetByCodeAsync("UNKNOWN", AdminUser, It.IsAny<CancellationToken>())).ReturnsAsync((Ticket?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.MarkUsedByCodeAsync("UNKNOWN"));
    }
    [Fact]
    public async Task MarkUsedByCodeAsync_WhenCustomer_ThrowsForbidden()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CustomerUser);

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.MarkUsedByCodeAsync("CODE123"));
    }
}
