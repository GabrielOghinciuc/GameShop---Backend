using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using Gamestore.Models;
using Gamestore.Data;

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
            var game = await _context.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found");
            }

            if (await _context.Games.AnyAsync(g => g.Id != id && g.Name.ToLower() == gameDto.Name.ToLower()))
            {
                return BadRequest($"A game with the name '{gameDto.Name}' already exists");
            }

            _context.Attach(gameDto);
            _context.Entry(gameDto).Property(g => g.Name).IsModified = true;
            _context.Entry(gameDto).Property(g => g.Description).IsModified = true;
            _context.Entry(gameDto).Property(g => g.Genre).IsModified = true;
            _context.Entry(gameDto).Property(g => g.Image).IsModified = true;
            _context.Entry(gameDto).Property(g => g.OriginalPrice).IsModified = true;
            _context.Entry(gameDto).Property(g => g.DiscountedPrice).IsModified = true;
            _context.Entry(gameDto).Property(g => g.Rating).IsModified = true;
            _context.Entry(gameDto).Property(g => g.showFullDescription).IsModified = true;
            _context.Entry(gameDto).Property(g => g.showOnFirstPage).IsModified = true;
            _context.Entry(gameDto).Property(g => g.Platforms).IsModified = true;

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
            var game = await _context.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
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
    [OutputCache(PolicyName = "LongCache")]
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
    [OutputCache(PolicyName = "LongCache")]
    public async Task<IActionResult> GetSiteGames()
    {
        try
        {
            var siteGames = await _context.Games
                .Select(g => new LimitedGameDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Image = g.Image,
                    OriginalPrice = g.OriginalPrice,
                    DiscountedPrice = g.DiscountedPrice,
                    Platforms = g.Platforms,
                    showFullDescription = g.showFullDescription
                })
                .ToListAsync();

            return Ok(siteGames);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving site games: {ex.Message}");
        }
    }

    [HttpGet("featured-games")]
    public async Task<IActionResult> GetFeaturedGames()
    {
        try
        {
            var featuredGames = await _context.Games
                .AsNoTracking()
                .Where(g => g.showOnFirstPage)
                .Select(g => new LimitedGameDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Image = g.Image,
                    OriginalPrice = g.OriginalPrice,
                    DiscountedPrice = g.DiscountedPrice,
                    Platforms = g.Platforms,
                    showFullDescription = g.showFullDescription
                })
                .ToListAsync();

            return Ok(featuredGames);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving featured games: {ex.Message}");
        }
    }

    [HttpPut("edit-game/{id}")]
    public async Task<IActionResult> EditGame(int id, [FromBody] GameDto gameDto)
    {
        try
        {
            var game = await _context.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found");
            }

            if (await _context.Games.AnyAsync(g => g.Id != id && g.Name.ToLower() == gameDto.Name.ToLower()))
            {
                return BadRequest($"A game with the name '{gameDto.Name}' already exists");
            }

            _context.Attach(gameDto);
            _context.Entry(gameDto).Property(g => g.Name).IsModified = true;
            _context.Entry(gameDto).Property(g => g.Description).IsModified = true;
            _context.Entry(gameDto).Property(g => g.Genre).IsModified = true;
            _context.Entry(gameDto).Property(g => g.Image).IsModified = true;
            _context.Entry(gameDto).Property(g => g.OriginalPrice).IsModified = true;
            _context.Entry(gameDto).Property(g => g.DiscountedPrice).IsModified = true;
            _context.Entry(gameDto).Property(g => g.Rating).IsModified = true;
            _context.Entry(gameDto).Property(g => g.showFullDescription).IsModified = true;
            _context.Entry(gameDto).Property(g => g.showOnFirstPage).IsModified = true;
            _context.Entry(gameDto).Property(g => g.Platforms).IsModified = true;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while editing the game: {ex.Message}");
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchGames([FromQuery] string query)
    {
        try
        {
            var games = await _context.Games
                .AsNoTracking()
                .Where(g => g.Name.ToLower().Contains(query.ToLower()))
                .Select(g => new LimitedGameDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Image = g.Image,
                    OriginalPrice = g.OriginalPrice,
                    DiscountedPrice = g.DiscountedPrice,
                    Platforms = g.Platforms,
                    showFullDescription = g.showFullDescription
                })
                .ToListAsync();

            return Ok(games);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while searching for games: {ex.Message}");
        }
    }
}
