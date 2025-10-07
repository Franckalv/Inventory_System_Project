namespace ProyectoCatedraDES.Models;

public class UserListItemVM
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public IList<string> Roles { get; set; } = new List<string>();
}

public class RoleCheckVM
{
    public string Name { get; set; } = "";
    public bool Selected { get; set; }
}

public class EditUserRolesVM
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public List<RoleCheckVM> Roles { get; set; } = new();
}
