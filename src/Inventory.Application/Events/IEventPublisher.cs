using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Events;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}
