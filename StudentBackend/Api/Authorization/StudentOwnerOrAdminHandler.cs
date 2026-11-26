using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace StudentApi.Backend.Authorization;

public class StudentOwnerOrAdminHandler : AuthorizationHandler<StudentOwnerOrAdminRequirement, Guid>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, StudentOwnerOrAdminRequirement requirement, Guid studentId)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userId, out Guid authenticatedStudentId) && studentId == authenticatedStudentId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
