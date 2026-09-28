namespace TMS.DataAccessLayer.Entities;

public partial class Menu
{
    public int MenuId { get; set; }

    public string MenuCode { get; set; } = null!;

    public string MenuName { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public virtual ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
}
