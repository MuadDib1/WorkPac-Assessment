using System.Collections.Concurrent;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Infrastructure.Messaging;

public class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Func<object, Task>>> _handlers = new();

    public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        if (_handlers.TryGetValue(typeof(T), out var handlers))
        {
            var tasks = handlers.Select(h => h(message));
            return Task.WhenAll(tasks);
        }
        return Task.CompletedTask;
    }

    public Task SubscribeAsync<T>(string queue, Func<T, Task> handler, CancellationToken ct = default) where T : class
    {
        _handlers.AddOrUpdate(
            typeof(T),
            _ => [msg => handler((T)msg)],
            (_, list) =>
            {
                list.Add(msg => handler((T)msg));
                return list;
            });
        return Task.CompletedTask;
    }
}
