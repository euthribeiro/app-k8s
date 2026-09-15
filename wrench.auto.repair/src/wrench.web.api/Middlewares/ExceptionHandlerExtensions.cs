using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using wrench.auto.repair.core.DomainObjects;

namespace wrench.web.api.Middlewares
{
    public static class ExceptionHandlerExtensions
    {
        public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = feature?.Error;

                    if (exception is DomainException)
                    {
                        var problem = CreateProblemDetails(context, 400);
                        await WriteResponse(context, problem);
                        return;
                    }

                    var defaultProblem = CreateProblemDetails(context, 500);
                    await WriteResponse(context, defaultProblem);
                });
            });
        }

        private static ProblemDetails CreateProblemDetails(HttpContext context, int statusCode)
        {
            var problem = new ProblemDetails
            {
                Title = "Erro interno",
                Status = statusCode,
                Detail = "Um erro inesperado ocorreu.",
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;

            return problem;
        }

        private static async Task WriteResponse(HttpContext context, ProblemDetails problem)
        {
            context.Response.StatusCode = problem.Status ?? 500;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
