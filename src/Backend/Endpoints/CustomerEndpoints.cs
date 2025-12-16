using Backend.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/customers")
            .WithTags("Customers");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAllCustomersQuery());
            return Results.Ok(result);
        })
        .WithName("GetAllCustomers")
        .WithDescription("Get all customers");

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCustomerByIdQuery(id));
            return result != null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetCustomerById")
        .WithDescription("Get a customer by ID");

        group.MapPost("/", async ([FromBody] CreateCustomerCommand command, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(command);
                return Results.Created($"/customers/{result.Id}", result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("CreateCustomer")
        .WithDescription("Create a new customer");

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateCustomerCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
            {
                return Results.BadRequest(new { message = "ID mismatch" });
            }

            try
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("UpdateCustomer")
        .WithDescription("Update an existing customer");

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteCustomerCommand(id));
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteCustomer")
        .WithDescription("Delete a customer");
    }
}
