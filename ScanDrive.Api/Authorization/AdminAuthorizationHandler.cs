using Microsoft.AspNetCore.Authorization;
using ScanDrive.Domain.Settings;

namespace ScanDrive.Api.Authorization;

public class AdminAuthorizationHandler : AuthorizationHandler<IAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        // Se o usuário tem permissão de admin, automaticamente satisfaz qualquer requisito
        if (context.User.HasClaim("Permission", $"{Claims.Modules.Administration}:{Claims.Permissions.View}") ||
            context.User.HasClaim("Permission", $"{Claims.Modules.Administration}:{Claims.Permissions.Create}") ||
            context.User.HasClaim("Permission", $"{Claims.Modules.Administration}:{Claims.Permissions.Edit}") ||
            context.User.HasClaim("Permission", $"{Claims.Modules.Administration}:{Claims.Permissions.Delete}"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
} 