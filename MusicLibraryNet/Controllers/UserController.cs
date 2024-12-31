using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicLibraryNet.Database;
using MusicLibraryNet.Database.Auth;
using MusicLibraryNet.Dto;
using MusicLibraryNet.Dto.Auth;

namespace MusicLibraryNet.Controllers;

[Authorize]
[ApiController]
[Route("/api/v1/[controller]")]
public class UsersController(
    ApplicationDbContext context,
    UserManager<MusicUser> userManager,
    IHttpContextAccessor httpContextAccessor
) : Controller
{
    [HttpPut]
    public async Task<IActionResult> Put(UserDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return Unauthorized();
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return Unauthorized();
    
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.DateOfBirth = dto.DateOfBirth;
        user.PhoneNumber = dto.PhoneNumber;
        
        await userManager.UpdateAsync(user);
        
        await context.SaveChangesAsync();
        return Ok();
        
    }
}