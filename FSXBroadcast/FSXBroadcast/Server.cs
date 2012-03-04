using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace FSXBroadcast
{
    public delegate void ServerDelegate();

    class Server
    {
        const int SERVER_PORT = 4020;

        private TcpListener server = null;
        private List<Client> clients;

        public event ServerDelegate ClientCountChanged;

        public Server()
        {
            server = new TcpListener(IPAddress.Any, SERVER_PORT);
            this.clients = new List<Client>();
        }

        public void Start()
        {
            server.Start();
            server.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
        }

        public void Stop()
        {
            server.Stop();
            lock (this.clients)
            {
                foreach (Client client in this.clients)
                {
                    client.TcpClient.Client.Disconnect(false);
                }
                this.clients.Clear();
            }
        }

        public int ClientCount()
        {
            return this.clients.Count;
        }

        public void Write(string message)
        {
            byte[] bytes = Encoding.Default.GetBytes(string.Format("{0}\n", message));

            foreach (Client client in this.clients)
            {
                NetworkStream stream = client.TcpClient.GetStream();
                stream.BeginWrite(bytes, 0, bytes.Length, WriteCallback, client);
            }
        }

        private void WriteCallback(IAsyncResult result)
        {
            Client client = result.AsyncState as Client;
            NetworkStream stream = client.TcpClient.GetStream();
            stream.EndWrite(result);
        }

        private void AcceptTcpClientCallback(IAsyncResult result)
        {
            TcpClient tcpClient = server.EndAcceptTcpClient(result);
            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
            Client client = new Client(tcpClient, buffer);
            lock (this.clients)
            {
                this.clients.Add(client);
            }
            ClientCountChanged();
            NetworkStream networkStream = client.NetworkStream;
            networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
            server.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
        }
      
        private void ReadCallback(IAsyncResult result)
        {
            Client client = result.AsyncState as Client;
            if (client == null) return;
            NetworkStream networkStream = client.NetworkStream;
            int read = networkStream.EndRead(result);
            if (read == 0)
            {
                lock (this.clients)
                {
                    this.clients.Remove(client);
                    ClientCountChanged();
                    return;
                }
            }
            string data = Encoding.Default.GetString(client.Buffer, 0, read);
            //Do something with the data object here.
            networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
        }
    }

    internal class Client
    {
        public Client(TcpClient tcpClient, byte[] buffer)
        {
            if (tcpClient == null) throw new ArgumentNullException("tcpClient");
            if (buffer == null) throw new ArgumentNullException("buffer");
            this.TcpClient = tcpClient;
            this.Buffer = buffer;
        }

        public TcpClient TcpClient { get; private set; }

        public byte[] Buffer { get; private set; }

        public NetworkStream NetworkStream { get { return TcpClient.GetStream(); } }
    }
}
