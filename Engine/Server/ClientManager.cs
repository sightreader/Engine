using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace SightReader.Engine.Server
{
    public class ClientManager
    {
        private IDictionary<Guid, Client> ClientsById { get; set; }

        public ClientManager()
        {
            ClientsById = new Dictionary<Guid, Client>();
        }

        public int Count
        {
            get
            {
                return ClientsById.Values.Count;
            }
        }

        public Client? FindById(Guid clientId)
        {
            if (!ClientsById.ContainsKey(clientId))
            {
                return null;
            }

            return ClientsById[clientId];
        }

        public void Register(Client client)
        {
            ClientsById[client.Id] = client;
            Log.Debug($"{client.Id}: [Register].");
        }

        public void Unregister(Client client)
        {
            ClientsById.Remove(client.Id);
        }
    }
}
