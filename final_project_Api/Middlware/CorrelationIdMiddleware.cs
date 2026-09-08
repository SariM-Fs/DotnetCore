using NLog;
namespace final_project_API.Middlware
{
    public class CorrelationIdMiddleware
    {
        private const string HeaderName = "X-Correlation-Id";
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing)
                ? existing.ToString()
                : Guid.NewGuid().ToString();

            context.Response.Headers[HeaderName] = correlationId;

            // Deliberately NOT wrapped in "using": this middleware is registered
            // INSIDE ExceptionHandlingMiddleware, so disposing this scope here would
            // pop the correlation id while the stack unwinds through this method,
            // before the outer exception handler gets to log the error. Leaving it
            // un-disposed is harmless - it lives only for this request's async flow.
            NLog.ScopeContext.PushProperty("CorrelationId", correlationId);

            _logger.LogInformation("Incoming request {Method} {Path}", context.Request.Method, context.Request.Path);

            await _next(context);
        }
    }
}
