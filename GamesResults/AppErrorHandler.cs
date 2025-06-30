using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace GamesResults
{
    public class AppErrorHandler
    {
        private readonly RequestDelegate _next;

        public AppErrorHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                string result;
                switch (ex)
                {
                    case KeyNotFoundException e:
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        result = JsonSerializer.Serialize(new
                        {
                            message = e.Message,
                        });
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        result = JsonSerializer.Serialize(new
                        {
                            message = string.Format("{0}\r\n{1}", ex.Message, ex.InnerException?.Message).Trim(),
                        });
                        break;
                }

                await response.WriteAsync(result);
            }
        }
    }
}
