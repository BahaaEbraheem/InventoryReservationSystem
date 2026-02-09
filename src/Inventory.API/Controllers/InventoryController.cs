using Microsoft.AspNetCore.Mvc;
using MediatR;
using Inventory.Application.Commands.ReserveStock;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Reserve stock for a product (temporary hold for 2 minutes)
    /// </summary>
    [HttpPost("reserve")]
    [ProducesResponseType(typeof(ReserveStockResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReserveStock([FromBody] ReserveStockRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new ReserveStockCommand(
            request.ProductId,
            request.Quantity,
            request.UserId
        );

        var result = await mediator.Send(command);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new
        {
            success = true,
            reservationId = result.ReservationId,
            message = "Stock reserved successfully. Expires in 2 minutes."
        });
    }
}

public record ReserveStockRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public Guid UserId { get; init; }
}