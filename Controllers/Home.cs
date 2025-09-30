using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TaoOneBE.Models;
using System.Data;

namespace TaoOneBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Home : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<Home> _logger;

        public Home(IConfiguration configuration, ILogger<Home> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultSQLConnection");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("GetHome")]
        public BaseResponse<HomeModel> GetHome ()
        {
            const string storedProcedure = "sp_home_get";
            //_logger.LogInformation("Executing stored procedure: {StoredProcedure}", storedProcedure);
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    //_logger.LogError("Database connection opened successfully.");
                    var result = connection.QueryMultiple(storedProcedure, null, commandType: CommandType.StoredProcedure);
                    var categories = result.Read<CategoryModel>().ToList();
                    var products = result.Read<ProductModel>().ToList();
                    foreach(CategoryModel category in categories)
                    {
                        category.products = products.Where(product => product.category_id == category.id).ToList();
                    }
                    var home = new HomeModel();
                    home.categories = categories;
                    var response = new BaseResponse<HomeModel>();
                    response.Data = home;
                    //_logger.LogInformation("Successfully retrieved home data.");
                    return response;
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "An error occurred while retrieving home data.");
                throw ex;
            }
        }
    }
}
