using Application.Abstractions.Services;
using Application.DTOs.Movies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovieController(IMovieService movieService) : ControllerBase
{
    private readonly IMovieService _movieService = movieService;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MovieDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var movies = await _movieService.GetAllAsync(cancellationToken);
        return Ok(movies);
    }

    [HttpGet("{id}", Name = "GetMovieById")]
    public async Task<ActionResult<MovieDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var movie = await _movieService.GetByIdAsync(id, cancellationToken);
        return Ok(movie);
    }

    [Authorize(Policy="Staff")]
    [HttpPost]
    public async Task<ActionResult<MovieDto>> CreateMovieAsync(
        [FromBody]CreateMovieRequest movie, 
        CancellationToken cancellationToken = default)
    {
        var created = await _movieService.CreateAsync(movie, cancellationToken);
        return CreatedAtRoute("GetMovieById", new { id = created.Id }, created);
    }
    
    [HttpPut("{id:long}")]
    public async Task<ActionResult<MovieDto>> UpdateMovieAsync(
        long id,
        [FromBody] UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var updatedMovie = await _movieService.UpdateAsync(id, request, cancellationToken);
        return Ok(updatedMovie);
    }
    
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<MovieDto>> DeleteMovieAsync(long id, CancellationToken cancellationToken)
    {
        var deletedMovie = await _movieService.DeleteAsync(id, cancellationToken);
        return Ok(deletedMovie);
    }
}
