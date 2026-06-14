namespace WorkPac.Recruitment.Shared.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;
    Task SubscribeAsync<T>(string queue, Func<T, Task> handler, CancellationToken ct = default) where T : class;
}
