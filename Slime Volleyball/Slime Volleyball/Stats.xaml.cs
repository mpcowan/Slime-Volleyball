using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace Slime_Volleyball
{
    public partial class Stats : PhoneApplicationPage
    {
        string wins = "";
        string games = "";
        string sent = "";
        string recv = "";
        string total = "";

        public Stats()
        {
            InitializeComponent();
            load_data();
            convertToMega();
            display();
        }

        private void load_data()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("GamesWon"))
                wins = IsolatedStorageSettings.ApplicationSettings["GamesWon"].ToString();
            else
                wins = "0";
            if (IsolatedStorageSettings.ApplicationSettings.Contains("GamesPlayed"))
                games = IsolatedStorageSettings.ApplicationSettings["GamesPlayed"].ToString();
            else
                games = "0";

            if (IsolatedStorageSettings.ApplicationSettings.Contains("BytesSent"))
                sent = IsolatedStorageSettings.ApplicationSettings["BytesSent"].ToString();
            else
                sent = "0";
            if (IsolatedStorageSettings.ApplicationSettings.Contains("BytesRecv"))
                recv = IsolatedStorageSettings.ApplicationSettings["BytesRecv"].ToString();
            else
                recv = "0";
            total = (Convert.ToInt32(sent) + Convert.ToInt32(recv)).ToString();
        }

        private void convertToMega()
        {
            int s = Convert.ToInt32(sent);
            sent = ((double)s / 1048576.0).ToString("0.00") + " MB";

            int r = Convert.ToInt32(recv);
            recv = ((double)r / 1048576.0).ToString("0.00") + " MB";

            int t = Convert.ToInt32(total);
            total = ((double)t / 1048576.0).ToString("0.00") + " MB";
        }

        private void display()
        {
            victories_tb.Text = wins;
            games_tb.Text = games;
            sent_tb.Text = sent;
            received_tb.Text = recv;
            data_tb.Text = total;
        }
    }
}