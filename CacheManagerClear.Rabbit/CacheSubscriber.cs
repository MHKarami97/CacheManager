﻿using System.Text;
using CacheManager;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CacheManagerClear.Rabbit;

/// <summary>
/// Subscriber for cache clear event by RabbitMQ
/// </summary>
public class CacheSubscriber : ICacheSubscriber
{
	private readonly IConnection _connection;
	private readonly IEasyCacheManager _cacheManager;
	private readonly string _exchange;
	private readonly string _queue;
	private bool _disposed;

	/// <summary>
	/// Subscriber for cache clear event by RabbitMQ
	/// </summary>
	/// <param name="connection">RabbitMQ connection</param>
	/// <param name="cacheManager">Cache manager instance</param>
	/// <param name="exchange">RabbitMQ exchange name</param>
	/// <param name="queue">RabbitMQ queue name</param>
	public CacheSubscriber(IConnection connection, string exchange, string queue, IEasyCacheManager cacheManager)
	{
		_connection = connection ?? throw new ArgumentNullException(nameof(connection));
		_cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
		_exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
		_queue = queue ?? throw new ArgumentNullException(nameof(queue));
	}

	/// <summary>
	/// Subscribes to cache clear events and processes them.
	/// </summary>
	public async Task SubscribeAsync(CancellationToken cancellationToken)
	{
		try
		{
			using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

			await channel.ExchangeDeclareAsync(exchange: _exchange, type: ExchangeType.Fanout, durable: true, cancellationToken: cancellationToken).ConfigureAwait(false);
			_ = await channel.QueueDeclareAsync(queue: _queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken).ConfigureAwait(false);
			await channel.QueueBindAsync(queue: _queue, exchange: _exchange, routingKey: string.Empty, cancellationToken: cancellationToken).ConfigureAwait(false);

			var consumer = new AsyncEventingBasicConsumer(channel);

			consumer.ReceivedAsync += async (_, ea) => await ProcessAsync(ea, channel, cancellationToken).ConfigureAwait(false);

			// Start consuming messages
			_ = await channel.BasicConsumeAsync(queue: _queue, autoAck: false, consumer: consumer, cancellationToken: cancellationToken).ConfigureAwait(false);
		}
		catch (TaskCanceledException)
		{
		}
		catch (Exception e)
		{
			Console.WriteLine(e);

			await Task.Delay(100, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Stops the Kafka subscription process.
	/// </summary>
	public async Task StopAsync()
	{
		if (_disposed)
		{
			return;
		}

		await _connection.DisposeAsync().ConfigureAwait(false);
		await _connection.CloseAsync().ConfigureAwait(false);
		_disposed = true;
	}

	/// <summary>
	/// Disposes the Kafka consumer and cancels the subscription.
	/// </summary>
	/// <returns></returns>
	public async ValueTask DisposeAsync()
	{
		if (_disposed)
		{
			return;
		}

		await StopAsync().ConfigureAwait(false);

		_disposed = true;
	}

	private async Task ProcessAsync(BasicDeliverEventArgs ea, IChannel channel, CancellationToken cancellationToken)
	{
		try
		{
			var key = Encoding.UTF8.GetString(ea.Body.ToArray());

			if (key.Equals(StaticData.ClearAllKey, StringComparison.Ordinal))
			{
				await _cacheManager.ClearAllCacheAsync().ConfigureAwait(false);
			}
			else
			{
				await _cacheManager.ClearCacheAsync(key).ConfigureAwait(false);
			}

			await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken).ConfigureAwait(false);
		}
		catch (TaskCanceledException)
		{
		}
		catch (Exception e)
		{
			Console.WriteLine(e);

			await Task.Delay(100, cancellationToken).ConfigureAwait(false);
		}
	}
}
