using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Application
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string HelpDeskAgent = "HelpDeskAgent";
    }

    public record UserDto(string Id, string Email, IEnumerable<string> Roles);
    
    public interface IUserManagementService
    {
        Task<IReadOnlyList<UserDto>> GetUsersAsync();
        Task CreateUserAsync(string email, string password, string role);
        Task AssignRoleAsync(string userId, string role);
        Task RemoveRoleAsync(string userId, string role);
        Task DeleteUserAsync(string userId);
    }
}
