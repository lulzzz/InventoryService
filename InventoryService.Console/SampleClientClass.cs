using Akka.Actor;
using Akka.Util.Internal;
using InventoryService.Messages.Request;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace InventoryService.Console
{
    public class SampleClientClass
    {
        public async Task StartSampleClientAsync()
        {
            const int productCount = 300;
            const int initialQuantity = 1;

            IList<Tuple<string, int, int>> products = new List<Tuple<string, int, int>>();

            for (var i = 0; i < 100000; i++)
            {
                products.Add(new Tuple<string, int, int>("ticketsections-" + i, initialQuantity, 0));
            }

            //products.Add(new Tuple<string, int, int>("ticketsections-2", initialQuantity, 0));
            //products.Add(new Tuple<string, int, int>("ticketsections-3", initialQuantity, 0));
            //products.Add(new Tuple<string, int, int>("ticketsections-4", initialQuantity, 0));
            //products.Add(new Tuple<string, int, int>("ticketsections-5", initialQuantity, 0));
            //products.Add(new Tuple<string, int, int>("ticketsections-6", initialQuantity, 0));

            //products.Add(new Tuple<string, int, int>("me-216", initialQuantity, 0));
            //products.Add(new Tuple<string, int, int>("me-217", initialQuantity, 0));

            //products.Add(new Tuple<string, int, int>("me-1", initialQuantity, 0));
            //products.Add(new Tuple<string, int, int>("me-2", initialQuantity, 0));
            //products.Add(new Tuple<string, int, int>("me-3", initialQuantity, 0));
            //products.Add(new Tuple<string, int, int>("me-4", initialQuantity, 0));
            //products.Add(new Tuple<string, int, int>("me-5", initialQuantity, 0));
            //products.Add(new Tuple<string, int, int>("me-6", initialQuantity, 0));

            System.Console.WriteLine("Starting Client");
            var actorSystem = ActorSystem.Create("InventoryService-Client");
            {
                var remoteAddress = ConfigurationManager.AppSettings["RemoteInventoryActorAddress"];
                var inventoryActorSelection =
                    actorSystem.ActorSelection(remoteAddress);

                var inventoryActor = inventoryActorSelection;

                var stopwatch = new Stopwatch();

                stopwatch.Start();

                //                for (var j = 0;j< 1000; j++)
                //                {
                //for (var i = 0; i < 10; i++)
                //                {
                //                    try
                //                    {
                //                        inventoryActor.Tell(new UpdateQuantityMessage("test2", 1));
                //                    }
                //                    catch (Exception ex)
                //                    {
                //                        System.Console.WriteLine("Failed on iteration {0} while updating quantity {1} : {2}", i,
                //                   "test2",
                //                            ex.Message + " - " + ex);
                //                    }
                //                }
                //                    Task.Delay(TimeSpan.FromSeconds(1)).TODO /* USE PROPER ASYNC AWAIT HERE */
                //                }

                //  var m = await inventoryActor.Ask(new UpdateQuantityMessage("test", 1));
                // m.TODO /* USE PROPER ASYNC AWAIT HERE */
                //  var n=  m.Result;
                var counter = 0;
                var totalIteration = 10000;
                var ticketSectionNumber = new Random();
                await Task.Delay(1000);

                products.ForEach(p =>
                {
                    for (var i = 0; i < totalIteration; i++)
                    {
                        try
                        {
                            inventoryActor.Tell(new UpdateQuantityMessage("ticketsections-" + ticketSectionNumber.Next(216, 216),10000));//.TODO /* USE PROPER ASYNC AWAIT HERE */

                            inventoryActor.Tell(new ReserveMessage("ticketsections-" + ticketSectionNumber.Next(216, 216), 1));//.TODO /* USE PROPER ASYNC AWAIT HERE */

                            // inventoryActor.Tell(new ReserveMessage("ticketsections-" + ticketSectionNumber.Next(1,999), i));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //inventoryActor.Ask(new ReserveMessage(p.Item1, 1));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //inventoryActor.Ask(new PlaceHoldMessage(p.Item1, 1));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //inventoryActor.Ask(new GetInventoryMessage(p.Item1));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //inventoryActor.Ask(new UpdateQuantityMessage(p.Item1, 10));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //inventoryActor.Ask(new GetInventoryMessage(p.Item1));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //if (i % 3 == 0)
                            //{
                            //    inventoryActor.ResolveOne(TimeSpan.FromSeconds(3));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //    actorSystem.Terminate();
                            //    inventoryActor =
                            //         ActorSystem.Create("InventoryService-Client").ActorSelection(remoteAddress);
                            //    inventoryActor.ResolveOne(TimeSpan.FromSeconds(3));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //    Task.Delay(TimeSpan.FromSeconds(1));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //    inventoryActor.ResolveOne(TimeSpan.FromSeconds(3));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //}

                            //if (i % 7 == 0)
                            //{
                            //    inventoryActor.ResolveOne(TimeSpan.FromSeconds(3));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //    actorSystem = ActorSystem.Create("InventoryService-Client");
                            //    inventoryActor =
                            //      actorSystem.ActorSelection(remoteAddress);
                            //    inventoryActor.ResolveOne(TimeSpan.FromSeconds(3));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //    Task.Delay(TimeSpan.FromSeconds(1));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //    inventoryActor.ResolveOne(TimeSpan.FromSeconds(3));//.TODO /* USE PROPER ASYNC AWAIT HERE */
                            //}
                            counter++;
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine("Failed on iteration {0} while updating quantity {1} : {2}", i,
                                p.Item1,
                                ex.Message + " - " + ex);
                        }

                        //     inventoryActor.Tell(new ReserveMessage(p.Item1, 1));
                        //    System.Console.WriteLine("Updating  item {0}'s quantity for product {0}", i, p.Item1);

                        //   await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                });
                if (counter != products.Count * totalIteration)
                {
                    throw new Exception();
                }
                //try
                //{
                //    var x = await inventoryActor.Ask<IInventoryServiceCompletedMessage>(new PurchaseMessage("ticketsection-216", 2));
                //}
                //catch (Exception e)
                //{
                //}

                // Task.WaitAll(actorSystem.Terminate());
                stopwatch.Stop();

                System.Console.WriteLine("Elapsed: {0}", stopwatch.Elapsed.TotalSeconds);
                System.Console.WriteLine("Speed: {0} per second", productCount * initialQuantity / stopwatch.Elapsed.TotalSeconds);
            }
        }
    }
}