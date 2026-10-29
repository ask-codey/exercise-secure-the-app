using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace NorthwindApi.Errors;

// Turns a constraint conflict from SQL Server (error 547), such as deleting a product that orders
// still use, into 409 Conflict. It's finished: register it in Program.cs.
public class DatabaseConflictHandler : IExceptionHandler
{
    private readonly IProblemDetailsService problemDetailsService;

    public DatabaseConflictHandler(IProblemDetailsService problemDetailsService)
    {
        this.problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not SqlException sqlException || sqlException.Number != 547)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = "Other data uses this item, or it uses data that doesn't exist, so it can't be saved or deleted."
            }
        });
    }
}
