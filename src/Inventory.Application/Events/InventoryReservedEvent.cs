using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Events
{
    public record InventoryReservedEvent(
    Guid ReservationId,
    Guid ProductId,
    Guid UserId,
    int Quantity,
    DateTime ExpiresAt
);
}
