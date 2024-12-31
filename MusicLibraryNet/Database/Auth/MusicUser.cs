using Microsoft.AspNetCore.Identity;

namespace MusicLibraryNet.Database.Auth;

public class MusicUser : IdentityUser<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly DateOfBirth { get; set; }
}