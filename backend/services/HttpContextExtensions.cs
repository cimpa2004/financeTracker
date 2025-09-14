using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace backend.services;

public static class HttpContextExtensions
{
    // Try to read a user id GUID from common JWT claim names. Returns true when parsed successfully.
    public static bool TryGetUserId(this HttpContext http, out Guid userId)
    {
        userId = Guid.Empty;
        if (http?.User == null)
            return false;

        var claimValue = http.User.FindFirst("userId")?.Value
                         ?? http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? http.User.FindFirst("sub")?.Value;

        return Guid.TryParse(claimValue, out userId);
    }

    // Convenience: returns nullable Guid
    public static Guid? GetUserId(this HttpContext http)
    {
        return http.TryGetUserId(out var id) ? id : (Guid?)null;
    }
}