using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace Inventory.Application.Commands.ReserveStock
{

    public record ReserveStockCommand(
        Guid ProductId,
        int Quantity,
        Guid UserId
    ) : IRequest<ReserveStockResult>;

    public record ReserveStockResult(
        bool Success,
        Guid? ReservationId = null,
        string? ErrorMessage = null
    );
}
