using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BeanScene.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeanScene.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

//this controller manages the 'view' of user roles, where the manager changes the role of any asp dotnet user
//this is how the manager can 'make' more managers, staff members, or revert them to a member

[Authorize(Roles = "Manager")] 
public class UserRoleViewModelsController : Controller
{ //this is where identity comes in. applicationuser/identity user are the ones registered through the register/login mechanism
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserRoleViewModelsController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var model = new List<UserRoleViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new UserRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = roles
            });
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UserRoleViewModel model, string[] selectedRoles)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.ToList();

        if (rolesToRemove.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove user roles");
                return View(model);
            }
        }

        if (selectedRoles != null && selectedRoles.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add user roles");
                return View(model);
            }
        }

        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var model = new UserRoleViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = roles
        };

        // Populate ViewBag.AllRoles with available roles so that the manager can see 'em
        ViewBag.AllRoles = _roleManager.Roles?.ToList();

        return View(model);
    }
}
