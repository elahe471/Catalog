

namespace Catalog.Infrastructure.Errors;

public static class ApiErrors
{
    public static ProblemHttpResult Problem(int statusCode, MessageCode code, params object[] arguments)
        => TypedResults.Problem(
            statusCode: statusCode,
            title: ReasonPhrases.GetReasonPhrase(statusCode),
            detail: MessageTranslator.Translate(code, arguments),
            extensions: new Dictionary<string, object?> { ["code"] = code.ToString() });
}
