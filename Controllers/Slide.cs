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
    public class Slide : ControllerBase
    {
        private readonly string _connectionString;

        public Slide(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultSQLConnection");
        }

        [HttpGet("GetSlideList")]
        public BaseResponse<BaseResponseList<ImageModel>> GetSlideList(string? screen, string? rules)
        {
            const string storedProcedure = "sp_slide_list_get";
            BaseResponse<BaseResponseList<ImageModel>> dt = new BaseResponse<BaseResponseList<ImageModel>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@screen", screen);
                    param.Add("@rules", rules);
                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);
                    var data = result.Read<ImageModel>().ToList();
                    var allData = new BaseResponseList<ImageModel>(data);

                    dt = new BaseResponse<BaseResponseList<ImageModel>>
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

        [HttpGet("GetSlideDetail")]
        public BaseResponse<ImageModel> GetSlideDetail([Required] Guid id)
        {
            const string storedProcedure = "sp_slide_detail";
            BaseResponse<ImageModel> dt = new BaseResponse<ImageModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@id", id);
                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);
                    var data = result.Read<ImageModel>().SingleOrDefault();

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
        [HttpPost("PostSlide")]
        public ActionResult<BaseResponse<Guid>> PostSlide([FromBody] ImageModel imageModel)
        {
            if (imageModel == null)
            {
                return BadRequest("Error");
            }
            const string storedProcedure = "sp_slide_post";

            Guid responseId = Guid.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var param = new DynamicParameters();
                    param.Add("@id", imageModel.id != null ? imageModel.id : Guid.NewGuid());
                    param.Add("@img", imageModel.img);
                    param.Add("@name", imageModel.name);
                    param.Add("@screen", imageModel.screen);
                    param.Add("@status", imageModel.status);

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
        [HttpDelete("DeteleSlide")]
        public ActionResult<BaseResponse<Guid>> DeteleSlide([Required] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("ID is required.");
            }
            const string storedProcedure = "sp_slide_delete";
            Guid responseId = Guid.Empty;
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
