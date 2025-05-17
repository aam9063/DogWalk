using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace DogWalk_API.Middleware
{
    public class AntiforgeryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiforgery;

        public AntiforgeryMiddleware(RequestDelegate next, IAntiforgery antiforgery)
        {
            _next = next;
            _antiforgery = antiforgery;
        }

        public async Task Invoke(HttpContext context)
        {
            // Verificar si es una solicitud GET, OPTIONS o HEAD
            if (HttpMethods.IsGet(context.Request.Method) ||
                HttpMethods.IsHead(context.Request.Method) ||
                HttpMethods.IsOptions(context.Request.Method))
            {
                // Para solicitudes GET, generar un token CSRF y enviarlo en una cookie
                var tokens = _antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken,
                    new CookieOptions
                    {
                        HttpOnly = false, // El cliente JS debe poder leerlo
                        Secure = !context.Request.IsHttps ? false : true,
                        SameSite = SameSiteMode.Strict,
                        IsEssential = true
                    });
            }
            else if (HttpMethods.IsPost(context.Request.Method) ||
                     HttpMethods.IsPut(context.Request.Method) ||
                     HttpMethods.IsDelete(context.Request.Method) ||
                     HttpMethods.IsPatch(context.Request.Method))
            {
                // Excluir rutas específicas como el login y el refresh token
                if (!context.Request.Path.StartsWithSegments("/api/Auth/login") &&
                    !context.Request.Path.StartsWithSegments("/api/Auth/refresh-token"))
                {
                    try
                    {
                        // Verificar el token CSRF para solicitudes no seguras (POST/PUT/DELETE/PATCH)
                        await _antiforgery.ValidateRequestAsync(context);
                    }
                    catch (AntiforgeryValidationException)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Token CSRF inválido o ausente.");
                        return;
                    }
                }
            }

            // Continuar con el siguiente middleware
            await _next(context);
        }
    }
}
