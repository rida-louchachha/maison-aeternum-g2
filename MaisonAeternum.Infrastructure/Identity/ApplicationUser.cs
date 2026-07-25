using Microsoft.AspNetCore.Identity;

namespace MaisonAeternum.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public int? ProfileImageId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
