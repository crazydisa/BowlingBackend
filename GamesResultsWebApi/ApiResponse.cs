using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace BowlingStatistic.Api.Controllers
{
    public class ApiResponse
    {
        public bool Ok { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public PaginationInfo? Pagination { get; set; }

        public static ApiResponse CreateSuccess(string message = "Успешно", object? data = null)
        {
            return new ApiResponse
            {
                Ok = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse CreateError(string message)
        {
            return new ApiResponse
            {
                Ok = false,
                Message = message
            };
        }
        public static  ApiResponse<T> Error(string message)
        {
            return new ApiResponse<T>
            {
                Ok = false,
                Message = message
            };
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public new T? Data { get; set; }

        public static ApiResponse<T> Success(T data, string message = "Успешно")
        {
            return new ApiResponse<T>
            {
                Ok = true,
                Message = message,
                Data = data
            };
        }

        public static new ApiResponse<T> Error(string message)
        {
            return new ApiResponse<T>
            {
                Ok = false,
                Message = message
            };
        }
    }

    public class PaginationInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}