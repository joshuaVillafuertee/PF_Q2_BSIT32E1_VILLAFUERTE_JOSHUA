using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class PokemonController : ControllerBase
{
    private readonly AppDbContext _context;

    public PokemonController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPokemon()
    {
        try
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            // Create table if it doesn't exist (with ImageUrl column)
            using var createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS Pokemon (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    ImageUrl TEXT
                )";
            await createTableCommand.ExecuteNonQueryAsync();

            // Check if ImageUrl column exists (for backward compatibility)
            using var checkColumnCommand = connection.CreateCommand();
            checkColumnCommand.CommandText = "PRAGMA table_info(Pokemon)";
            using var columnReader = await checkColumnCommand.ExecuteReaderAsync();
            var hasImageUrlColumn = false;
            while (await columnReader.ReadAsync())
            {
                if (columnReader.GetString(1) == "ImageUrl")
                {
                    hasImageUrlColumn = true;
                    break;
                }
            }

            // Add ImageUrl column if it doesn't exist
            if (!hasImageUrlColumn)
            {
                using var addColumnCommand = connection.CreateCommand();
                addColumnCommand.CommandText = "ALTER TABLE Pokemon ADD COLUMN ImageUrl TEXT";
                await addColumnCommand.ExecuteNonQueryAsync();
            }

            // Check if table has data
            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Pokemon";
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            // Insert sample data if empty (with images)
            if (count == 0)
            {
                var samplePokemon = new[]
                {
                    new { Name = "Pikachu", ImageUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/25.png" },
                    new { Name = "Charizard", ImageUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/6.png" },
                    new { Name = "Bulbasaur", ImageUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/1.png" },
                    new { Name = "Squirtle", ImageUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/7.png" },
                    new { Name = "Eevee", ImageUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/133.png" }
                };
                
                foreach (var pokemon in samplePokemon)
                {
                    using var insertCommand = connection.CreateCommand();
                    insertCommand.CommandText = "INSERT INTO Pokemon (Name, ImageUrl) VALUES (@name, @imageUrl)";
                    
                    var nameParam = insertCommand.CreateParameter();
                    nameParam.ParameterName = "@name";
                    nameParam.Value = pokemon.Name;
                    insertCommand.Parameters.Add(nameParam);
                    
                    var imageUrlParam = insertCommand.CreateParameter();
                    imageUrlParam.ParameterName = "@imageUrl";
                    imageUrlParam.Value = pokemon.ImageUrl;
                    insertCommand.Parameters.Add(imageUrlParam);
                    
                    await insertCommand.ExecuteNonQueryAsync();
                }
            }

            // Get all Pokemon
            using var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "SELECT Id, Name, ImageUrl FROM Pokemon";
            
            using var reader = await selectCommand.ExecuteReaderAsync();
            var result = new List<object>();

            while (await reader.ReadAsync())
            {
                result.Add(new
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    imageUrl = reader.IsDBNull(2) ? null : reader.GetString(2)
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            // Return 500 for unexpected database errors
            return StatusCode(500, $"Database error: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddPokemon([FromBody] PokemonRequest request)
    {
        // Validate request
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Pokemon (Name, ImageUrl) VALUES (@name, @imageUrl)";
            
            var nameParam = command.CreateParameter();
            nameParam.ParameterName = "@name";
            nameParam.Value = request.Name;
            command.Parameters.Add(nameParam);
            
            var imageUrlParam = command.CreateParameter();
            imageUrlParam.ParameterName = "@imageUrl";
            imageUrlParam.Value = request.ImageUrl ?? (object)DBNull.Value;
            command.Parameters.Add(imageUrlParam);
            
            await command.ExecuteNonQueryAsync();

            return Ok(new { message = "Pokemon added successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePokemon(int id, [FromBody] PokemonRequest request)
    {
        // Validate request
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Pokemon SET Name = @name, ImageUrl = @imageUrl WHERE Id = @id";
            
            var idParam = command.CreateParameter();
            idParam.ParameterName = "@id";
            idParam.Value = id;
            command.Parameters.Add(idParam);
            
            var nameParam = command.CreateParameter();
            nameParam.ParameterName = "@name";
            nameParam.Value = request.Name;
            command.Parameters.Add(nameParam);
            
            var imageUrlParam = command.CreateParameter();
            imageUrlParam.ParameterName = "@imageUrl";
            imageUrlParam.Value = request.ImageUrl ?? (object)DBNull.Value;
            command.Parameters.Add(imageUrlParam);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                return NotFound("Pokemon not found");
            }

            return Ok(new { message = "Pokemon updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database error: {ex.Message}");
        }
    }
}

public class PokemonRequest
{
    [Required(ErrorMessage = "Pokemon name is required")]
    [MinLength(2, ErrorMessage = "Pokemon name must be at least 2 characters")]
    [MaxLength(50, ErrorMessage = "Pokemon name cannot exceed 50 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Url(ErrorMessage = "Image URL must be a valid URL")]
    public string? ImageUrl { get; set; }
}
