using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace Slime_Volleyball
{
    public partial class Coop_Creation : PhoneApplicationPage
    {
        public Coop_Creation()
        {
            InitializeComponent();

            connect_to_game_server("160.39.234.102", 9001);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App._socket.Close();
            base.OnNavigatedFrom(e);
        }

        private void connect_to_game_server(string ip, int port)
        {
            DnsEndPoint game_server = new DnsEndPoint(ip, port);

            // This will set up a TCP Socket, hence SocketType.Stream and Protocol Tcp
            // We may want to switch to UDP which is SocketType.Dgram and ProtocolType.Udp
            App._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.RemoteEndPoint = game_server;
            socketEventArg.Completed += socketEventArg_Completed;

            App._socket.ConnectAsync(socketEventArg);
        }

        /// <summary>
        /// Method called upon completion of connection with game server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void socketEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (App._socket != null && e.SocketError == SocketError.Success)
            {
                SocketAsyncEventArgs sendDataEventArg = new SocketAsyncEventArgs();
                sendDataEventArg.RemoteEndPoint = App._socket.RemoteEndPoint;
                sendDataEventArg.UserToken = null;
                sendDataEventArg.Completed += sendDataEventArg_Completed;

                // Add the data to be send into the buffer
                byte[] payload = Encoding.UTF8.GetBytes("create");
                sendDataEventArg.SetBuffer(payload, 0, payload.Length);
                App._socket.SendAsync(sendDataEventArg);
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Unable to contact the game server. Try again later.", "Woops :(", MessageBoxButton.OK);
                    });
            }
        }

        void sendDataEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                SocketAsyncEventArgs receiveDataEventArg = new SocketAsyncEventArgs();
                receiveDataEventArg.RemoteEndPoint = App._socket.RemoteEndPoint;
                receiveDataEventArg.SetBuffer(new byte[512], 0, 512);
                receiveDataEventArg.Completed += receiveDataEventArg_Completed;

                App._socket.ReceiveAsync(receiveDataEventArg);
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Unable to send create command to game server. Try again later.", "Woops :(", MessageBoxButton.OK);
                    });
            }
        }

        void receiveDataEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Retrieve the data from the buffer
                string response = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                response = response.Trim('\0');
                try
                {
                    int gameID = Convert.ToInt32(response);
                    Dispatcher.BeginInvoke(() =>
                    {
                        gameID_tb.Text = response;
                        MessageBox.Show("Send your game room ID: " + response + " to your friend. Once they join the game you will be able to start the game", "Invite Your Friend", MessageBoxButton.OK);
                        App.gameID = response;
                    });
                    SocketAsyncEventArgs waitForServerOK = new SocketAsyncEventArgs();
                    waitForServerOK.RemoteEndPoint = App._socket.RemoteEndPoint;
                    waitForServerOK.SetBuffer(new byte[512], 0, 512);
                    waitForServerOK.Completed += waitForServerOK_Completed;

                    App._socket.ReceiveAsync(waitForServerOK);
                }
                catch (Exception exc)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Unable to receive a game room id from server. Try again later.", "Woops :(", MessageBoxButton.OK);
                    });
                }
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Unable to receive a game room id from server. Try again later.", "Woops :(", MessageBoxButton.OK);
                    });
            }
        }

        void waitForServerOK_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Retrieve the data from the buffer
                string response = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                response = response.Trim('\0');

                if (response.StartsWith("ERROR"))
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("It doesn't seem that your friend ever joined. Try again later.", "Woops :(", MessageBoxButton.OK);
                        NavigationService.GoBack();
                    });
                }
                else
                {

                    Dispatcher.BeginInvoke(() =>
                    {
                        App.opponent_ip = response;
                        start_btn.IsEnabled = true;
                    });
                }
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("It doesn't seem that your friend ever joined. Try again later.", "Woops :(", MessageBoxButton.OK);
                    });
            }
        }

        private void start_btn_Click(object sender, RoutedEventArgs e)
        {
            // Everything is a go on the serverside, it is time to start the game
            NavigationService.Navigate(new Uri("/Game_Page.xaml?type=leader", UriKind.Relative));
        }
    }
}