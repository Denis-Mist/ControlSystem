using Microsoft.AspNetCore.Identity;

namespace YourProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Дополнительные поля можно добавлять здесь: FullName, Department и т.д.
        public string FullName { get; set; }
    }
}
