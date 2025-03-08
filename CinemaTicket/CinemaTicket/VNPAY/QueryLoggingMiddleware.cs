namespace CinemaTicket.VNPAY
{
    public class QueryLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public QueryLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            foreach (var queryParam in context.Request.Query)
            {
                Console.WriteLine($"{queryParam.Key}: {queryParam.Value}");
            }

            await _next(context);
        }
    }
}
