using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This middleware replaces the default service provider by the one for the current tenant
    /// </summary>
    public class ModularTenantContainerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IShellHost _shellHost;
        private readonly IRunningShellTable _runningShellTable;

        public ModularTenantContainerMiddleware(
            RequestDelegate next,
            IShellHost shellHost,
            IRunningShellTable runningShellTable)
        {
            _next = next;
            _shellHost = shellHost;
            _runningShellTable = runningShellTable;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Ensure all ShellContext are loaded and available.
            await _shellHost.InitializeAsync();

            var shellSettings = _runningShellTable.Match(httpContext);

            // We only serve the next request if the tenant has been resolved.
            if (shellSettings != null)
            {
                if (shellSettings.State == TenantState.Initializing)
                {
                    httpContext.Response.Headers.Add(HeaderNames.RetryAfter, "10");
                    httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    await httpContext.Response.WriteAsync("The requested tenant is currently initializing.");
                    return;
                }

                var shellScope = await _shellHost.GetScopeAsync(shellSettings);

                httpContext

                    // Makes 'RequestServices' aware of the current 'ShellScope'.
                    .UseShellScopeServices()

                    // For logging infos outside any 'ShellScope'.
                    .RegisterShellContext(shellScope.ShellContext);

                await shellScope.UsingAsync(scope => _next.Invoke(httpContext));
            }
        }
    }
}