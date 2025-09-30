namespace TaoOneBE.Models
{
    public class BaseResponse<T>
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public string? Status { get; set; }
        public T? Data { get; set; }

        public BaseResponse()
        {
            StatusCode = 200;
            Status = "success";
        }
    }
    public class ResponseId
    {
        public Guid? Id { get; set; }

        public ResponseId(Guid id)
        {
            Id = id;
        }
    }

    public class LoginResponse
    {
        public string? username { get; set; }
        public string? token { get; set; }
    }

    public class BaseResponseList<T>
    {
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<T>? Data { get; set; }

        public BaseResponseList()
        {
            
        }

        public BaseResponseList(List<T> data)
        {
            RecordsFiltered = data?.Count ?? 0;
            Data = data;
        }
    }
}
