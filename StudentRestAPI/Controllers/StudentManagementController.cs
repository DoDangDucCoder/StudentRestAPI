using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudentRestAPI.Models;

namespace StudentRestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentManagementController : ControllerBase
    {
        private readonly ILogger<StudentManagementController> _logger;
        private readonly IStudentRepository _studentRepository;

        public StudentManagementController(ILogger<StudentManagementController> logger, IStudentRepository studentRepository)
        {
            _logger = logger;
            _studentRepository = studentRepository;
        }

        [HttpGet("allstudents")]
        public async Task<ActionResult<IEnumerable<Student>>> GetAllStudents()
        {
            try
            {
                var students = await _studentRepository.GetStudents();
                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from the database");
                return StatusCode(500, "Error retrieving data from the database");
            }
        }

        [HttpGet("student/{id:int}")]
        public async Task<ActionResult<Student>> GetStudentById(int id)
        {
            try
            {
                var student = await _studentRepository.GetStudent(id);
                if (student == null)
                {
                    return NotFound($"Student with ID {id} not found");
                }
                return Ok(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from the database");
                return StatusCode(500, "Error retrieving data from the database");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Student>>> SearchStudents(
            [FromQuery] string name,
            [FromQuery] Gender? gender)
        {
            try
            {
                var result = await _studentRepository.Search(name, gender);
                if (result.Any())
                {
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from the database");
                return StatusCode(500, "Error retrieving data from the database");
            }
        }

        // Additional endpoints for CRUD operations on students can be added here
    }
}
