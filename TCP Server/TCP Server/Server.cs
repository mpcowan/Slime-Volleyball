using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace TCP_Server
{
    class Server
    {
        private TcpListener listener;
        private Thread listen_thread;

        public Server(int port)
        {
            this.listener = new TcpListener(port);
            this.listen_thread = new Thread(new ThreadStart(ListenForClients));
            this.listen_thread.Start();
        }

        private void ListenForClients()
        {
            this.listener.Start();
            while (true)
            {
                // blocks until a client has connected to the server
                TcpClient client = this.listener.AcceptTcpClient();

                // create a thread to handle communication with connected client
                Thread client_thread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                client_thread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
            }

            tcpClient.Close();
        }
    }
}
