using System.Security.Cryptography;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.services;
using Microsoft.AspNetCore.Http;

namespace backend.apis;

public static class LogoutApi
{

    public static void MapLogout(this WebApplication app)
    {
        app.MapPost("/api/logout", async (HttpContext http) =>
        {
            return Results.Ok(new
            {
                message = "Successfully logged out."
            });
        })
        .WithName("LogoutUser");
    }
}

           