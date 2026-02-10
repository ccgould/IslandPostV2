using IslandPostApi.Contracts;
using IslandPostApi.Data;
using IslandPostApi.Models;
using Microsoft.EntityFrameworkCore;

namespace IslandPostApi.Services;

public class UserService : IUserService
{
    private readonly IslandPostDbContext _context;

    public UserService(IslandPostDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> List()
    {
        return await _context.Users
            .Include(u => u.IdRolNavigation)
            .ToListAsync();
    }

    public async Task<User> Add(User entity)
    {
        var userExists = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == entity.Email);

        if (userExists != null)
            throw new TaskCanceledException("The email already exists");

        _context.Users.Add(entity);
        await _context.SaveChangesAsync();

        var userCreated = await _context.Users
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsers == entity.IdUsers);

        if (userCreated == null)
            throw new TaskCanceledException("Failed to create user");

        return userCreated;
    }

    public async Task<User> Edit(User entity)
    {
        var userExists = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == entity.Email && u.IdUsers != entity.IdUsers);

        if (userExists != null)
            throw new TaskCanceledException("The email already exists");

        var userEdit = await _context.Users
            .FirstOrDefaultAsync(u => u.IdUsers == entity.IdUsers);

        if (userEdit == null)
            throw new TaskCanceledException("User not found");

        userEdit.Name = entity.Name;
        userEdit.Email = entity.Email;
        userEdit.Phone = entity.Phone;
        userEdit.IdRol = entity.IdRol;
        userEdit.IsActive = entity.IsActive;
        userEdit.Password = entity.Password;

        if (entity.Photo != null && entity.Photo.Length > 0)
        {
            userEdit.Photo = entity.Photo;
        }

        _context.Users.Update(userEdit);
        await _context.SaveChangesAsync();

        return await _context.Users
            .Include(u => u.IdRolNavigation)
            .FirstAsync(u => u.IdUsers == entity.IdUsers);
    }

    public async Task<bool> Delete(int idUser)
    {
        var userFound = await _context.Users
            .FirstOrDefaultAsync(u => u.IdUsers == idUser);

        if (userFound == null)
            throw new TaskCanceledException("User does not exist");

        _context.Users.Remove(userFound);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<User> GetByCredentials(string email, string password)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
    }

    public async Task<User> GetById(int idUser)
    {
        return await _context.Users
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsers == idUser);
    }
}