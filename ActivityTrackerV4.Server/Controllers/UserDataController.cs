using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorAuthAPI.Controllers;

[ApiController]
[Route("api/userdata")]
public class UserDataController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserDataController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<string>> GetUserData()
    {
        var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userData = await _context.UserData.SingleOrDefaultAsync(x => x.Email.ToString() == email);
        return userData != null ? Ok(userData.JsonData) : NotFound();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SaveUserData([FromBody] string jsonData)
    {
        var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userData = await _context.UserData.SingleOrDefaultAsync(x => x.Email.ToString() == email);

        if (userData != null)
        {
            userData.JsonData = jsonData;
            _context.UserData.Update(userData);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

}
