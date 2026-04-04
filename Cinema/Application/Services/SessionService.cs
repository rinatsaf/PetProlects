using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Sessions;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class SessionService(ISessionRepository sessionRepository, IMovieRepository movieRepository, IHallRepository hallRepository, IMapper mapper) : ISessionService
{
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IMovieRepository _movieRepository = movieRepository;
    private readonly IHallRepository _hallRepository = hallRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IReadOnlyList<SessionDto>> GetUpcomingAsync(CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetAllUpcomingAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<SessionDto>>(sessions);
    }

    public async Task<SessionDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException($"Session with id {id} not found");
        return _mapper.Map<SessionDto>(session);
    }

    public async Task<SessionDto> CreateAsync(CreateSessionRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureMovieAndHallExist(request.MovieId, request.HallId, cancellationToken);

        if (await _sessionRepository.HasOverlapAsync(request.HallId, request.StartTime, request.EndTime, null, cancellationToken))
        {
            throw new ConflictException("Session overlaps with existing session in this hall");
        }

        var session = _mapper.Map<Session>(request);
        var created = await _sessionRepository.AddAsync(session, cancellationToken);
        return _mapper.Map<SessionDto>(created);
    }

    public async Task<SessionDto> UpdateAsync(long id, UpdateSessionRequest request, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException($"Session with id {id} not found");

        await EnsureMovieAndHallExist(request.MovieId, request.HallId, cancellationToken);

        if (await _sessionRepository.HasOverlapAsync(request.HallId, request.StartTime, request.EndTime, id, cancellationToken))
        {
            throw new ConflictException("Session overlaps with existing session in this hall");
        }

        _mapper.Map(request, session);
        session.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _sessionRepository.UpdateAsync(session, cancellationToken);
        return _mapper.Map<SessionDto>(updated);
    }

    public async Task<SessionDto> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException($"Session with id {id} not found");

        var deleted = await _sessionRepository.DeleteAsync(session, cancellationToken);
        return _mapper.Map<SessionDto>(deleted);
    }

    private async Task EnsureMovieAndHallExist(long movieId, long hallId, CancellationToken cancellationToken)
    {
        if (!await _movieRepository.ExistsByIdAsync(movieId, cancellationToken))
        {
            throw new NotFoundException($"Movie {movieId} not found");
        }

        var hall = await _hallRepository.GetByIdAsync(hallId, cancellationToken);
        if (hall is null)
        {
            throw new NotFoundException($"Hall {hallId} not found");
        }
    }
}
