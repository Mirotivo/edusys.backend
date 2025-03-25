using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Backend.Database.Models;
using Backend.Interfaces.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Backend.Services.BackgroundServices
{
    public class EventProcessorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EventProcessorService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AvanciraDbContext>();

                var unprocessedEvents = await dbContext.Events
                    .Where(e => !e.Processed)
                    .AsTracking()
                    .ToListAsync();

                foreach (var storedEvent in unprocessedEvents)
                {
                    try
                    {
                        var handler = FindHandlerForEvent(storedEvent, scope.ServiceProvider);
                        if (handler != null)
                        {
                            var eventDataType = handler.GetType()
                                .GetInterfaces()
                                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                                .GetGenericArguments()[0];

                            var eventData = JsonSerializer.Deserialize(storedEvent.Data, eventDataType);
                            if (eventData != null)
                            {
                                var handleMethod = handler.GetType().GetMethod(nameof(IEventHandler<object>.HandleAsync));
                                if (handleMethod != null)
                                {
                                    await (Task)handleMethod.Invoke(handler, new object[] { eventData });
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No handler found for event type: {storedEvent.EventType}");
                        }

                        storedEvent.Processed = true;
                        dbContext.Events.Update(storedEvent);
                        await dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing event {storedEvent.Id}: {ex.Message}");
                    }
                }

                await Task.Delay(60000, stoppingToken); // Check every 60 seconds
            }
        }

        private object FindHandlerForEvent(StoredEvent storedEvent, IServiceProvider serviceProvider)
        {
            var handlerType = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(t => !t.IsAbstract && !t.IsInterface &&
                    t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)) &&
                    t.Name.Contains(storedEvent.EventType.ToString(), StringComparison.OrdinalIgnoreCase));

            if (handlerType == null)
                return null;

            var interfaceType = handlerType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>));

            return interfaceType != null ? serviceProvider.GetService(interfaceType) : null;
        }
    }
}

