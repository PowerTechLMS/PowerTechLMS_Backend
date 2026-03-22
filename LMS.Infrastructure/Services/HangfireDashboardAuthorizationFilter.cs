using Hangfire.Dashboard;

namespace LMS.Infrastructure.Services;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) { return true; }
}
