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
    public class News : ControllerBase
    {
        private readonly string _connectionString;
        public News(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultSQLConnection");
        }
        [HttpGet("GetNewsList")]
        public async Task<BaseResponse<BaseResponseList<NewsModel>>> GetNews(int? status, string? filter, int offSet, int pageSize)
        {
            const string storedProcedure = "sp_news_list_get";
            BaseResponse<BaseResponseList<NewsModel>> dt = new BaseResponse<BaseResponseList<NewsModel>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@filter", filter);
                    param.Add("@status", status);
                    param.Add("@offSet", offSet);
                    param.Add("@pageSize", pageSize);
                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);
                    var data = await result.ReadFirstOrDefaultAsync<BaseResponseList<NewsModel>>();
                    data.Data = result.Read<NewsModel>().ToList();

                    dt = new BaseResponse<BaseResponseList<NewsModel>>
                    {
                        Data = data
                    };
                    return dt;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex?.ToString());
            }
            return dt;
        }

        [HttpGet("GetNewsRelate")]
        public async Task<BaseResponse<BaseResponseList<NewsModel>>> GetNewsRelate(string? currentNews)
        {
            const string storedProcedure = "sp_news_list_relate";
            BaseResponse<BaseResponseList<NewsModel>> dt = new BaseResponse<BaseResponseList<NewsModel>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@currentNews", currentNews);
                    var result = (await connection.QueryAsync<NewsModel>(storedProcedure, param, commandType: CommandType.StoredProcedure)).ToList();

                    dt.Data = new BaseResponseList<NewsModel>
                    {
                        Data = result
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex?.ToString());
            }
            return dt;
        }

        [HttpGet("GetNewsDetail")]
        public BaseResponse<NewsModel> GetCategoryDetail(Guid? id, string? slug)
        {
            const string storedProcedure = "sp_news_detail";
            BaseResponse<NewsModel> dt = new BaseResponse<NewsModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@id", id);
                    param.Add("@slug", slug);
                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);
                    var data = result.Read<NewsModel>().SingleOrDefault();

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
        [HttpPost("PostNews")]
        public ActionResult<BaseResponse<Guid>> PostCategory([FromBody] NewsModel news)
        {
            if (news == null)
            {
                return BadRequest();
            }
            const string storedProcedure = "sp_news_post";

            Guid responseId = Guid.Empty;
            string resultMessage = string.Empty;
            string resultStatus = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var param = new DynamicParameters();
                    param.Add("@id", news.id != null ? news.id : Guid.NewGuid());
                    param.Add("@title", news.title);
                    param.Add("@slug", news.slug);
                    param.Add("@thumbnailUrl", news.thumbnailUrl);
                    param.Add("@coverImageUrl", news.coverImageUrl);
                    param.Add("@excerpt", news.excerpt);
                    param.Add("@contentHtml", news.contentHtml);
                    //param.Add("@publishedAt", news.publishedAt);
                    //param.Add("@updatedAt", news.updatedAt);
                    param.Add("status", news.status);

                    responseId = param.Get<Guid>("@id");

                    var result = connection.QueryFirstOrDefault<dynamic>(storedProcedure, param, commandType: CommandType.StoredProcedure);

                    if (result != null) {
                        resultMessage = result.Message;
                        resultStatus = result.Status;
                    }
                    
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return new BaseResponse<Guid>
            {
                Message = resultMessage,
                Status = resultStatus,
                Data = responseId
            };

        }

        [Authorize]
        [HttpDelete("DeleteNews")]
        public ActionResult<BaseResponse<Guid>> DeleteNews([Required] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("ID is required.");
            }
            const string storedProcedure = "sp_news_delete";
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
