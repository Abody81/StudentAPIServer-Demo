using Microsoft.AspNetCore.Mvc;
using StudentApi.DataSimulation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using StudentShared;
using StudentShared.Requests;
using StudentApi.Backend.Models;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
namespace StudentBackend.API.Controllers
{
    [Authorize]
    [ApiController]
    [EnableRateLimiting("ApiLimiter")]
    [Route("api/Students")]
    public class StudentsController(
        IAuthorizationService authorizationService,
        ILogger<StudentsController> logger) : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("HeavyLimiter")]
        [HttpGet("All", Name = "GetAllStudents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StudentDto>> GetAllStudents()
        {
            //StudentDataSimulation.StudentsList.Clear();

            if (StudentDataSimulation.StudentsList.Count == 0)
            {
                return NotFound("No Students Found!");
            }
            return Ok(StudentDataSimulation.StudentsList.Select(ToDto).ToList());
        }

        [Authorize(Roles = "Student")]
        [HttpGet("Passed", Name = "GetPassedStudents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StudentDto>> GetPassedStudents()
        {
            var passedStudents = StudentDataSimulation.StudentsList.Where(student => student.Grade >= 50).ToList();
            //passedStudents.Clear();

            if (passedStudents.Count == 0)
            {
                return NotFound("No Students Passed");
            }


            return Ok(passedStudents.Select(ToDto).ToList());
        }


        [AllowAnonymous]
        [HttpGet("AverageGrade", Name = "GetAverageGrade")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double> GetAverageGrade()
        {
            if (StudentDataSimulation.StudentsList.Count == 0)
            {
                return NotFound("No students found.");
            }

            var averageGrade = StudentDataSimulation.StudentsList.Average(student => student.Grade);
            return Ok(averageGrade);
        }


        [HttpGet("{id}", Name = "GetStudentById")]
        [Authorize(Roles = "Student,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentDto>> GetStudentById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            AuthorizationResult authResult = await authorizationService.AuthorizeAsync(
            User,
            id,
            "StudentOwnerOrAdmin");

            if (!authResult.Succeeded)
            {
                // Authorization violation: a non-owner student trying to read
                // another user's record is a security signal worth an audit line.
                logger.LogWarning("Access denied: user {Actor} tried to read student {StudentId}", Actor, id);
                return Forbid();
            }

            var student = StudentDataSimulation.StudentsList.FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            return Ok(ToDto(student));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost(Name = "AddStudent")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<StudentDto> AddStudent(CreateStudentRequest request)
        {
            //we validate the data here
            if (request == null || string.IsNullOrEmpty(request.Name) || request.Age < 0 || request.Grade < 0)
            {
                logger.LogWarning("Student create REJECTED: invalid data by user {Actor}", Actor);
                return BadRequest("Invalid student data.");
            }

            var newStudent = new Student
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Age = request.Age,
                Grade = request.Grade,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role
            };

            StudentDataSimulation.StudentsList.Add(newStudent);

            logger.LogInformation("Student created: {StudentId} {Name} by user {Actor}",
                newStudent.Id, newStudent.Name, Actor);

            //we dont return Ok here,we return createdAtRoute: this will be status code 201 created.
            return CreatedAtRoute("GetStudentById", new { id = newStudent.Id }, ToDto(newStudent));

        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}", Name = "DeleteStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteStudent(Guid id)
        {
            if (id == Guid.Empty)
            {
                logger.LogWarning("Student delete REJECTED: invalid id {StudentId} by user {Actor}", id, Actor);
                return BadRequest($"Not accepted ID {id}");
            }

            var student = StudentDataSimulation.StudentsList.FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                // Deleting/updating a non-existent id can be probing.
                logger.LogWarning("Student delete REJECTED: id {StudentId} not found - user {Actor}", id, Actor);
                return NotFound($"Student with ID {id} not found.");
            }

            StudentDataSimulation.StudentsList.Remove(student);

            logger.LogInformation("Student deleted: {StudentId} {Name} by user {Actor}",
                id, student.Name, Actor);

            

            return Ok($"Student with ID {id} has been deleted.");

            
        }

        //here we use http put method for update
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}", Name = "UpdateStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<StudentDto> UpdateStudent(Guid id, UpdateStudentRequest request)
        {
            if (id == Guid.Empty || request == null || string.IsNullOrEmpty(request.Name) || request.Age < 0 || request.Grade < 0)
            {
                logger.LogWarning("Student update REJECTED: invalid data (id {StudentId}) by user {Actor}", id, Actor);
                return BadRequest("Invalid student data.");
            }

            var student = StudentDataSimulation.StudentsList.FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                logger.LogWarning("Student update REJECTED: id {StudentId} not found - user {Actor}", id, Actor);
                return NotFound($"Student with ID {id} not found.");
            }

            // Capture the old values before overwriting - the audit log
            // should show WHAT changed, not just that something changed.
            var (oldName, oldAge, oldGrade) = (student.Name, student.Age, student.Grade);

            student.Name = request.Name;
            student.Age = request.Age;
            student.Grade = request.Grade;

            logger.LogInformation(
                "Student updated: {StudentId} by user {Actor} - name: {OldName} -> {NewName}, age: {OldAge} -> {NewAge}, grade: {OldGrade} -> {NewGrade}",
                id, Actor, oldName, student.Name, oldAge, student.Age, oldGrade, student.Grade);

            return Ok(ToDto(student));
        }

        private static StudentDto ToDto(Student s) => new()
        {
            Id = s.Id,
            Name = s.Name,
            Age = s.Age,
            Grade = s.Grade,
            Email = s.Email,
            Role = s.Role
        };

        // Actor identity for the audit trail, taken from the authenticated
        // user's claims - the same claims the rate limiter partitions by.
        private string Actor =>
            $"{User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous"} ({User.FindFirstValue(ClaimTypes.Role) ?? "no-role"})";

    }
}
