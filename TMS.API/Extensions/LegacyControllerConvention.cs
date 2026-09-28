using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace TMS.API.Extensions;

/// <summary>The original generic CRUD controllers stay, but only role/user administrators may call them.</summary>
public class LegacyControllerConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var ns = controller.ControllerType.Namespace ?? string.Empty;
        if (ns.EndsWith(".Workflow") || controller.ControllerName == "Auth")
        {
            return;
        }

        controller.Filters.Add(new AuthorizeFilter("perm:manage_roles_users"));
    }
}
