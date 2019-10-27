using Config.Net;
using Fleck;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Net;
using Serilog;
using MessagePack;
using System.Threading;
using System.Threading.Tasks;

namespace SightReader.Engine.Server
{
    public class CommandServer
    {
        private void SetupLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("introducer.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();
        }

        private IConfig LoadConfig()
        {
            return new ConfigurationBuilder<IConfig>()
                  .UseJsonConfig("config.json")
                  .Build();
        }

        private IWebSocketServer CreateWebsocketServer(IConfig config)
        {
            var currentDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

            FleckLog.Level = LogLevel.Info;
            var server = new WebSocketServer($"ws://0.0.0.0:{config.Port}", false);
            server.RestartAfterListenError = true;

            return server;
        }

        private async Task PeriodicPing(IWebSocketConnection socket, TimeSpan interval, CancellationToken cancellationToken)
        {
            byte numMissedHeartbeats = 0;

            socket.OnPong = (byte[] response) =>
            {
                numMissedHeartbeats -= 1;
            };

            while (socket.IsAvailable)
            {
                if (numMissedHeartbeats >= 5)
                {
                    Log.Debug($"Closing socket {socket.ConnectionInfo.Id} because it missed {numMissedHeartbeats} heartbeats.");
                    socket.Close();
                }

                numMissedHeartbeats += 1;
                await socket.SendPing(new byte[] { 255 });
                await Task.Delay(interval, cancellationToken);
            }
        }

        private void BeginPeriodicPing(IWebSocketConnection socket)
        {
            var _ = PeriodicPing(socket, new TimeSpan(0, 0, 5), new CancellationToken());
        }

        public void Run(IEngineContext engine)
        {
            SetupLogging();
            var config = LoadConfig();
            var clients = new ClientManager();
            var server = CreateWebsocketServer(config);
            var processor = new CommandProcessor(engine, clients);

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    lock (this)
                    {
                        if (clients.Count >= config.MaxWebsocketConnections)
                        {
                            Log.Debug($"{socket.ConnectionInfo.ClientIpAddress}: [Connected] Over connection limit with {clients.Count} sockets connected. Dropping connection.");
                            socket.Close();
                        }
                        else
                        {
                            Log.Debug($"{socket.ConnectionInfo.ClientIpAddress}: [Connected].");

                            var client = new Client() { Socket = socket };
                            Log.Debug($"{client.Id}: [Register].");
                            clients.Register(client);

                            BeginPeriodicPing(socket);
                        }
                    }
                };

                socket.OnClose = () =>
                {
                    Log.Debug($"{socket.ConnectionInfo.ClientIpAddress}: [Disconnected].");

                    var client = clients.FindById(socket.ConnectionInfo.Id);

                    if (client == null)
                    {
                        return;
                    }

                    Log.Debug($"{client.Id}: [Unregister].");
                    clients.Unregister(client);
                };

                socket.OnError = error =>
                {
                    if (socket.IsAvailable)
                    {
                        socket.Close();
                        Log.Error($"[Socket] Error (socket closed): {error.ToString()}");
                        var client = clients.FindById(socket.ConnectionInfo.Id);
                        if (client != null)
                        {
                            clients.Unregister(client);
                        }
                    } else
                    {
                        Log.Error($"[Socket] Error: {error.ToString()}");
                    }
                };

                socket.OnBinary = bytes =>
                {
                    lock (this)
                    {
                        var client = clients.FindById(socket.ConnectionInfo.Id);

                        if (client == null)
                        {
                            client = new Client() { Socket = socket };
                            Log.Debug($"{client.Id}: [Register].");
                            clients.Register(client);
                        }

                        var command = MessagePackSerializer.Deserialize<GenericCommand>(bytes);

                        processor.Process(command.Command, command.Kind, bytes, client);
                    }
                };
            });
        }
    }
}
