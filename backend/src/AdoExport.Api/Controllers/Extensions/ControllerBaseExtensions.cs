using System.Net;

using Microsoft.AspNetCore.Mvc;

namespace AdoExport.Api.Controllers.Extensions;

public static class ControllerBaseExtensions
{
    /// <summary>
    /// Provides a quick way to produce a status code response for an expired audit.
    /// </summary>
    /// <param name="controller">The controller to create a status code response for.</param>
    /// <returns>A status code response indicating that an audit expired.</returns>
    public static ObjectResult AuditExpiredStatusCode(this ControllerBase controller)
    {
        Guard.IsNotNull(controller);

        return controller.StatusCode((int)HttpStatusCode.Gone, "Audit not found or expired");
    }
}