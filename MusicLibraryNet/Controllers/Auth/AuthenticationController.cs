using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicLibraryNet.Database;
using MusicLibraryNet.Database.Auth;
using MusicLibraryNet.Dto.Auth;

namespace MusicLibraryNet.Controllers.Auth;

[ApiController]
[Route("[controller]")]
public class AuthenticationController(
    UserManager<MusicUser> userManager,
    SignInManager<MusicUser> signInManager,
    ApplicationDbContext databaseContext)
    : ControllerBase
{
    [HttpPost("auth")]
    public async Task<IActionResult> Authenticate(AuthenticationDto authenticationDto)
    {
        var user = await userManager.FindByNameAsync(authenticationDto.UserName);
        if (user is null)
            return Unauthorized();

       
        var passCheckResult = await signInManager.CheckPasswordSignInAsync(user, authenticationDto.Password, false);
        if (!passCheckResult.Succeeded)
            return Unauthorized();
        
        await signInManager.SignInWithClaimsAsync(user, true, [
            new Claim("IsAdmin", "false"),
            new Claim("IsSystemAdmin", "false")
        ]);
        return Ok();   
    }

   
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (HttpContext.User.Identity is null) 
            return Unauthorized();
        
        await signInManager.SignOutAsync();
        return Ok();
    }

    [HttpPost("signup")]
    public async Task<IActionResult> CreateUser(SignUpDto signUpDto)
    {
        if (await userManager.FindByNameAsync(signUpDto.UserName) is not null)
            return Unauthorized("This username already belongs to someone. Try something else");

        var user = new MusicUser
        {
            Email = signUpDto.Email,
            UserName = signUpDto.UserName,
        };
        var creationResult = await userManager.CreateAsync(user);
        if (!creationResult.Succeeded)
            return BadRequest(); // TODO add error text
        
        var passSetResult = await userManager.AddPasswordAsync(user, signUpDto.Password);
        if (!passSetResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return BadRequest($"Invalid password {passSetResult.Errors.First().Description}");
        }
        return Ok();
    }

    [Authorize]
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok();
    }
}