using System;
using System.Collections.Generic;
using System.Text;
using Config.Net;

namespace SightReader.Engine.Introducer
{    public interface IConfig
    {
        [Option(DefaultValue = "relay.sightread.xyz")]
        string Host { get; set; }

        [Option(DefaultValue = 55367)]
        int Port { get; set; }

        /// <summary>
        /// Specifies whether this machine's LAN IP will be sent to the
        /// public Introducer service for peer discovery.
        /// </summary>
        [Option(DefaultValue = true)]
        bool UseIntroducer { get; set; }
    }
}
 