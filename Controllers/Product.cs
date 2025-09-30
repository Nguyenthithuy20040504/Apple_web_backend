using Dapper;
using DapperParameters;
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
    public class Product : ControllerBase
    {
        private readonly string _connectionString;

        public Product(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultSQLConnection");
        }

        [HttpGet("GetProductList")]
        public async Task<BaseResponse<BaseResponseList<ProductModel>>> GetProductList(string? category_code, string? category_detail_id, string? filter, int offSet, int pageSize, string? sort, int status)
        {
            const string storedProcedure = "sp_product_page_get";
            BaseResponse<BaseResponseList<ProductModel>> dt = new BaseResponse<BaseResponseList<ProductModel>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@code", category_code); // code la watch, ipad, macbook để vào màn hình tất cả sản phẩm của loại
                    param.Add("@category_detail", category_detail_id); //cái này để lọc ở bộ lọc các cate_detail trong 1 loại
                    param.Add("@filter", filter);
                    param.Add("@offSet", offSet);
                    param.Add("@pageSize", pageSize);
                    param.Add("@sort", sort);
                    param.Add("@status", status);
                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);
                    var data = await result.ReadFirstOrDefaultAsync<BaseResponseList<ProductModel>>();
                    data.Data = result.Read<ProductModel>().ToList();

                    dt = new BaseResponse<BaseResponseList<ProductModel>>
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

        [HttpGet("GetRelateProduct")]
        public async Task<BaseResponse<List<ProductModel>>> GetRelateProduct([Required] string? productId)
        {
            const string storedProcedure = "sp_product_relate";
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    var param = new DynamicParameters();
                    param.Add("@productId", productId);

                    var data = (await connection.QueryAsync<ProductModel>(storedProcedure, param, commandType: CommandType.StoredProcedure)).ToList();

                    return new BaseResponse<List<ProductModel>>
                    {
                        Data = data
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new BaseResponse<List<ProductModel>> { Data = new List<ProductModel>() };
            }
        }

        [HttpGet("GetProductDetail")]
        public BaseResponse<ProductModel> GetProductDetail([Required] Guid id)
        {
            const string storedProcedure = "sp_product_detail";
            BaseResponse<ProductModel> dt = new BaseResponse<ProductModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@id", id);
                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);
                    var data = result.Read<ProductModel>().SingleOrDefault();
                    var listImages = result.Read<SubImageModel>().ToList();
                    data.listImages = listImages;
                    dt.Data = data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex?.ToString());
            }
            return dt;
        }

        //them-sua product, them cot category_detail_name vao bang product de tien lm breadcrumb va dieu huong ra category neu kh an vao category_detail tren breadcrumb
        [Authorize]
        [HttpPost("PostProduct")]
        public ActionResult<BaseResponse<Guid?>> PostProduct([FromBody] ProductModel product)
        {
            const string storedProcedure = "sp_product_post";
            Guid? responseId = Guid.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var param = new DynamicParameters();
                    Guid? ProductId = product.id != null ? product.id : Guid.NewGuid();
                    param.Add("@id", ProductId);
                    param.Add("@category_id", product.category_id);
                    param.Add("@category_detail_id", product.category_detail_id);
                    param.Add("@img", product.img);
                    param.Add("@name", product.name);
                    param.Add("@price", product.price);
                    param.Add("@salePrice", product.salePrice);
                    param.Add("@description", product.description);
                    param.Add("@specs", product.specs);
                    param.Add("@status", product.status);
                    param.Add("@size", product.size);
                    param.AddTable("@listImages", "subImageListType", product.listImages);

                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);

                    responseId = ProductId;
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return new BaseResponse<Guid?>
            {
                Data = responseId
            };
        }

        [Authorize]
        [HttpDelete("DeleteProduct")]
        public ActionResult<BaseResponse<Guid>> DeleteProduct([Required] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("ID is required.");
            }
            const string storedProcedure = "sp_product_delete";
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
