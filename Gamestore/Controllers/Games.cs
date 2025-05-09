﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using Gamestore.Models;
using Gamestore.Data;
using Gamestore.Services;
using Microsoft.AspNetCore.Http;

namespace Gamestore.Controllers;

[ApiController]
[Route("[controller]")]
public class GamesController : ControllerBase
{
    private readonly GamestoreContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string container = "games";

    public GamesController(GamestoreContext context, IFileStorage fileStorage, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _fileStorage = fileStorage;
        _httpContextAccessor = httpContextAccessor;
    }

    private static string GetFullImageUrl(string imagePath, HttpContext context)
    {
        if (string.IsNullOrEmpty(imagePath)) return "";

        if (imagePath.StartsWith("http") && !imagePath.Contains("/games/"))
        {
            return imagePath;
        }

        if (imagePath.Contains("/games/"))
        {
            imagePath = imagePath[(imagePath.IndexOf("/games/") + 7)..];
        }

        return $"{context.Request.Scheme}://{context.Request.Host}/games/{imagePath}";
    }

    [HttpGet("image/{*imagePath}")]
    public IActionResult GetImage(string imagePath)
    {
        imagePath = imagePath?.Replace("..", "").TrimStart('/');

        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "games", imagePath ?? "");
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var extension = Path.GetExtension(path).ToLower();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        var imageFileStream = System.IO.File.OpenRead(path);
        return File(imageFileStream, contentType);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGame([FromForm] GameDto gameDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"[ModelError] {entry.Key}: {error.ErrorMessage}");
                    }
                }
                return BadRequest(ModelState);
            }

            if (await _context.Games.AnyAsync(g => g.Name.ToLower() == gameDto.Name.ToLower()))
            {
                return BadRequest($"A game with the name '{gameDto.Name}' already exists");
            }

            if (gameDto.Picture != null)
            {
                gameDto.Image = await _fileStorage.Store(container, gameDto.Picture);
            }

            if (gameDto.Rating < 0)
            {
                gameDto.Rating = 0;
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
    public async Task<IActionResult> UpdateGame(int id, [FromForm] GameDto gameDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"[ModelError] {entry.Key}: {error.ErrorMessage}");
                    }
                }
                return BadRequest(ModelState);
            }

            var game = await _context.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found");
            }

            if (gameDto.Picture != null)
            {
                gameDto.Image = await _fileStorage.Edit(game.Image, container, gameDto.Picture);
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetGame(int id)
    {
        try
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found");
            }

            if (!game.Image.StartsWith("http"))
            {
                game.Image = GetFullImageUrl(game.Image, HttpContext);
            }
            return Ok(game);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving the game: {ex.Message}");
        }
    }

    [HttpPost("batch")]
    public async Task<IActionResult> GetGamesByIds([FromBody] GetGamesByIdsRequest request)
    {
        try
        {
            if (request.GameIds == null || !request.GameIds.Any())
            {
                return BadRequest("No game IDs provided");
            }

            var games = await _context.Games
                .Where(g => request.GameIds.Contains(g.Id))
                .ToListAsync();

            foreach (var game in games)
            {
                if (!game.Image.StartsWith("http"))
                {
                    game.Image = GetFullImageUrl(game.Image, HttpContext);
                }
            }

            var foundIds = games.Select(g => g.Id).ToHashSet();
            var notFoundIds = request.GameIds.Where(id => !foundIds.Contains(id)).ToList();

            var response = new
            {
                Games = games,
                NotFoundIds = notFoundIds,
                TotalFound = games.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving the games: {ex.Message}");
        }
    }

    [HttpGet]
    [OutputCache(PolicyName = "LongCache")]
    public async Task<IActionResult> GetAllGames()
    {
        try
        {
            var games = await _context.Games.ToListAsync();
            foreach (var game in games)
            {
                game.Image = GetFullImageUrl(game.Image, HttpContext);
            }
            return Ok(games);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving games: {ex.Message}");
        }
    }

    [HttpGet("game-list/{page:int=1}")]
    [OutputCache(PolicyName = "LongCache")]
    public async Task<IActionResult> GetSiteGames(int page)
    {
        try
        {
            const int pageSize = 15;
            var httpContext = HttpContext;

            var query = _context.Games
                .AsNoTracking()
                .Select(g => new LimitedGameDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Image = g.Image.StartsWith("http") ? g.Image : $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/games/{g.Image}",
                    OriginalPrice = g.OriginalPrice,
                    DiscountedPrice = g.DiscountedPrice,
                    Platforms = g.Platforms,
                    showFullDescription = g.showFullDescription
                });

            var totalGames = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalGames / (double)pageSize);

            page = Math.Max(1, Math.Min(page, totalPages));

            var games = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new
            {
                Games = games,
                CurrentPage = page,
                PageSize = pageSize,
                TotalGames = totalGames,
                TotalPages = totalPages,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            };

            return Ok(response);
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
            var httpContext = HttpContext;
            var featuredGames = await _context.Games
                .AsNoTracking()
                .Where(g => g.showOnFirstPage)
                .Select(g => new LimitedGameDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Image = g.Image.StartsWith("http") ? g.Image : $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/games/{g.Image}",
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
    public async Task<IActionResult> EditGame(int id, [FromForm] GameDto gameDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"[ModelError] {entry.Key}: {error.ErrorMessage}");
                    }
                }
                return BadRequest(ModelState);
            }

            var game = await _context.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found");
            }

            if (await _context.Games.AnyAsync(g => g.Id != id && g.Name.ToLower() == gameDto.Name.ToLower()))
            {
                return BadRequest($"A game with the name '{gameDto.Name}' already exists");
            }

            // Set the Id from the route parameter
            gameDto.Id = id;

            if (gameDto.Picture != null)
            {
                gameDto.Image = await _fileStorage.Edit(game.Image, container, gameDto.Picture);
            }
            else
            {
                gameDto.Image = game.Image;
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
            var httpContext = HttpContext;
            var games = await _context.Games
                .AsNoTracking()
                .Where(g => g.Name.ToLower().Contains(query.ToLower()))
                .Select(g => new LimitedGameDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Image = g.Image.StartsWith("http") ? g.Image : $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/games/{g.Image}",
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

    [HttpGet("genre/{genre}")]
    public async Task<IActionResult> GetGamesByGenre(string genre)
    {
        try
        {
            var httpContext = HttpContext;
            var games = await _context.Games
                .AsNoTracking()
                .Where(g => g.Genre.ToLower().Contains(genre.ToLower()))
                .Select(g => new LimitedGameDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Image = g.Image.StartsWith("http") ? g.Image : $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/games/{g.Image}",
                    OriginalPrice = g.OriginalPrice,
                    DiscountedPrice = g.DiscountedPrice,
                    Platforms = g.Platforms,
                    showFullDescription = g.showFullDescription
                })
                .ToListAsync();

            if (!games.Any())
            {
                return NotFound($"No games found for genre: {genre}");
            }

            return Ok(games);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving games by genre: {ex.Message}");
        }
    }

    [HttpPost("{id:int}/review")]
    public async Task<IActionResult> ReviewGame(int id, [FromBody] GameReviewDto reviewDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound($"Game with ID {id} not found");
            }

            game.Rating = (game.Rating + reviewDto.Rating) / 2;

            _context.Entry(game).Property(g => g.Rating).IsModified = true;
            await _context.SaveChangesAsync();

            return Ok(new { 
                GameId = game.Id, 
                NewRating = game.Rating 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while reviewing the game: {ex.Message}");
        }
    }
}
