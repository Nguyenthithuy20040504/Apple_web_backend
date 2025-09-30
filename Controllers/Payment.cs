using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using TaoOneBE.Models;
using DapperParameters;
using Newtonsoft.Json;

namespace TaoOneBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Payment : ControllerBase
    {
        private readonly string _connectionString;

        public Payment(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultSQLConnection");
        }

        [Authorize]
        [HttpGet("GetPaymentList")]
        public async Task<BaseResponse<BaseResponseList<PaymentModel>>> GetPaymentList(string? filter, int? status, int offSet, int pageSize)
        {
            const string storedProcedure = "sp_payment_list_get";
            BaseResponse<BaseResponseList<PaymentModel>> dt = new BaseResponse<BaseResponseList<PaymentModel>>();
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
                    int records = await result.ReadFirstAsync<int>();
                    List<PaymentModel> payments = (await result.ReadAsync<PaymentModel>()).ToList();
                    List<PaymentProductModel> paymentProduct = (await result.ReadAsync<PaymentProductModel>()).ToList();

                    foreach (var payment in payments)
                    {
                        payment.products = paymentProduct.Where(t => t.payment_id == payment.id).ToList();
                    }

                    dt.Data = new BaseResponseList<PaymentModel>();
                    dt.Data.RecordsTotal = records;
                    dt.Data.Data = payments;

                    return dt;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex?.ToString());
            }
            return dt;
        }

        [Authorize]
        [HttpPost("UpdatePaymentStatus")]
        public ActionResult<BaseResponse<Guid>> UpdatePaymentStatus([FromBody] PaymentStatusModel paymentStt)
        {
            const string storedProcedure = "sp_payment_update_stt";
            Guid responseId = paymentStt.id;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var param = new DynamicParameters();

                    param.Add("@id", paymentStt.id);
                    param.Add("@status", paymentStt.status);

                    responseId = param.Get<Guid>("@id");

                    var result = connection.QueryMultiple(storedProcedure, param, commandType: CommandType.StoredProcedure);

                    connection.Close();
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            return new BaseResponse<Guid>
            {
                Data = responseId
            };
        }

        [HttpPost("PostPayment")]
        public ActionResult<BaseResponse<Guid?>> PostPayment([FromBody] PaymentModel payment)
        {
            const string spPayment = "sp_payment_post";
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Insert vào bảng payment
                    var param = new DynamicParameters();

                    param.Add("@id", payment.id);
                    param.Add("@payment_method", payment.payment_method);
                    param.Add("@note", payment.note);
                    param.Add("@status", payment.status);

                    param.Add("@name", payment.name);
                    param.Add("@phone", payment.phone);
                    param.Add("@email", payment.email);
                    param.Add("@tp", payment.tp);
                    param.Add("@qh", payment.qh);
                    param.Add("@px", payment.px);
                    param.Add("@address", payment.address);
                    param.Add("@total_bill", payment.total_bill);

                    param.AddTable("@products", "paymentDetailType", payment.products);

                    var result = connection.QueryMultiple(spPayment, param, commandType: CommandType.StoredProcedure);
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return new BaseResponse<Guid?>
            {
                Data = payment.id
            };
        }

        [Authorize]
        [HttpDelete("DeletePayment")]
        public ActionResult<BaseResponse<Guid>> DeletePayment([Required] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("ID is required.");
            }
            const string storedProcedure = "sp_payment_delete";
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
