using System;
using System.Collections.Generic;
using System.Text;
using Config.Net;

namespace SightReader.Engine.Server
{    public interface IConfig
    {
        [Option(DefaultValue = 55367)]
        int Port { get; set; }

        /// <summary>
        /// The maximum number of incoming websocket connections allowed before
        /// connections are dropped.
        /// </summary>
        [Option(DefaultValue = 10)]
        int MaxWebsocketConnections { get; set; }

        /// <summary>
        /// Shared secret to decrypt/encrypt initial websocket traffic.
        /// </summary>
        [Option(DefaultValue = "skirt multiple enlarging update yearbook why")]
        string SharedSecret { get; set; }
    }
}
 