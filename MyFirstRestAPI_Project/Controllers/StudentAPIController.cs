using Microsoft.AspNetCore.Mvc;
using StudentApi.DataSimulation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using StudentApi.Shared;
using StudentApi.Shared.Requests;
using MyFirstRestAPI_Project.Models;
namespace StudentApi.Controllers
{
    [Authorize]
    [ApiController]
    [EnableRateLimiting("ApiLimiter")]
    [Route("api/Students")]
    public class StudentsController(IAuthorizationService authorizationService) : ControllerBase
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

            //   StudentDataSimulation.StudentsList.Clear();

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
        public async Task<ActionResult<StudentDto>> GetStudentById(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            AuthorizationResult authResult = await authorizationService.AuthorizeAsync(
            User,
            id,
            "StudentOwnerOrAdmin");

            if (!authResult.Succeeded) return Forbid();

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
                return BadRequest("Invalid student data.");
            }

            var newStudent = new Student
            {
                Id = StudentDataSimulation.StudentsList.Count > 0 ? StudentDataSimulation.StudentsList.Max(s => s.Id) + 1 : 1,
                Name = request.Name,
                Age = request.Age,
                Grade = request.Grade,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role
            };

            StudentDataSimulation.StudentsList.Add(newStudent);

            //we dont return Ok here,we return createdAtRoute: this will be status code 201 created.
            return CreatedAtRoute("GetStudentById", new { id = newStudent.Id }, ToDto(newStudent));

        }

        [Authorize(Roles = "Student")]
        [HttpDelete("{id}", Name = "DeleteStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteStudent(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            var student = StudentDataSimulation.StudentsList.FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            StudentDataSimulation.StudentsList.Remove(student);
            return Ok($"Student with ID {id} has been deleted.");
        }

        //here we use http put method for update
        [Authorize(Roles = "Student")]
        [HttpPut("{id}", Name = "UpdateStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<StudentDto> UpdateStudent(int id, Student updatedStudent)
        {
            if (id < 1 || updatedStudent == null || string.IsNullOrEmpty(updatedStudent.Name) || updatedStudent.Age < 0 || updatedStudent.Grade < 0)
            {
                return BadRequest("Invalid student data.");
            }

            var student = StudentDataSimulation.StudentsList.FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            student.Name = updatedStudent.Name;
            student.Age = updatedStudent.Age;
            student.Grade = updatedStudent.Grade;

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

    }
}
