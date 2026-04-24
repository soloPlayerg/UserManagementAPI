using UserManagementAPI.Models;

namespace UserManagementAPI.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new()
    {
        new User
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice.johnson@techhive.com",
            Department = "HR",
            Role = "Administrator"
        },
        new User
        {
            Id = 2,
            FirstName = "Brian",
            LastName = "Smith",
            Email = "brian.smith@techhive.com",
            Department = "IT",
            Role = "Support Analyst"
        }
    };

    private int _nextId = 3;

    public IEnumerable<User> GetAll()
    {
        return _users.OrderBy(u => u.Id);
    }

    public User? GetById(int id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public User Add(User user)
    {
        user.Id = _nextId++;
        _users.Add(user);
        return user;
    }

    public bool Update(User user)
    {
        var existingUser = GetById(user.Id);
        if (existingUser is null)
        {
            return false;
        }

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.Department = user.Department;
        existingUser.Role = user.Role;

        return true;
    }

    public bool Delete(int id)
    {
        var user = GetById(id);
        if (user is null)
        {
            return false;
        }

        _users.Remove(user);
        return true;
    }

    public bool EmailExists(string email, int? excludeId = null)
    {
        return _users.Any(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
            (!excludeId.HasValue || u.Id != excludeId.Value));
    }
}