namespace Catalog.Infrastructure.HealthChecks
{
    public static class HealthCheckResponseWriter
    {
        public static Task WriteResponse(
            HttpContext context,
            HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),

                totalDuration =
                    $"{report.TotalDuration.TotalMilliseconds:0.##} ms",

                checks = report.Entries.ToDictionary(
                    entry => entry.Key,
                    entry => new
                    {
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        duration =
                            $"{entry.Value.Duration.TotalMilliseconds:0.##} ms"
                    })
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
