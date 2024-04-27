using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentRestAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace StudentRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]

    public class StudentsController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IConfiguration _config;

        public StudentsController(IStudentRepository studentRepository, IConfiguration config, ILogger<StudentsController> logger)
        {
            _studentRepository = studentRepository;
            _config = config;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginModel login)
        {
            try
            {
                if (string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
                {
                    _logger.LogWarning("Login failed: Username or password is empty.");
                    return BadRequest("Username and password are required.");
                }

                _logger.LogInformation("Attempting to log in user: {Username}", login.Username);

                if (IsValidUser(login.Username, login.Password))
                {
                    var tokenString = GenerateJWT(login.Username);
                    _logger.LogInformation("User logged in successfully: {Username}", login.Username);
                    return Ok(new { token = tokenString });
                }

                _logger.LogWarning("Login failed: Invalid username or password.");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login.");
                // Return specific error message and appropriate HTTP status code
                return BadRequest("Invalid login credentials");
            }
        }

        [HttpGet("callback")]
        [AllowAnonymous] // Cho phép yêu cầu không cần xác thực
        public async Task<IActionResult> Callback(string code)
        {
            try
            {
                var accessToken = await GetAccessToken(code);
                var userInfo = await GetUserInfo(accessToken);
                var tokenString = GenerateJWT(userInfo);
                return Ok(new { token = tokenString });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OAuth callback.");
                return BadRequest("Error during OAuth callback.");
            }
        }
        public class UserInfo
        {
            public string Username { get; set; }
        }

        private async Task<string> GetAccessToken(string code)
        {
            using var httpClient = new HttpClient();
            var tokenRequest = new Dictionary<string, string>
    {
        { "grant_type", "authorization_code" },
        { "code", code },
        { "client_id", "YOUR_CLIENT_ID" },
        { "client_secret", "YOUR_CLIENT_SECRET" }
    };

            var tokenEndpoint = "URL_OF_TOKEN_ENDPOINT"; // Thay thế bằng URL thực sự của điểm cuối token
            var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(tokenRequest));
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

            return tokenResponse["access_token"];
        }
        private async Task<UserInfo> GetUserInfo(string accessToken)
        {
            var userInfoEndpoint = "URL_OF_USER_INFO_ENDPOINT"; // Thay thế bằng URL thực sự của điểm cuối thông tin người dùng

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync(userInfoEndpoint);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserInfo>(responseContent);
        }
        private string GenerateJWT(UserInfo userInfo)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Secret"]);

            var claims = new List<Claim>
    {
        new(ClaimTypes.Name, userInfo.Username)
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private bool IsValidUser(string username, string password)
        {
            // Logic to validate username and password, typically querying a database
            // Return true if valid, false otherwise
            return (username == "example" && password == "password");
        }

        private string GenerateJWT(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, username)
                    // You can add more claims here based on user roles or other information
                }),
                Expires = DateTime.UtcNow.AddDays(1), // Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private readonly IStudentRepository studentRepository;
        private readonly ILogger<StudentsController> _logger;


        [HttpGet("{search}")]
        public async Task<ActionResult<IEnumerable<Student>>> Search(string name, Gender? gender)
        {
            try
            {
                var result = await studentRepository.Search(name, gender);
                if (result.Any())
                {
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from the database");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Student>>> GetAllStudents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "",
            [FromQuery] string filterByGender = "",
            [FromQuery] int? minAge = null,
            [FromQuery] int? maxAge = null,
            [FromQuery] string searchName = "")
        {
            try
            {
                var students = await _studentRepository.GetStudents();

                // Lọc dữ liệu theo giới tính
                if (!string.IsNullOrEmpty(filterByGender))
                {
                    students = students.Where(s => s.Gender.ToString().Equals(filterByGender, StringComparison.OrdinalIgnoreCase));
                }

                // Lọc dữ liệu theo tuổi
                if (minAge.HasValue)
                {
                    students = students.Where(s => CalculateAge(s.DateOfBirth) >= minAge);
                }

                if (maxAge.HasValue)
                {
                    students = students.Where(s => CalculateAge(s.DateOfBirth) <= maxAge);
                }

                // Tìm kiếm theo tên
                if (!string.IsNullOrEmpty(searchName))
                {
                    students = students.Where(s => s.FirstName.Contains(searchName) || s.LastName.Contains(searchName));
                }

                // Sắp xếp dữ liệu
                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "firstname":
                            students = students.OrderBy(s => s.FirstName);
                            break;
                        case "lastname":
                            students = students.OrderBy(s => s.LastName);
                            break;
                        case "email":
                            students = students.OrderBy(s => s.Email);
                            break;
                        case "gender":
                            students = students.OrderBy(s => s.Gender);
                            break;
                        case "departmentid":
                            students = students.OrderBy(s => s.DepartmenId);
                            break;
                        case "photopath":
                            students = students.OrderBy(s => s.PhotoPath);
                            break;
                        case "dateofbirth":
                            students = students.OrderBy(s => s.DateOfBirth);
                            break;
                        default:
                            break;
                    }
                }
                // Phân trang
                var paginatedStudents = students.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                return Ok(paginatedStudents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from the database");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            try
            {
                var student = await _studentRepository.GetStudent(id);
                if (student == null)
                {
                    return NotFound();
                }
                return student;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from the database");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age))
                age--;
            return age;
        }

        private async Task<ActionResult> ValidateStudent(Student student)
        {
            // Kiểm tra tính hợp lệ của email
            if (!IsValidEmail(student.Email))
            {
                ModelState.AddModelError("Email", "Invalid email format");
                return BadRequest(ModelState);
            }
            // Kiểm tra tuổi của sinh viên
            if (CalculateAge(student.DateOfBirth) < 18)
            {
                ModelState.AddModelError("DateOfBirth", "Student must be at least 18 years old");
                return BadRequest(ModelState);
            }
            // Kiểm tra email đã tồn tại hay chưa
            var existingStudent = await studentRepository.GetStudentByEmail(student.Email);
            if (existingStudent != null)
            {
                ModelState.AddModelError("Email", "Student email already in use");
                return BadRequest(ModelState);
            }
            return null;
        }

        [HttpPost]
        
        // Chỉ cho phép người dùng có quyền hạn "Admin" thực hiện hành động này
        public async Task<ActionResult<Student>> CreateStudent(Student student)
        {
            try
            {
                if (student == null)
                    return BadRequest();
                var validationError = await ValidateStudent(student);
                if (validationError != null)
                    return validationError;
                // Thêm sinh viên mới
                var createdStudent = await studentRepository.AddStudent(student);
                return CreatedAtAction(nameof(GetStudent), new { id = createdStudent.StudentId }, createdStudent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new student record");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new student record");
            }
        }

        [HttpPut("{id:int}")]
        //[Authorize(Roles = "Admin")] // Chỉ cho phép người dùng có quyền hạn "Admin" thực hiện hành động này
        public async Task<ActionResult<Student>> UpdateStudent(int id, Student student)
        {
            try
            {
                if (id != student.StudentId)
                {
                    return BadRequest("Student ID mismatch");
                }
                // Kiểm tra tồn tại của sinh viên cần cập nhật
                var studentToUpdate = await studentRepository.GetStudent(id);
                if (studentToUpdate == null)
                {
                    return NotFound($"Student with Id = {id} not found");
                }
                // Kiểm tra tính hợp lệ của email
                if (!IsValidEmail(student.Email))
                {
                    ModelState.AddModelError("Email", "Invalid email format");
                    return BadRequest(ModelState);
                }
                // Kiểm tra email đã tồn tại hay chưa (trừ sinh viên cần cập nhật)
                var existingStudent = await studentRepository.GetStudentByEmail(student.Email);
                if (existingStudent != null && existingStudent.StudentId != id)
                {
                    ModelState.AddModelError("Email", "Student email already in use");
                    return BadRequest(ModelState);
                }
                // Cập nhật thông tin sinh viên
                return await studentRepository.UpdateStudent(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student record");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating student record");
            }
        }

        [HttpDelete("{id:int}")]
        //[Authorize(Roles = "Admin")] // Chỉ cho phép người dùng có quyền hạn "Admin" thực hiện hành động này
        public async Task<ActionResult> DeleteStudent(int id)
        {
            try
            {
                var studentToDelete = await studentRepository.GetStudent(id);
                if (studentToDelete == null)
                {
                    return NotFound($"Student with Id = {id} not found");
                }
                await studentRepository.DeleteStudent(id);
                return Ok($"Student with Id = {id} deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Student record");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting Student record");
            }
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
