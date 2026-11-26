using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace StudentApi.Backend.Authorization;

public class StudentOwnerOrAdminRequirement : IAuthorizationRequirement
{

}
