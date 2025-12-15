using Backend.Application.Commands;
using Backend.Application.Jobs;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Endpoints;

public static class ExampleEndpoints
{
    public static void MapExampleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/examples")
            .WithTags("Examples");

        // Example: Using MediatR
        group.MapPost("/subscriptions/create", async (
            [FromBody] CreateSubscriptionRequest request,
            IMediator mediator) =>
        {
            var command = new CreateSubscriptionCommand(
                request.UserId,
                request.Plan,
                request.PaymentMethodId
            );

            var result = await mediator.Send(command);

            return Results.Ok(new
            {
                subscriptionId = result.SubscriptionId,
                status = result.Status,
                createdAt = result.CreatedAt
            });
        })
        .WithName("CreateSubscriptionExample")
        .WithDescription("Example endpoint using MediatR for CQRS pattern");

        // Example: Fire-and-forget Hangfire job
        group.MapPost("/jobs/subscription-renewal", (
            [FromBody] SubscriptionRenewalRequest request,
            IBackgroundJobClient jobClient) =>
        {
            var jobId = jobClient.Enqueue<SampleBackgroundJob>(
                job => job.ProcessSubscriptionRenewal(request.SubscriptionId));

            return Results.Ok(new { jobId, message = "Subscription renewal job queued" });
        })
        .WithName("QueueSubscriptionRenewal")
        .WithDescription("Example endpoint to queue a fire-and-forget background job");

        // Example: Delayed Hangfire job
        group.MapPost("/jobs/welcome-email", (
            [FromBody] WelcomeEmailRequest request,
            IBackgroundJobClient jobClient) =>
        {
            var jobId = jobClient.Schedule<SampleBackgroundJob>(
                job => job.SendWelcomeEmail(request.UserId, request.Email),
                TimeSpan.FromMinutes(5));

            return Results.Ok(new
            {
                jobId,
                message = "Welcome email scheduled to send in 5 minutes"
            });
        })
        .WithName("ScheduleWelcomeEmail")
        .WithDescription("Example endpoint to schedule a delayed background job");

        // Example: Combining MediatR and Hangfire
        group.MapPost("/subscriptions/create-with-email", async (
            [FromBody] CreateSubscriptionWithEmailRequest request,
            IMediator mediator,
            IBackgroundJobClient jobClient) =>
        {
            // Use MediatR to create subscription
            var command = new CreateSubscriptionCommand(
                request.UserId,
                request.Plan,
                request.PaymentMethodId
            );

            var result = await mediator.Send(command);

            // Queue background job to send welcome email
            var jobId = jobClient.Schedule<SampleBackgroundJob>(
                job => job.SendWelcomeEmail(request.UserId, request.Email),
                TimeSpan.FromMinutes(1));

            return Results.Ok(new
            {
                subscriptionId = result.SubscriptionId,
                status = result.Status,
                createdAt = result.CreatedAt,
                emailJobId = jobId,
                message = "Subscription created and welcome email scheduled"
            });
        })
        .WithName("CreateSubscriptionWithEmail")
        .WithDescription("Example combining MediatR command and Hangfire background job");
    }
}

// Request DTOs
public record CreateSubscriptionRequest(
    Guid UserId,
    string Plan,
    string PaymentMethodId
);

public record SubscriptionRenewalRequest(Guid SubscriptionId);

public record WelcomeEmailRequest(Guid UserId, string Email);

public record CreateSubscriptionWithEmailRequest(
    Guid UserId,
    string Plan,
    string PaymentMethodId,
    string Email
);
