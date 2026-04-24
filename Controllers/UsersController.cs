using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;
using UserManagementAPI.Repositories;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers()
    {
        var users = _userRepository.GetAll();
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public ActionResult<User> GetUserById(int id)
    {
        var user = _userRepository.GetById(id);

        if (user is null)
        {
            return NotFound(new { error = "User not found." });
        }

        return Ok(user);
    }

    [HttpPost]
    public ActionResult<User> CreateUser([FromBody] CreateUserDto dto)
    {
        if (_userRepository.EmailExists(dto.Email))
        {
            return Conflict(new { error = "A user with this email already exists." });
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim(),
            Department = dto.Department.Trim(),
            Role = dto.Role.Trim()
        };

        var createdUser = _userRepository.Add(user);

        return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
    }

    [HttpPut("{id:int}")]
    public ActionResult<User> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var existingUser = _userRepository.GetById(id);

        if (existingUser is null)
        {
            return NotFound(new { error = "User not found." });
        }

        if (_userRepository.EmailExists(dto.Email, id))
        {
            return Conflict(new { error = "A user with this email already exists." });
        }

        existingUser.FirstName = dto.FirstName.Trim();
        existingUser.LastName = dto.LastName.Trim();
        existingUser.Email = dto.Email.Trim();
        existingUser.Department = dto.Department.Trim();
        existingUser.Role = dto.Role.Trim();

        _userRepository.Update(existingUser);

        return Ok(existingUser);
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        var deleted = _userRepository.Delete(id);

        if (!deleted)
        {
            return NotFound(new { error = "User not found." });
        }

        return NoContent();
    }
}