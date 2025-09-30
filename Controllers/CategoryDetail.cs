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
    public class CategoryDetail : ControllerBase
    {
        private readonly string _connectionString;

        public CategoryDetail(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultSQLConnection");
        }

        [HttpGet("GetCategoryDetailList")]
        public BaseResponse<BaseResponseList<CategoryDetailModel>> GetCategoryDetailList(string? category_code, string? screen)
        {
            const string storedProcedure = "sp_categoryDetail_list_get";
            BaseResponse<BaseResponseList<CategoryDetailModel>> dt = new BaseResponse<BaseResponseList<CategoryDetailModel>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@category_code", category_code);
                    param.Add("@screen", screen);
                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);
                    var data = result.Read<CategoryDetailModel>().ToList();
                    var allData = new BaseResponseList<CategoryDetailModel>(data);

                    dt = new BaseResponse<BaseResponseList<CategoryDetailModel>>
                    {
                        Data = allData
                    };

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex?.ToString());
            }
            return dt;
        }

        [HttpGet("GetCategoryDetailDetail")]
        public BaseResponse<CategoryDetailModel> GetCategoryDetailDetail([Required] Guid id)
        {
            const string storedProcedure = "sp_categoryDetail_detail";
            BaseResponse<CategoryDetailModel> dt = new BaseResponse<CategoryDetailModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@id", id);
                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);
                    var data = result.Read<CategoryDetailModel>().SingleOrDefault();

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
        [HttpPost("PostCategoryDetail")]
        public ActionResult<BaseResponse<Guid>> PostCategoryDetail([FromBody] CategoryDetailModel categoryDetail)
        {
            if (categoryDetail == null)
            {
                return BadRequest("Error");
            }
            const string storedProcedure = "sp_categoryDetail_post";

            Guid responseId = Guid.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var param = new DynamicParameters();
                    param.Add("@id", categoryDetail.id != null ? categoryDetail.id : Guid.NewGuid());
                    param.Add("@name", categoryDetail.name);
                    param.Add("@category_id", categoryDetail.category_id);
                    param.Add("size", categoryDetail.size);

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
        [HttpDelete("DeleteCategoryDetail")]
        public ActionResult<BaseResponse<Guid>> DeleteCategoryDetail([Required] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("ID is required.");
            }
            const string storedProcedure = "sp_categoryDetail_delete";
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
