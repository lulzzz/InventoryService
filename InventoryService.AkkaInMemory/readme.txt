﻿Thank you for downloading inventory service server!

Here are a few code snippets to get you started 
( SCROLL TO LAST SECTION TO SEE A RECOMENDATION FOR THE CONFIG SECTION )

NOTE : THERE IS A DEPENDENCY ON InventoryService.Messages PACKAGE THAT HAS TO BE ADDED MANUALLY!!!


            var inventoryActorAddress = "akka.tcp://InventoryService-Server@localhost:10000/user/InventoryActor";
            var serverOptions = new InventoryServerOptions()
            {
                InventoryActorAddress = inventoryActorAddress,
                ServerEndPoint = "http://*:" + InventoryServerOptions.GetFreeTcpPort() + "/",
                StorageType = typeof(InMemory),
                OnInventoryActorSystemReady = (ia, s) =>
                {
                    if (internalInventoryActorRef != null)
                    {
                        internalInventoryActorRef = ia;
                    }
                },
                ServerActorSystemName = "InventoryService-Server",
                ServerActorSystemConfig = @"
                   akka {
                   loggers = [""Akka.Logger.NLog.NLogLogger,Akka.Logger.NLog""]
                   #stdout-loglevel = DEBUG
                   loglevel = DEBUG
                   log-config-on-start = on

                    }
                   akka.actor{
                      provider= ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                      debug {
			                receive = on
				            autoreceive = on
				            lifecycle = on
				            event-stream = on
				            unhandled = on
		            }
                   }
                  akka.remote {
                      log-remote-lifecycle-events = DEBUG
                      log-received-messages = on
                      log-sent-messages = on
                    helios.tcp {
                      log-remote-lifecycle-events = DEBUG
                      log-received-messages = on
                      log-sent-messages = on
                      transport-class =""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
                      port = 10000
                      transport-protocol = tcp
                      hostname = ""localhost""
                     }
                   }                  
              "
            };
            var inventoryserviceServer = new InventoryServiceServer(serverOptions);


=============================================================================================================================


    [TestClass]
        public class UnitTest1
        {
            [TestMethod]
            public void TestMethod1()
            {
                var initialInventory = new RealTimeInventory("ticketsections-100", 10, 0, 0);
                var serverOptions = new InventoryServerOptions()
                {
                    InitialInventory = initialInventory,
                    InventoryActorAddress = "akka.tcp://InventoryService-Server@localhost:10000/user/InventoryActor",
                    ServerEndPoint = "http://*:10088/",
                    StorageType = typeof(InMemory),
                    ServerActorSystemName = "InventoryService-Server",
                    ServerActorSystemConfig = @"
                  akka.actor{provider= ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""}
                  akka.remote.helios.tcp {
                      transport-class =""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
                      port = 10000
                      transport-protocol = tcp
                      hostname = ""localhost""
                  }
              "
                };


                using (var server = new InventoryServiceServer(serverOptions))
                {
                    var mySystem = Akka.Actor.ActorSystem.Create("mySystem", ConfigurationFactory.ParseString(@"
                  akka.actor{provider= ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""}
                  akka.remote.helios.tcp {
                      transport-class =""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
                      port = 0
                      transport-protocol = tcp
                      hostname = ""localhost""
                  }
              "));
                    var inventoryActor = mySystem.ActorSelection(serverOptions.InventoryActorAddress);


                    var result =
                     server.inventoryActor.Ask<IInventoryServiceCompletedMessage>(new ReserveMessage(
                            initialInventory.ProductId, 20));

                    result.ConfigureAwait(false);

                    Task.WaitAll(result);

                    if (result.Result.Successful)
                    {
                       Log.Debug(result.Result.RealTimeInventory);
                    }
                    else
                    {
                       Log.Debug(result.Result.RealTimeInventory);
                    }
                }
            }
        }





THE CONFIG SECTION LOOKS SOMETHING LIKE THIS
==============================================

  <configSections>
    <section name="akka" type="Akka.Configuration.Hocon.AkkaConfigurationSection, Akka" />
  </configSections>
  <appSettings>
    <add key="Storage" value="InventoryService.Storage.InMemoryLib.InMemory, InventoryService.Storage.InMemoryLib" />
    <add key="RemoteInventoryActorAddress" value="akka.tcp://InventoryService-Server@localhost:10000/user/InventoryActor" />
    <add key="ServerEndPoint" value="http://*:10080/" />
    <add key="ServerActorSystemName" value="InventoryService-Server" />
  </appSettings>
  <akka>
    <hocon>
      <![CDATA[
        akka {
          actor {
             provider = "Akka.Remote.RemoteActorRefProvider, Akka.Remote"
          }
          remote {
                helios.tcp {
                    port = 10000
                    hostname =0.0.0.0
                    public-hostname = "localhost"
                }
            }
        }
      ]]>
    </hocon>
  </akka>