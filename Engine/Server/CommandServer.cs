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

        public void Run(IEngineContext engine)
        {
            SetupLogging();
            var config = LoadConfig();
            var clients = new ClientManager();
            var server = CreateWebsocketServer(config);
            var processor = new CommandProcessor(engine);

            server.Start(socket =>
            {
                socket.OnOpen = () =>
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
                    }
                };

                socket.OnClose = () =>
                {
                    Log.Debug($"{socket.ConnectionInfo.ClientIpAddress}: [Disconnected].");

                    var client = clients.FindById(socket.ConnectionInfo.Id);
                    Log.Debug($"{client.Id}: [Unregister].");
                    clients.Unregister(client);
                };

                socket.OnBinary = bytes =>
                {
                    var client = clients.FindById(socket.ConnectionInfo.Id);

                    var command = MessagePackSerializer.Deserialize<ICommand>(bytes);

                    processor.Process(command, client);
                };
            });
        }
    }
}
