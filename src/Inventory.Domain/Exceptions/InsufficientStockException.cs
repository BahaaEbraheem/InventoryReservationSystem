using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain.Exceptions
{
    public class InsufficientStockException : Exception
    {
        public Guid ProductId { get; }
        public int AvailableStock { get; }
        public int RequestedQuantity { get; }

        public InsufficientStockException(Guid productId, int availableStock, int requestedQuantity)
            : base($"Product {productId} has insufficient stock. Available: {availableStock}, Requested: {requestedQuantity}")
        {
            ProductId = productId;
            AvailableStock = availableStock;
            RequestedQuantity = requestedQuantity;
        }
    }
}
