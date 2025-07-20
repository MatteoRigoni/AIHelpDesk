using AIHelpDesk.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Infrastructure
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<IdentityUser> _users;
        private readonly RoleManager<IdentityRole> _roles;

        public UserManagementService(
            UserManager<IdentityUser> users,
            RoleManager<IdentityRole> roles)
        {
            _users = users;
            _roles = roles;
        }

        public async Task<IReadOnlyList<UserDto>> GetUsersAsync()
        {
            var userEntities = await _users.Users.ToListAsync();

            var dtos = await Task.WhenAll(userEntities.Select(async u =>
            {
                var roles = await _users.GetRolesAsync(u);
                return new UserDto(u.Id, u.Email, roles);
            }));

            return dtos;
        }

        public async Task CreateUserAsync(string email, string password, string role)
        {
            var user = new IdentityUser { UserName = email, Email = email };
            var res = await _users.CreateAsync(user, password);
            if (!res.Succeeded) throw new Exception(res.Errors.First().Description);
            await _users.AddToRoleAsync(user, role);
        }

        public async Task AssignRoleAsync(string userId, string role)
        {
            var user = await _users.FindByIdAsync(userId);
            await _users.AddToRoleAsync(user, role);
        }

        public async Task RemoveRoleAsync(string userId, string role)
        {
            var user = await _users.FindByIdAsync(userId);
            await _users.RemoveFromRoleAsync(user, role);
        }

        public async Task DeleteUserAsync(string userId)
        {
            var u = await _users.FindByIdAsync(userId);
            if (u != null) await _users.DeleteAsync(u);
        }
    }
}
