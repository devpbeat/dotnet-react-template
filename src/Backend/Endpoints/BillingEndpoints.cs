using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Endpoints;

public static class BillingEndpoints
{
    public static void MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/billing")
            .WithTags("Billing");

        group.MapPost("/subscribe", async (
            [FromBody] SubscribeRequest request,
            IBancardService bancardService) =>
        {
            var processId = await bancardService.CreateCatastroRequestAsync(request.UserId, request.Plan);
            return Results.Ok(new { process_id = processId });
        })
        .WithName("Subscribe")
        .WithDescription("Create a subscription request");

        group.MapPost("/webhook", async (
            [FromBody] object payload,
            IBancardService bancardService) =>
        {
            var success = await bancardService.ProcessWebhookAsync(payload.ToString() ?? "");

            if (success)
            {
                return Results.Ok(new { status = "success" });
            }

            return Results.BadRequest();
        })
        .WithName("ProcessWebhook")
        .WithDescription("Process payment webhook from Bancard");
    }
}

public class SubscribeRequest
{
    public Guid UserId { get; set; }
    public string Plan { get; set; } = string.Empty;
}
