using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Data;
using TaoOneBE.Models;
using Microsoft.AspNetCore.Authorization;


namespace TaoOneBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Category : ControllerBase
    {
        private readonly string _connectionString;

        public Category(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultSQLConnection");
        }

        [HttpGet("GetCategoryList")]
        public BaseResponse<BaseResponseList<CategoryModel>> GetCategoryList(string? filter)
        {
            const string storedProcedure = "sp_category_list_get";
            BaseResponse<BaseResponseList<CategoryModel>> dt = new BaseResponse<BaseResponseList<CategoryModel>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@filter", filter);
                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);
                    var data = result.Read<CategoryModel>().ToList();
                    var allData = new BaseResponseList<CategoryModel>(data);

                    dt. Data = allData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex?.ToString());
            }
            return dt;
        }

        [HttpGet("GetCategoryDetail")]
        public BaseResponse<CategoryModel> GetCategoryDetail([Required] Guid id)
        {
            const string storedProcedure = "sp_category_detail";
            BaseResponse<CategoryModel> dt = new BaseResponse<CategoryModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@id", id);
                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);
                    var data = result.Read<CategoryModel>().SingleOrDefault();

                    dt.Data = data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex?.ToString());
            }
            return dt;
        }

        [Authorize]
        [HttpPost("PostCategory")]
        public ActionResult<BaseResponse<Guid>> PostCategory([FromBody] CategoryModel category)
        {
            if(category == null)
            {
                return BadRequest();
            }
            const string storedProcedure = "sp_category_post";

            Guid responseId = Guid.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var param = new DynamicParameters();
                    param.Add("@id", category.id != null ? category.id : Guid.NewGuid());
                    param.Add("@code", category.code);
                    param.Add("@name", category.name);
                    param.Add("@img", category.img);
                    param.Add("@order", category.order);
                    param.Add("@status", category.status);
                    param.Add("@is_show_home", category.is_show_home);

                    responseId = param.Get<Guid>("@id");

                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return new BaseResponse<Guid>
            {
                Data = responseId
            };

        }

        [Authorize]
        [HttpDelete("DeleteCategory")]
        public ActionResult<BaseResponse<Guid>> DeleteCategory([Required] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("ID is required.");
            }
            const string storedProcedure = "sp_category_delete";
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@id", id);

                    var result = connection.QueryFirstOrDefault<BaseResponse<Guid>>(storedProcedure, param, commandType: CommandType.StoredProcedure);
                    if (result == null)
                    {
                        // Xử lý khi kết quả là null (có thể trả về BadRequest hoặc response khác)
                        return BadRequest("No data deleted or error occurred.");
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
    }
}
