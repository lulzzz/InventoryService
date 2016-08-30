﻿using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Services;
using InventoryService.Storage;
using System.Threading.Tasks;

namespace InventoryService.Actors
{
    public class ProductInventoryActor : ReceiveActor
    {
        private readonly string _id;


        private readonly IProductInventoryOperations _productInventoryOperations;

        public ProductInventoryActor(IInventoryStorage inventoryStorage, string id)
        {
            _id = id;

           

            Become(Running);
            _productInventoryOperations = new ProductInventoryOperations(inventoryStorage, id);
        }

        private void Running()
        {
            Receive<GetInventoryMessage>(message =>
           {
               //todo cache
              // if (message.GetNonStaleResult)
             //  {
                   _productInventoryOperations.ReadInventory(message.ProductId).ContinueWith(result =>
                   {
                       var thatMessage = message;
                       if (!result.Result.IsSuccessful)
                       {
                           return
                               new GetInventoryCompletedMessage(
                                   result.Result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to get inventory on product " + thatMessage.ProductId));
                       }

                       var quantity = result.Result.Result.Quantity;
                       var reservations = result.Result.Result.Reservations;
                       var holds = result.Result.Result.Holds;
                       return new GetInventoryCompletedMessage(message.ProductId, quantity, reservations, holds);
                   },
                   TaskContinuationOptions.AttachedToParent
                   & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
             //  }
           });

            Receive<ReserveMessage>(message =>
           {
               _productInventoryOperations.Reserve(message.ProductId, message.ReservationQuantity).ContinueWith(result =>
               {
                   var thatMessage = message;
                   if (!result.Result.IsSuccessful)
                   {
                       return
                           new ReserveCompletedMessage(
                               result.Result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a reserve of " + thatMessage.ReservationQuantity + " on product " + thatMessage.ProductId));
                   }


                   var quantity = result.Result.Result.Quantity;
                   var reservations = result.Result.Result.Reservations;
                   var holds = result.Result.Result.Holds;
                   return new ReserveCompletedMessage(message.ProductId, quantity, reservations, holds, true);
               }, TaskContinuationOptions.AttachedToParent
                   & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<UpdateQuantityMessage>(message =>
           {
               _productInventoryOperations.UpdateQuantity(message.ProductId, message.Quantity).ContinueWith(result =>
                {
                    var thatMessage = message;
                    if (!result.Result.IsSuccessful)
                    {
                        return
                            new UpdateQuantityCompletedMessage(
                                result.Result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a update quantity of " + thatMessage.Quantity + " on product " + thatMessage.ProductId));
                    }


                    var quantity = result.Result.Result.Quantity;
                    var reservations = result.Result.Result.Reservations;
                    var holds = result.Result.Result.Holds;
                    return new UpdateQuantityCompletedMessage(message.ProductId, quantity, reservations, holds, true);
                }, TaskContinuationOptions.AttachedToParent
                       & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<PlaceHoldMessage>(message =>
           {
               _productInventoryOperations.PlaceHold(message.ProductId, message.Holds).ContinueWith(result =>
               {
                   var thatMessage = message;
                   if (!result.Result.IsSuccessful)
                   {
                       return
                           new PlaceHoldCompletedMessage(
                               result.Result.ToInventoryOperationErrorMessage(thatMessage.ProductId,"Operation failed while trying to do a hold of "+ thatMessage.Holds+" on product "+ thatMessage.ProductId));
                   }

                   var quantity = result.Result.Result.Quantity;
                   var reservations = result.Result.Result.Reservations;
                   var holds = result.Result.Result.Holds;
                   return new PlaceHoldCompletedMessage(message.ProductId, quantity, reservations, holds, true);
               }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<PurchaseMessage>(message =>
           {
               _productInventoryOperations.Purchase(message.ProductId, message.Quantity).ContinueWith(result =>
               {
                   var thatMessage = message;
                   if (!result.Result.IsSuccessful)
                   {
                       return
                           new PurchaseCompletedMessage(
                               result.Result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a purchase of " + thatMessage.Quantity + " on product " + thatMessage.ProductId));
                   }


                   var quantity = result.Result.Result.Quantity;
                   var reservations = result.Result.Result.Reservations;
                   var holds = result.Result.Result.Holds;
                   return new PurchaseCompletedMessage(message.ProductId, quantity, reservations, holds, true);
               }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<PurchaseFromHoldsMessage>(message =>
           {
               _productInventoryOperations.PurchaseFromHolds(message.ProductId, message.Quantity).ContinueWith(result =>
               {
                   var thatMessage = message;
                   if (!result.Result.IsSuccessful)
                   {
                       return
                           new PurchaseFromHoldsCompletedMessage(
                               result.Result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a purchase from hold of " + thatMessage.Quantity + " on product " + thatMessage.ProductId));
                   }


                   var quantity = result.Result.Result.Quantity;
                   var reservations = result.Result.Result.Reservations;
                   var holds = result.Result.Result.Holds;
                   return new PurchaseFromHoldsCompletedMessage(message.ProductId, quantity, reservations, holds, true);
               }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });
            Receive<FlushStreamsMessage>(message =>
            {
                _productInventoryOperations.InventoryStorageFlush(_id).ContinueWith(
                    result => { }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
            });
        }
    }
}