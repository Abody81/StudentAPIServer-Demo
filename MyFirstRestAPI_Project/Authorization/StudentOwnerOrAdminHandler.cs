using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MyFirstRestAPI_Project.Authorization;

public class StudentOwnerOrAdminHandler : AuthorizationHandler<StudentOwnerOrAdminRequirement, int>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, StudentOwnerOrAdminRequirement requirement, int studentId)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(userId, out int authenticatedStudentId) && studentId == authenticatedStudentId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
