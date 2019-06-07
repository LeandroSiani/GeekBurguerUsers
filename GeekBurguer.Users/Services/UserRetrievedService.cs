using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using System.Data.Entity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using GeekBurguer.Users.Models;
using GeekBurguer.Users.Services.GeekBurger.Products.Service;

namespace GeekBurguer.Users.Services
{
    public class UserRetrievedService : IUserRetrievedService
    {
        private const string Topic = "UserRetrieved";
        private readonly IConfiguration _configuration;
        private IMapper _mapper;
        private readonly List<Message> _messages;
        private Task _lastTask;
        private readonly IServiceBusNamespace _namespace;
        //private readonly ILogService _logService;
        private CancellationTokenSource _cancelMessages;
        private IServiceProvider _serviceProvider { get; }

        public async void SendMessagesAsync()
        {
            if (_lastTask != null && !_lastTask.IsCompleted)
                return;

            var config = _configuration.GetSection("serviceBus").Get<ServiceBusConfiguration>();
            var topicClient = new TopicClient(config.ConnectionString, Topic);

            //_logService.SendMessagesAsync("Product was changed");

            _lastTask = SendAsync(topicClient, _cancelMessages.Token);

            await _lastTask;

            var closeTask = topicClient.CloseAsync();
            await closeTask;
            HandleException(closeTask);
        }

        public async Task SendAsync(TopicClient topicClient,
            CancellationToken cancellationToken)
        {
            var tries = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_messages.Count <= 0)
                    break;

                Message message;
                lock (_messages)
                {
                    message = _messages.FirstOrDefault();
                }

                var sendTask = topicClient.SendAsync(message);
                await sendTask;
                var success = HandleException(sendTask);

                if (!success)
                {
                    var cancelled = cancellationToken.WaitHandle.WaitOne(10000 * (tries < 60 ? tries++ : tries));
                    if (cancelled) break;
                }
                else
                {
                    if (message == null) continue;
                    //AddOrUpdateEvent(new ProductChangedEvent() { EventId = new Guid(message.MessageId) });
                    _messages.Remove(message);
                }
            }
        }

        //private void AddOrUpdateEvent(ProductChangedEvent productChangedEvent)
        //{
            //using (var scope = _serviceProvider.CreateScope())
            //{
                //var scopedProcessingService =
                //    scope.ServiceProvider
                //        .GetRequiredService<IProductChangedEventRepository>();

                //ProductChangedEvent evt;
                //if (productChangedEvent.EventId == Guid.Empty
                //    || (evt = scopedProcessingService.Get(productChangedEvent.EventId)) == null)
                //    scopedProcessingService.Add(productChangedEvent);
                //else
                //{
                //    evt.MessageSent = true;
                //    scopedProcessingService.Update(evt);
                //}

                //scopedProcessingService.Save();
            //}
        //}

        public void AddToMessageList(IEnumerable<EntityEntry<User>> changes)
        {
            //_messages.AddRange(changes
            //.Where(entity => entity.State != EntityState.Detached
            //        && entity.State != EntityState.Unchanged)
            //.Select(GetMessage).ToList());
        }

        public Message GetMessage(EntityEntry<User> entity)
        {
            //var productChanged = Mapper.Map<ProductChangedMessage>(entity);
            //var productChangedSerialized = JsonConvert.SerializeObject(productChanged);
            //var productChangedByteArray = Encoding.UTF8.GetBytes(productChangedSerialized);

            //var productChangedEvent = Mapper.Map<ProductChangedEvent>(entity);
            //AddOrUpdateEvent(productChangedEvent);

            return new Message
            {
                //Body = productChangedByteArray,
                //MessageId = productChangedEvent.EventId.ToString(),
                //Label = productChanged.Product.StoreId.ToString()
            };
        }

        public bool HandleException(Task task)
        {
            if (task.Exception == null || task.Exception.InnerExceptions.Count == 0) return true;

            task.Exception.InnerExceptions.ToList().ForEach(innerException =>
            {
                Console.WriteLine($"Error in SendAsync task: {innerException.Message}. Details:{innerException.StackTrace} ");

                if (innerException is ServiceBusCommunicationException)
                    Console.WriteLine("Connection Problem with Host. Internet Connection can be down");
            });

            return false;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            EnsureTopicIsCreated();

            return Task.CompletedTask;
        }

        public void EnsureTopicIsCreated()
        {
            if (!_namespace.Topics.List()
                .Any(topic => topic.Name
                    .Equals(Topic, StringComparison.InvariantCultureIgnoreCase)))
                _namespace.Topics.Define(Topic)
                    .WithSizeInMB(1024).Create();

        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancelMessages.Cancel();

            return Task.CompletedTask;
        }
    }
}
