using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Infrastructure.Messaging;

public class RabbitMqEventBus : IEventBus, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private readonly string _exchange;

    public RabbitMqEventBus(IOptions<RabbitMqOptions> options, ILogger<RabbitMqEventBus> logger)
    {
        _logger = logger;
        var opts = options.Value;
        _exchange = opts.Exchange;

        var factory = new ConnectionFactory
        {
            HostName = opts.Host,
            Port = opts.Port,
            UserName = opts.Username,
            Password = opts.Password
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _channel.ExchangeDeclareAsync(_exchange, ExchangeType.Topic, durable: true).GetAwaiter().GetResult();
    }

    public async Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        var routingKey = typeof(T).Name;
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await _channel.BasicPublishAsync(
            exchange: _exchange,
            routingKey: routingKey,
            body: body,
            cancellationToken: ct);

        _logger.LogDebug("Published {MessageType} to {Exchange}/{RoutingKey}", typeof(T).Name, _exchange, routingKey);
    }

    public async Task SubscribeAsync<T>(string queue, Func<T, Task> handler, CancellationToken ct = default) where T : class
    {
        var routingKey = typeof(T).Name;
        await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await _channel.QueueBindAsync(queue, _exchange, routingKey, cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));
                if (message is not null)
                    await handler(message);
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {RoutingKey}", routingKey);
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: ct);
            }
        };

        await _channel.BasicConsumeAsync(queue, autoAck: false, consumer: consumer, cancellationToken: ct);
        _logger.LogInformation("Subscribed to {Queue} for {MessageType}", queue, typeof(T).Name);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
        _channel.Dispose();
        _connection.Dispose();
    }
}

public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5900;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Exchange { get; set; } = "workpac.events";
}
