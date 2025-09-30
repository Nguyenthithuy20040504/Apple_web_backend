using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaoOneBE.Models;

namespace TaoOneBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Login : ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly string _jwtSecretKey;
        private readonly int _jwtTokenExpiryInHours;

        public Login(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultSQLConnection");
            _jwtIssuer = configuration["Jwt:Issuer"];
            _jwtAudience = configuration["Jwt:Audience"];
            _jwtSecretKey = configuration["Jwt:SecretKey"];
            _jwtTokenExpiryInHours = int.Parse(configuration["Jwt:TokenExpiryInHours"]);
        }

        [HttpPost("PostLogin")]
        public ActionResult<BaseResponse<LoginResponse>> PostLogin([FromBody] LoginModel login)
        {
            const string storedProcedure = "sp_login_post";
            LoginResponse loginResponse = new LoginResponse();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@username", login.username);
                    param.Add("@password", login.password);

                    var result = connection.QueryFirstOrDefault(storedProcedure, param, commandType: CommandType.StoredProcedure);
                    if (result != null)
                    {
                        if (result.Status == "success")
                        {
                            loginResponse.username = login.username;
                            loginResponse.token = GenerateJwtToken(login.username);
                        }
                    }

                    return new BaseResponse<LoginResponse>
                    {
                        Message = result.Message,
                        Status = result.Status,
                        Data = loginResponse
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }
        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddHours(_jwtTokenExpiryInHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
