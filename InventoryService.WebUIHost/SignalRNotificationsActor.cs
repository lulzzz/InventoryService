﻿using Akka.Actor;
using Akka.Event;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.NotificationSubscriptionMessages;
using InventoryService.Messages.Request;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryService.WebUIHost
{
    public class SignalRInventoryQueryActor : ReceiveActor
    {
        public readonly ILoggingAdapter Logger = Context.GetLogger();
        private SignalRNotificationService SignalRNotificationService { set; get; }
        private readonly List<Type> _requestmessageTypes;
        private string SubscriptionId { set; get; }

        private QueryInventoryCompletedMessage CurrentInventoryCompletedMessage { set; get; }
        protected double MessageSpeed = 0;
        protected double PeakMessageSpeed = 0;

        public SignalRInventoryQueryActor(string inventoryNotificationActorAddress, string remoteInventoryActorAddress)
        {
            var type = typeof(IRequestMessage);
            _requestmessageTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p =>
                type.IsAssignableFrom(p) &&
                p.Name != nameof(IRequestMessage) &&
                p.Name != nameof(GetInventoryMessage) &&
                p.Name != nameof(ResetInventoryQuantityReserveAndHoldMessage)
                 ).ToList();

            SignalRNotificationService = new SignalRNotificationService();

            Receive<ExportAllInventoryMessage>(message =>
            {
                if (InventoryQueryActorRef != null)
                {
                    InventoryQueryActorRef.Tell(message);
                }
                else
                {
                    SignalRNotificationService.SendJsonResultNotification("Unable to send ExportAllInventory Message ");
                }
            });

            Receive<ExportAllInventoryCompletedMessage>(message =>
            {
                SignalRNotificationService.SendJsonResultNotification($"Inventory export :{(message.InventoriesCsv == null ? $"failed!" : "succeeded!")}");
                SignalRNotificationService.SendInventoryExportCsv(message.InventoriesCsv);
            });

            Receive<RequestInstructionIntoRemoteServermessage>(message =>
           {
               object messageToSend = null;
               try
               {
                   if (message.Message != null)
                       messageToSend = message.Message;
                   else if (!string.IsNullOrEmpty(message.OperationName))
                   {
                       var messageType = _requestmessageTypes.FirstOrDefault(x => x.Name == message.OperationName);
                       if (messageType != null)
                       {
                           messageToSend = Activator.CreateInstance(messageType, message.ProductId, message.Quantity);
                       }
                   }
                   else
                   {
                       SignalRNotificationService.SendJsonResultNotification("Can't send message to remote actor - Unknown message" + JsonConvert.SerializeObject(message.Message));
                   }

                   if (!(messageToSend is IRequestMessage))
                   {
                       SignalRNotificationService.SendJsonResultNotification("Can't send message to remote actor -  Unknown message " + JsonConvert.SerializeObject(message.Message));
                   }
               }
               catch (Exception e)
               {
                   SignalRNotificationService.SendJsonResultNotification("Can't send message to remote actor - " + JsonConvert.SerializeObject(e));
               }
               if ((!(messageToSend is IRequestMessage))) return;

               SendMessageToRemoteInventoryActor(remoteInventoryActorAddress, messageToSend, message.RetryCount);
           });

            Receive<IInventoryServiceCompletedMessage>(message =>
            {
                SignalRNotificationService.SendJsonResultNotification(message.GetType().Name + " - " + JsonConvert.SerializeObject(message));
            });

            Receive<GetMetricsCompletedMessage>(message =>
            {
                SignalRNotificationService.SendMessageSpeed(message.MessageSpeedPersecond);
                Logger.Debug("received  - " + message.GetType().Name + " - MessageSpeedPersecond :" + message.MessageSpeedPersecond);
            });

            Receive<RealTimeInventoryChangeMessage>(message =>
            {
                CurrentInventoryCompletedMessage = new QueryInventoryCompletedMessage(new List<IRealTimeInventory>() { message.RealTimeInventory }, MessageSpeed, PeakMessageSpeed);
                //MessageCount++;
                //var secondsPast = (int)(DateTime.UtcNow - lastUpadteTime).TotalSeconds;

                //if (secondsPast <= 1) return;

                //MessageSpeed = MessageCount / secondsPast;
                //PeakMessageSpeed = (PeakMessageSpeed > MessageSpeed) ? PeakMessageSpeed : MessageSpeed;
                //lastUpadteTime = DateTime.UtcNow;
                //MessageCount = 0;
            });

            Receive<QueryInventoryCompletedMessage>(message =>
            {
                MessageSpeed = message.Speed;
                PeakMessageSpeed = message.PeakMessageSpeed;
                // SignalRNotificationService.SendInventoryList(new QueryInventoryCompletedMessage(message.RealTimeInventories?? new List<IRealTimeInventory>() , MessageSpeed, PeakMessageSpeed), _requestmessageTypes.Select(x => x.Name).ToList());
                SignalRNotificationService.SendInventoryList(message, _requestmessageTypes.Select(x => x.Name).ToList());
            });

            Receive<SendNotificationToClientMessage>(message =>
            {
                SignalRNotificationService.SendInventoryList(CurrentInventoryCompletedMessage, _requestmessageTypes.Select(x => x.Name).ToList());
            });

            Receive<IRequestMessage>(message =>
            {
                SignalRNotificationService.SendIncomingMessage(message.GetType().Name + " : " + message.Update + " for " + message.ProductId);
                Logger.Debug("received by inventory Actor - " + message.GetType().Name + " - " + message.ProductId + " : quantity " + message.Update);
            });

            Receive<ServerNotificationMessage>(message =>
            {
                SignalRNotificationService.SendServerNotification(message.ServerMessage);
                Logger.Debug("received  - " + message.GetType().Name + " -  ServerMessage : " + message.ServerMessage);
            });

            Receive<GetAllInventoryListMessage>(message =>
            {
                if (InventoryQueryActorRef != null && !InventoryQueryActorRef.IsNobody())
                {
                    InventoryQueryActorRef.Tell(message);
                }
                else
                {
                    Self.Tell(new SubscribeToNotifications());
                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(5), Self, new GetAllInventoryListMessage(), Self);
                }
            });
            //  Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1), Self, new GetAllInventoryListMessage(), Self);

            ReceiveAsync<UnSubscribedNotificationMessage>(async _ =>
            {
                Logger.Error("Suddenly unsubscribed to notification. ");
                InventoryQueryActorRef = await SubscribeToRemoteNotificationActorMessasges(inventoryNotificationActorAddress);
            });
            ReceiveAsync<SubscribeToNotifications>(async _ =>
            {
                InventoryQueryActorRef = await SubscribeToRemoteNotificationActorMessasges(inventoryNotificationActorAddress);
            });

            Receive<SubScribeToNotificationCompletedMessage>(message =>
            {
                SubscriptionId = message.SubscriptionId;
                Context.Watch(InventoryQueryActorRef);
                Self.Tell(new GetAllInventoryListMessage());
            });

            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), Self, new SubscribeToNotifications(), Self);

            Receive<Terminated>(t =>
            {
                Logger.Error("Suddenly unsubscribed from notification at " + t.ActorRef.Path + ". trying to subscribe again ");
                Self.Tell(new SubscribeToNotifications());
            });
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1), Self, new SendNotificationToClientMessage(), Self);
        }

        private void SendMessageToRemoteInventoryActor(string remoteInventoryActorAddress, object messageToSend, int numberRetry = 1)
        {
            try
            {
                var actorRef = Context.ActorSelection(remoteInventoryActorAddress).ResolveOne(TimeSpan.FromSeconds(10)).Result;
                for (var i = 0; i < numberRetry; i++)
                {
                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromMilliseconds(1000), actorRef, messageToSend, Self);
                }
            }
            catch (Exception e)
            {
                SignalRNotificationService.SendJsonResultNotification("Can't send message to remote actor - " +
                                                                      JsonConvert.SerializeObject(e));
            }
        }

        public IActorRef InventoryQueryActorRef { get; set; }

        private async Task<IActorRef> SubscribeToRemoteNotificationActorMessasges(string inventoryActorAddress)
        {
            var InventoryQueryActor = Context.ActorSelection(inventoryActorAddress);
            Logger.Debug("Trying to reach remote actor at  " + inventoryActorAddress + " ....");
            var isReachable = false;
            var retryMax = 10;
            var expDelay = 0;
            IActorRef InventoryQueryActorRef = null;
            while (!isReachable && retryMax > 0)
            {
                try
                {
                    InventoryQueryActorRef = await InventoryQueryActor.ResolveOne(TimeSpan.FromSeconds(3));

                    isReachable = true;
                    Logger.Debug("Successfully reached " + inventoryActorAddress + " ....");
                }
                catch (Exception e)
                {
                    retryMax--;
                    await Task.Delay(TimeSpan.FromSeconds(expDelay++));
                    isReachable = false;
                    Logger.Error("remote actor is not reachable, so im retrying " + inventoryActorAddress + " ....", e);
                }
            }

            if (isReachable)
            {
                Logger.Debug("Subscribing for notifications at " + inventoryActorAddress + " ....");
                Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), InventoryQueryActor, new SubScribeToNotificationMessage(), Self);
            }
            else
            {
                //kill the actor
                await Self.GracefulStop(TimeSpan.FromSeconds(10));
            }
            return await Task.FromResult(InventoryQueryActorRef);
        }

        public class SubscribeToNotifications
        {
        }
    }

    public class MonitorHealthMessage
    {
    }
}