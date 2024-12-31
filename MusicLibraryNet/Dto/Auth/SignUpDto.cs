namespace MusicLibraryNet.Dto.Auth;

public class SignUpDto
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string PhoneNumber { get; set; }
}