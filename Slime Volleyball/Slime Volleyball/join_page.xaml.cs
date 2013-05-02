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
    public partial class join_page : PhoneApplicationPage
    {
        public join_page()
        {
            InitializeComponent();
        }

        private void join_btn_Click(object sender, RoutedEventArgs e)
        {
            if (gameID_tb.Text != "" && !gameID_tb.Text.Contains('.'))
            {
                try
                {
                    int gameID = Convert.ToInt32(gameID_tb.Text);
                    connect_to_game_server("160.39.234.102", 9001);
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Please ensure that you entered your game ID number first", "Woops", MessageBoxButton.OK);
                }
            }
            else
                MessageBox.Show("Please ensure that you entered your game ID number first", "Woops", MessageBoxButton.OK);
        }

        private void connect_to_game_server(string ip, int port)
        {
            DnsEndPoint game_server = new DnsEndPoint(ip, port);

            // This will set up a TCP Socket, hence SocketType.Stream and Protocol Tcp
            // We may want to switch to UDP which is SocketType.Dgram and ProtocolType.Udp
            App._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs connectionEventArg = new SocketAsyncEventArgs();
            connectionEventArg.RemoteEndPoint = game_server;
            connectionEventArg.Completed += connectionEventArg_Completed;

            App._socket.ConnectAsync(connectionEventArg);
        }

        void connectionEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (App._socket != null && e.SocketError == SocketError.Success)
            {
                SocketAsyncEventArgs sendDataEventArg = new SocketAsyncEventArgs();
                sendDataEventArg.RemoteEndPoint = App._socket.RemoteEndPoint;
                sendDataEventArg.UserToken = null;
                sendDataEventArg.Completed += sendDataEventArg_Completed;

                // Add the data to be send into the buffer
                Dispatcher.BeginInvoke(() =>
                {
                    byte[] payload = Encoding.UTF8.GetBytes("join " + gameID_tb.Text);
                    sendDataEventArg.SetBuffer(payload, 0, payload.Length);
                    App._socket.SendAsync(sendDataEventArg);
                });
            }
            else
            {
                MessageBox.Show("Unable to contact the game server. Try again later.", "Woops :(", MessageBoxButton.OK);
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
                MessageBox.Show("Unable to send the join command to the server. Try again later.", "Woops :(", MessageBoxButton.OK);
            }
        }

        void receiveDataEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Retrieve the data from the buffer
                string response = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                response = response.Trim('\0');

                if (!response.Equals("ERROR"))
                {
                    // Everything is a go on the serverside, it is time to start the game
                    Dispatcher.BeginInvoke(() =>
                    {
                        App.gameID = gameID_tb.Text;
                        App.opponent_ip = response;
                        NavigationService.Navigate(new Uri("/Game_Page.xaml?type=opponent", UriKind.Relative));
                    });
                }
                else
                {
                    MessageBox.Show("Unable to join specified game ID. Try again later.", "Woops :(", MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show("Unable to join specified game ID. Try again later.", "Woops :(", MessageBoxButton.OK);
            }
        }
    }
}