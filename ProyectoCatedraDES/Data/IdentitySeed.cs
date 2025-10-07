using Microsoft.AspNetCore.Identity;

namespace ProyectoCatedraDES.Data
{
    public static class IdentitySeed
    {
        public static async Task EnsureSeedAsync(IServiceProvider sp)
        {
            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = sp.GetRequiredService<UserManager<IdentityUser>>();

            string[] roles = { "Admin", "Operador" };
            foreach (var r in roles)
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            var adminEmail = "admin@ddb.local";
            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                
                await userMgr.CreateAsync(admin, "Admin#12345");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
