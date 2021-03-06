﻿using Fleck;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SightReader.Engine.Server
{
    public class Client
    {
        /// <summary>
        /// The underlying websocket connection.
        /// </summary>
        public IWebSocketConnection? Socket { get; set; }

        public Guid Id => Socket == null ? Guid.Empty : Socket.ConnectionInfo.Id;

        public bool Equals(Client x, Client y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            if (x.Socket == null || y.Socket == null)
            {
                return false;
            }

            return x.Socket.ConnectionInfo.Id == y.Socket.ConnectionInfo.Id;
        }

        public int GetHashCode(Client client)
        {
            if (Object.ReferenceEquals(client, null)) return 0;

            return client.Socket == null ? -1 : client.Socket.ConnectionInfo.Id.GetHashCode();
        }
    }
}
