using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

public static class RoleAddingMiddleware
{
    public static HtmlString ScriptNonce(this IHtmlHelper helper)
    {
        var httpContext = helper.ViewContext.HttpContext;
        var nonce = httpContext.Items["ScriptNonce"] as string;
        return new HtmlString(nonce ?? string.Empty); // Handle null value by returning an empty string
    }

    public static HtmlString StyleNonce(this IHtmlHelper helper)
    {
        var httpContext = helper.ViewContext.HttpContext;
        var nonce = httpContext.Items["StyleNonce"] as string;
        return new HtmlString(nonce ?? string.Empty); // Handle null value by returning an empty string
    }
}


