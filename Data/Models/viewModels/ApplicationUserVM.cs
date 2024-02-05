using Microsoft.AspNetCore.Identity;

namespace PortHub.Api.Tokens.Data.Models.viewModels
{
    public class ApplicationUserVM : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
