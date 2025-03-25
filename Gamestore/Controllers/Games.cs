using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gamestore.Models;
using Gamestore.Data;
using Gamestore.Services;

namespace Gamestore.Controllers;

[ApiController]
[Route("[controller]")]
public class GamesController : ControllerBase
{
    private readonly GamestoreContext _context;

    public GamesController(GamestoreContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGame([FromBody] GameDto gameDto)
    {
        try
        {
            if (await _context.Games.AnyAsync(g => g.Name.ToLower() == gameDto.Name.ToLower()))
            {
                return BadRequest($"A game with the name '{gameDto.Name}' already exists");
            }

            await _context.Games.AddAsync(gameDto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGame), new { id = gameDto.Id }, gameDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while creating the game: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGame(int id, [FromBody] GameDto gameDto)
    {
        try
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found");
            }

            if (await _context.Games.AnyAsync(g => g.Id != id && g.Name.ToLower() == gameDto.Name.ToLower()))
            {
                return BadRequest($"A game with the name '{gameDto.Name}' already exists");
            }

            game.Name = gameDto.Name;
            game.Description = gameDto.Description;
            game.Genre = gameDto.Genre;
            game.Image = gameDto.Image;
            game.OriginalPrice = gameDto.OriginalPrice;
            game.DiscountedPrice = gameDto.DiscountedPrice;
            game.Rating = gameDto.Rating;
            game.showFullDescription = gameDto.showFullDescription;
            game.showOnFirstPage = gameDto.showOnFirstPage;
            game.Platforms = gameDto.Platforms;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while updating the game: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGame(int id)
    {
        try
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found");
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while deleting the game: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGame(int id)
    {
        try
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found");
            }

            return Ok(game);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving the game: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGames()
    {
        try
        {
            var games = await _context.Games.ToListAsync();
            return Ok(games);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving games: {ex.Message}");
        }
    }

    [HttpGet("game-list")]
    public async Task<IActionResult> GetSiteGames()
    {
        try
        {
            var siteGames = await _context.Games
                .Select(g => new LimitedGameDto
                {
                    Name = g.Name,
                    Image = g.Image,
                    OriginalPrice = g.OriginalPrice,
                    DiscountedPrice = g.DiscountedPrice,
                    Platforms = g.Platforms
                })
                .ToListAsync();

            return Ok(siteGames);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving site games: {ex.Message}");
        }
    }
}
