using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Halls;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class HallService(IHallRepository hallRepository, IMapper mapper) : IHallService
{
    private readonly IHallRepository _hallRepository = hallRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IReadOnlyList<HallDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var halls = await _hallRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<HallDto>>(halls);
    }

    public async Task<HallDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var hall = await _hallRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException($"Hall with id {id} not found");
        return _mapper.Map<HallDto>(hall);
    }

    public async Task<HallDto> CreateAsync(CreateHallRequest request, CancellationToken cancellationToken = default)
    {
        if (await _hallRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new ConflictException("Hall with the same name already exists");
        }

        var hall = _mapper.Map<Hall>(request);
        var created = await _hallRepository.AddAsync(hall, cancellationToken);
        return _mapper.Map<HallDto>(created);
    }

    public async Task<HallDto> UpdateAsync(long id, UpdateHallRequest request, CancellationToken cancellationToken = default)
    {
        var hall = await _hallRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException($"Hall with id {id} not found");

        if (await _hallRepository.ExistsByNameAsync(request.Name, id, cancellationToken))
        {
            throw new ConflictException("Hall with the same name already exists");
        }

        _mapper.Map(request, hall);
        hall.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _hallRepository.UpdateAsync(hall, cancellationToken);
        return _mapper.Map<HallDto>(updated);
    }

    public async Task<HallDto> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var hall = await _hallRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException($"Hall with id {id} not found");

        var deleted = await _hallRepository.DeleteAsync(hall, cancellationToken);
        return _mapper.Map<HallDto>(deleted);
    }
}
