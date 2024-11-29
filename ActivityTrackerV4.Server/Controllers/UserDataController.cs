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

        if (userData == null)
            return Ok(null);

        if (string.IsNullOrWhiteSpace(userData.JsonData))
        {
            return Ok(null);
        }

        // Decode jsonData before returning it
        var decryptedData = EncryptionHelper.Decrypt(userData.JsonData);
        return Ok(decryptedData);
    }

    [HttpGet("firstName")]
    [Authorize]
    public async Task<ActionResult<string>> GetUserFirstName()
    {
        var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userData = await _context.UserData.SingleOrDefaultAsync(x => x.Email.ToString() == email);

        if (userData == null)
            return Ok(null);

        return Ok(userData.FirstName);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SaveUserData([FromBody] string jsonData)
    {
        var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userData = await _context.UserData.SingleOrDefaultAsync(x => x.Email.ToString() == email);

        // Encode jsonData before saving it
        var encryptedData = EncryptionHelper.Encrypt(jsonData);

        if (userData != null)
        {
            userData.JsonData = encryptedData;
            _context.UserData.Update(userData);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

}
