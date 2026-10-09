using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Hackaton.Application.Exceptions;

namespace HackatonApi.Middlewares
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var problem = exception switch
            {
                EntityValidationException ev => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validação falhou.",
                    Detail = string.Join(" | ", ev.Errors),
                    Type = "https://httpstatuses.io/400"
                },
                EntityNotFoundException en => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Recurso não encontrado.",
                    Detail = en.Message,
                    Type = "https://httpstatuses.io/404"
                },
                BadCredentialsException bc => new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Credenciais inválidas.",
                    Detail = bc.Message,
                    Type = "https://httpstatuses.io/401"
                },
                _ => BuildUnexpectedProblem(exception)
            };

            if (problem.Status == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Exceção não tratada: {Message}", exception.Message);

            problem.Instance = httpContext.Request.Path;
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = problem.Status!.Value;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }

        private ProblemDetails BuildUnexpectedProblem(Exception exception)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Ocorreu um erro inesperado.",
                Detail = _environment.IsDevelopment() ? BuildDetail(exception) : "Ocorreu um erro inesperado. Consulte o identificador da requisição." ,
                Type = "https://httpstatuses.io/500"
            };

            if (_environment.IsDevelopment())
            {
                problem.Extensions["exceptionType"] = exception.GetType().FullName;
                problem.Extensions["stackTrace"] = exception.StackTrace;
                if (exception.InnerException is not null)
                {
                    problem.Extensions["innerExceptionType"] = exception.InnerException.GetType().FullName;
                    problem.Extensions["innerMessage"] = exception.InnerException.Message;
                }
            }

            return problem;
        }

        private static string BuildDetail(Exception exception)
        {
            var messages = new List<string> { exception.Message };
            var inner = exception.InnerException;
            while (inner is not null)
            {
                messages.Add(inner.Message);
                inner = inner.InnerException;
            }
            return string.Join(" -> ", messages);
        }
    }
}
