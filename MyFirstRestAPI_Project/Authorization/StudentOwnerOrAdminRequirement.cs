using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MyFirstRestAPI_Project.Authorization;

public class StudentOwnerOrAdminRequirement : IAuthorizationRequirement
{
    
}
