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

        public Client FindById(Guid clientId)
        {
            return ClientsById[clientId];
        }

        public void Register(Client client)
        {
            ClientsById[client.Id] = client;
        }

        public void Unregister(Client client)
        {
            ClientsById.Remove(client.Id);
        }
    }
}
