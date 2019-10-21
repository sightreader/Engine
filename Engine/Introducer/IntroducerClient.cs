using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;

namespace SightReader.Engine.Introducer
{
    public class IntroducerClient
    {
        private WebsocketClient client;
        private IConfig config;

        public IntroducerClient(IConfig config)
        {
            this.config = config;
            client = new WebsocketClient(new Uri($"wss://{config.Host}:{config.Port}"), new Func<ClientWebSocket>(() => new ClientWebSocket
            {
                Options =
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(5),
                }
            }));
        }

        private IPAddress GetLanIpAddress()
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;

            if (endPoint == null)
            {
                throw new Exception("Unable to get LAN IP address.");
            }
            return endPoint.Address;
        }

        public async Task RegisterAsync()
        {
            if (!config.UseIntroducer)
            {
                Log.Debug($"Introducer peer discovery service will not be used.");
                return;
            }

            client.Name = "Introducer";
            client.ReconnectTimeoutMs = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
            client.ErrorReconnectTimeoutMs = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
            client.ReconnectionHappened.Subscribe(async type =>
            {
                Log.Information($"Reconnection happened, type: {type}, url: {client.Url}");
                Log.Debug($"[Introducer] Connected: {type} {client.Url}");
                var command = $"update-role|interpreter|{GetLanIpAddress().MapToIPv4().ToString()}";
                Log.Debug($"[Introducer] Sending message: {command}");
                await client.Send(command);
            });
            client.DisconnectionHappened.Subscribe(type => {
                Log.Debug($"[Introducer] Disconnected, type {type}.");
            });

            client.MessageReceived.Subscribe(msg =>
            {
                Log.Debug($"[Introducer] Message received: {msg}");
            });

            await client.Start();
        }

        public void Disconnect()
        {
            client.Dispose();
        }
    }
}
