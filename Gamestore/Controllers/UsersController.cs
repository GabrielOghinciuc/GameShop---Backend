using Gamestore.Data;
using Gamestore.Models;
using Gamestore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly GamestoreContext _context;
        private readonly IFileStorage _fileStorage;
        private const string container = "users";

        public UsersController(GamestoreContext context, IFileStorage fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email || u.Username == userDto.Username))
            {
                return BadRequest("A user with the same email or username already exists.");
            }

            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Password = userDto.Password,
                FullName = userDto.FullName,
                BirthDate = userDto.BirthDate,
                JoinedOn = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                ProfilePicture = "/user/user.jpg",
                IsClient = true,
                IsGameDeveloper = false,
                IsAdmin = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginRequest.Email && u.Password == loginRequest.Password);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                JoinedOn = user.JoinedOn,
                LastLogin = user.LastLogin,
                IsClient = user.IsClient,
                IsGameDeveloper = user.IsGameDeveloper,
                IsAdmin = user.IsAdmin,
                ProfilePicture = user.ProfilePicture,
                BoughtGames = user.BoughtGames 
            });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(user => new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    BirthDate = user.BirthDate,
                    JoinedOn = user.JoinedOn,
                    LastLogin = user.LastLogin,
                    IsClient = user.IsClient,
                    IsGameDeveloper = user.IsGameDeveloper,
                    IsAdmin = user.IsAdmin,
                    ProfilePicture = user.ProfilePicture,
                    BoughtGames = user.BoughtGames
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    BirthDate = u.BirthDate,
                    JoinedOn = u.JoinedOn,
                    LastLogin = u.LastLogin,
                    IsClient = u.IsClient,
                    IsGameDeveloper = u.IsGameDeveloper,
                    IsAdmin = u.IsAdmin,
                    ProfilePicture = u.ProfilePicture
                })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user);
        }

        [HttpPut("edit-profile/{id:guid}")]
        public async Task<IActionResult> EditProfile(Guid id, [FromForm] UserUpdateDto userUpdateDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                if (!string.IsNullOrEmpty(userUpdateDto.Username) &&
                    await _context.Users.AnyAsync(u => u.Id != id && u.Username == userUpdateDto.Username))
                {
                    return BadRequest("Username is already taken");
                }

                if (!string.IsNullOrEmpty(userUpdateDto.Email) &&
                    await _context.Users.AnyAsync(u => u.Id != id && u.Email == userUpdateDto.Email))
                {
                    return BadRequest("Email is already taken");
                }

                if (userUpdateDto.ProfilePicture != null)
                {
                    user.ProfilePicture = await _fileStorage.Edit(user.ProfilePicture, container, userUpdateDto.ProfilePicture);
                }

                if (!string.IsNullOrEmpty(userUpdateDto.Username)) user.Username = userUpdateDto.Username;
                if (!string.IsNullOrEmpty(userUpdateDto.Email)) user.Email = userUpdateDto.Email;
                if (!string.IsNullOrEmpty(userUpdateDto.Password)) user.Password = userUpdateDto.Password;
                if (!string.IsNullOrEmpty(userUpdateDto.FullName)) user.FullName = userUpdateDto.FullName;
                if (userUpdateDto.BirthDate.HasValue) user.BirthDate = userUpdateDto.BirthDate.Value;

                await _context.SaveChangesAsync();

                return Ok(new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    BirthDate = user.BirthDate,
                    ProfilePicture = user.ProfilePicture,
                    JoinedOn = user.JoinedOn,
                    LastLogin = user.LastLogin,
                    IsClient = user.IsClient,
                    IsGameDeveloper = user.IsGameDeveloper,
                    IsAdmin = user.IsAdmin
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the profile: {ex.Message}");
            }
        }

        [HttpPatch("{id:guid}/set-admin-status")]
        public async Task<IActionResult> SetAdminStatus(Guid id, [FromBody] bool isAdmin)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            user.IsAdmin = isAdmin;
            await _context.SaveChangesAsync();

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                ProfilePicture = user.ProfilePicture,
                JoinedOn = user.JoinedOn,
                LastLogin = user.LastLogin,
                IsClient = user.IsClient,
                IsGameDeveloper = user.IsGameDeveloper,
                IsAdmin = user.IsAdmin
            });
        }

        [HttpPatch("{id:guid}/set-developer-status")]
        public async Task<IActionResult> SetDeveloperStatus(Guid id, [FromBody] bool isGameDeveloper)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            user.IsGameDeveloper = isGameDeveloper;
            await _context.SaveChangesAsync();

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                ProfilePicture = user.ProfilePicture,
                JoinedOn = user.JoinedOn,
                LastLogin = user.LastLogin,
                IsClient = user.IsClient,
                IsGameDeveloper = user.IsGameDeveloper,
                IsAdmin = user.IsAdmin
            });
        }

        [HttpPost("{userId}/add-games")]
        public async Task<IActionResult> AddGamesToUser(Guid userId, [FromBody] List<int> gameIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (gameIds == null || !gameIds.Any())
                {
                    return BadRequest("No game IDs provided.");
                }

                // Use tracking query to ensure proper state management
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);
                    
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Validate if all games exist
                var existingGames = await _context.Games
                    .Where(g => gameIds.Contains(g.Id))
                    .Select(g => new { g.Id, g.Name })
                    .ToListAsync();

                if (existingGames.Count != gameIds.Count)
                {
                    var nonExistingGames = gameIds.Except(existingGames.Select(g => g.Id));
                    return BadRequest($"Games with IDs {string.Join(", ", nonExistingGames)} do not exist.");
                }

                user.BoughtGames ??= new List<int>();
                var newGameIds = gameIds.Where(gameId => !user.BoughtGames.Contains(gameId)).ToList();

                if (!newGameIds.Any())
                {
                    return BadRequest("All provided games are already in the user's inventory.");
                }

                user.BoughtGames.AddRange(newGameIds);
                
                // Mark entity as modified
                _context.Entry(user).Property(u => u.BoughtGames).IsModified = true;
                await _context.SaveChangesAsync();
                
                // Commit transaction
                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = "Games added successfully.",
                    AddedGames = existingGames.Where(g => newGameIds.Contains(g.Id)).ToList(),
                    TotalGamesCount = user.BoughtGames.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"An error occurred while adding games to the user: {ex.Message}");
            }
        }

        [HttpGet("{userId}/bought-games")]
        public async Task<IActionResult> GetUserBoughtGames(Guid userId)
        {
            try 
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                if (user.BoughtGames == null || !user.BoughtGames.Any())
                {
                    return Ok(new { Games = new List<GameDto>(), TotalGames = 0 });
                }

                var games = await _context.Games
                    .Where(g => user.BoughtGames.Contains(g.Id))
                    .Select(g => new
                    {
                        g.Id,
                        g.Name,
                        g.Description,
                        g.Image,
                        g.OriginalPrice,
                        g.DiscountedPrice,
                        g.Rating,
                        g.Genre
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Games = games,
                    TotalGames = games.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving user's games: {ex.Message}");
            }
        }

        [HttpDelete("{userId}/remove-game")]
        public async Task<IActionResult> RemoveGameFromUser(Guid userId, [FromBody] int gameId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                if (user.BoughtGames == null || !user.BoughtGames.Contains(gameId))
                {
                    return BadRequest("The specified game is not in the user's inventory.");
                }

                var game = await _context.Games
                    .Where(g => g.Id == gameId)
                    .Select(g => new { g.Id, g.Name })
                    .FirstOrDefaultAsync();

                if (game == null)
                {
                    return NotFound("Game not found.");
                }

                user.BoughtGames.Remove(gameId);
                _context.Entry(user).Property(u => u.BoughtGames).IsModified = true;
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();

                return Ok(new 
                { 
                    Message = $"Game '{game.Name}' removed successfully.",
                    RemovedGameId = gameId,
                    RemainingGamesCount = user.BoughtGames.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"An error occurred while removing the game: {ex.Message}");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            if (!string.IsNullOrEmpty(user.ProfilePicture) && user.ProfilePicture != "/user/user.jpg")
            {
                await _fileStorage.Delete(user.ProfilePicture, container);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok($"User with ID {id} was deleted successfully");
        }
    }
}
