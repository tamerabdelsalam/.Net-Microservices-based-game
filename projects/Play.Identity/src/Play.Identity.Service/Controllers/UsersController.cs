using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Play.Identity.Service.Entities;
using static Duende.IdentityServer.IdentityServerConstants;
using static Play.Identity.Service.Dtos;

namespace Play.Identity.Service.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize
(// Only allow access to the API from the local machine (that identity server is running on)
Policy = LocalApi.PolicyName
)]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
    }

    // /Users
    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> Get()
    {
        var users = userManager.Users.ToList().Select(user => user.AsDto());
        return Ok(users);
    }

    // /Users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetByIdAsync(string id)
    {
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return user.AsDto();
        }
    }

    // /Users/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> PutAsync(string id, UpdateUserDto userDto)
    {
        var user = await userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        user.Email = userDto.Email;
        user.UserName = userDto.Email;
        user.Gil = userDto.Gil;

        await userManager.UpdateAsync(user);

        return NoContent();
    }

    // /Users/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        await userManager.DeleteAsync(user);

        return NoContent();
    }
}
