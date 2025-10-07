using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoCatedraDES.Models;

namespace ProyectoCatedraDES.Controllers;

[Authorize(Roles = "Admin")]
public class UsuariosController : Controller
{
    private readonly UserManager<IdentityUser> _userMgr;
    private readonly RoleManager<IdentityRole> _roleMgr;

    public UsuariosController(UserManager<IdentityUser> userMgr, RoleManager<IdentityRole> roleMgr)
    {
        _userMgr = userMgr;
        _roleMgr = roleMgr;
    }

    // GET: /Usuarios
    public IActionResult Index()
    {
        var users = _userMgr.Users.ToList();
        var data = new List<UserListItemVM>();

        foreach (var u in users)
        {
            data.Add(new UserListItemVM
            {
                Id = u.Id,
                Email = u.Email ?? u.UserName ?? "(sin email)",
                Roles = _userMgr.GetRolesAsync(u).GetAwaiter().GetResult()
            });
        }

        return View(data);
    }

    // GET: /Usuarios/EditRoles/{id}
    public async Task<IActionResult> EditRoles(string id)
    {
        var user = await _userMgr.FindByIdAsync(id);
        if (user is null) return NotFound();

        // asegúrate de que roles existan (Admin / Operador)
        foreach (var r in new[] { "Admin", "Operador" })
            if (!await _roleMgr.RoleExistsAsync(r))
                await _roleMgr.CreateAsync(new IdentityRole(r));

        var model = new EditUserRolesVM
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? "(sin email)"
        };

        var userRoles = await _userMgr.GetRolesAsync(user);
        foreach (var role in _roleMgr.Roles.Select(r => r.Name!).OrderBy(n => n))
        {
            model.Roles.Add(new RoleCheckVM
            {
                Name = role,
                Selected = userRoles.Contains(role)
            });
        }

        return View(model);
    }

    // POST: /Usuarios/EditRoles
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoles(EditUserRolesVM model)
    {
        var user = await _userMgr.FindByIdAsync(model.UserId);
        if (user is null) return NotFound();

        var currentRoles = await _userMgr.GetRolesAsync(user);
        var selectedRoles = model.Roles.Where(r => r.Selected).Select(r => r.Name).ToArray();

        // Protección: no permitir que el propio admin se quite el rol Admin
        var isSelf = User?.Identity?.Name == user.Email || User?.Identity?.Name == user.UserName;
        if (isSelf && !selectedRoles.Contains("Admin"))
        {
            ModelState.AddModelError("", "No puedes quitarte a ti mismo el rol de Admin.");
            return await EditRoles(model.UserId);
        }

        // Calcular cambios
        var toAdd = selectedRoles.Except(currentRoles).ToArray();
        var toRemove = currentRoles.Except(selectedRoles).ToArray();

        if (toAdd.Length > 0)
            await _userMgr.AddToRolesAsync(user, toAdd);

        if (toRemove.Length > 0)
            await _userMgr.RemoveFromRolesAsync(user, toRemove);

        TempData["Ok"] = "Roles actualizados.";
        return RedirectToAction(nameof(Index));
    }
}
