using Application.Abstractions.Repositories;
using Application.DTOs.Genres;
using Application.Exceptions;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.Tests;

public class GenreServiceTests
{
    private readonly Mock<IGenreRepository> _repo = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly GenreService _sut;

    public GenreServiceTests()
    {
        _sut = new GenreService(_repo.Object, _mapper.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllGenres()
    {
        var genres = new List<Genre> { new() { Id = 1, Name = "Action" } };
        var dtos = new List<GenreDto> { new() { Id = 1, Name = "Action" } };
        _repo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(genres);
        _mapper.Setup(x => x.Map<IReadOnlyList<GenreDto>>(genres)).Returns(dtos);

        var result = await _sut.GetAllAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsGenre()
    {
        var genre = new Genre { Id = 5, Name = "Drama" };
        _repo.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(genre);
        _mapper.Setup(x => x.Map<GenreDto>(genre)).Returns(new GenreDto { Id = 5, Name = "Drama" });

        var result = await _sut.GetByIdAsync(5);

        Assert.Equal("Drama", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        _repo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Genre?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task CreateAsync_WithUniqueName_CreatesGenre()
    {
        _repo.Setup(x => x.ExistsByNameAsync("New", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mapper.Setup(x => x.Map<Genre>(It.IsAny<CreateGenreRequest>())).Returns(new Genre { Name = "New" });
        _repo.Setup(x => x.AddAsync(It.IsAny<Genre>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Genre { Id = 10, Name = "New" });
        _mapper.Setup(x => x.Map<GenreDto>(It.IsAny<Genre>())).Returns(new GenreDto { Id = 10, Name = "New" });

        var result = await _sut.CreateAsync(new CreateGenreRequest { Name = "New" });

        Assert.Equal(10, result.Id);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsConflict()
    {
        _repo.Setup(x => x.ExistsByNameAsync("Dup", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateAsync(new CreateGenreRequest { Name = "Dup" }));
    }

    [Fact]
    public async Task UpdateAsync_WhenExistsAndUnique_UpdatesGenre()
    {
        var genre = new Genre { Id = 1, Name = "Old" };
        _repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(genre);
        _repo.Setup(x => x.ExistsByNameAsync("Updated", 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mapper.Setup(x => x.Map<GenreDto>(It.IsAny<Genre>())).Returns(new GenreDto { Id = 1, Name = "Updated" });

        var result = await _sut.UpdateAsync(1, new UpdateGenreRequest { Name = "Updated" });

        Assert.Equal("Updated", result.Name);
        _repo.Verify(x => x.UpdateAsync(It.IsAny<Genre>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsNotFound()
    {
        _repo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Genre?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateAsync(99, new UpdateGenreRequest { Name = "X" }));
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ThrowsConflict()
    {
        var genre = new Genre { Id = 1, Name = "Old" };
        _repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(genre);
        _repo.Setup(x => x.ExistsByNameAsync("Existing", 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.UpdateAsync(1, new UpdateGenreRequest { Name = "Existing" }));
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_DeletesAndReturns()
    {
        var genre = new Genre { Id = 1, Name = "X" };
        _repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(genre);
        _repo.Setup(x => x.DeleteAsync(genre, It.IsAny<CancellationToken>())).ReturnsAsync(genre);
        _mapper.Setup(x => x.Map<GenreDto>(genre)).Returns(new GenreDto { Id = 1 });

        var result = await _sut.DeleteAsync(1);

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ThrowsNotFound()
    {
        _repo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Genre?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(99));
    }
}
