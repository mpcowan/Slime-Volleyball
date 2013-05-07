using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Slime_Engine;


namespace Slime_Volleyball
{
    public partial class Game_Page : PhoneApplicationPage
    {
        ApplicationBar appbar;
        ApplicationBarIconButton pause_btn;
        VibrateController haptic;

        ContentManager contentManager;
        GameTimer timer;

        Engine engine;

        // For rendering the XAML onto a texture
        UIElementRenderer elementRenderer;

        public Game_Page()
        {
            haptic = VibrateController.Default;

            InitializeComponent();

            #region AppBar Creation

            appbar = new ApplicationBar();
            appbar.IsVisible = true;
            appbar.Opacity = 1;
            appbar.IsMenuEnabled = false;

            pause_btn = new ApplicationBarIconButton();
            pause_btn.Text = "Resume";
            pause_btn.IconUri = new Uri("/Images/appbar.control.play.png", UriKind.Relative);
            ApplicationBarIconButton quit_btn = new ApplicationBarIconButton();
            quit_btn.Text = "Quit";
            quit_btn.IconUri = new Uri("/Images/appbar.close.png", UriKind.Relative);
            ApplicationBarIconButton help_btn = new ApplicationBarIconButton();
            help_btn.Text = "Help";
            help_btn.IconUri = new Uri("/Images/appbar.question.png", UriKind.Relative);

            appbar.Buttons.Add(help_btn);
            appbar.Buttons.Add(quit_btn);
            appbar.Buttons.Add(pause_btn);
            
            help_btn.Click += help_btn_Click;
            quit_btn.Click += quit_btn_Click;
            pause_btn.Click += pause_btn_Click;

            this.ApplicationBar = appbar;

            #endregion

            // Get the content manager from the application
            contentManager = (Application.Current as App).Content;

            // Create a timer for this page
            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;

            LayoutUpdated += new EventHandler(GamePage_LayoutUpdated);

            // Disable the auto-sleep mode so that the app will be kept alive even when the
            // user is not interacting with the device
            Microsoft.Phone.Shell.PhoneApplicationService.Current.UserIdleDetectionMode =
                Microsoft.Phone.Shell.IdleDetectionMode.Disabled;
        }

        void GamePage_LayoutUpdated(object sender, EventArgs e)
        {
            // Create the UIElementRenderer to draw the XAML page to a texture.

            // Check for 0 because when we navigate away the LayoutUpdate event
            // is raised but ActualWidth and ActualHeight will be 0 in that case.
            if (ActualWidth > 0 && ActualHeight > 0 && elementRenderer == null)
            {
                elementRenderer = new UIElementRenderer(this, (int)640, (int)480);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            if (App._socket != null)
                App._socket.Close();

            string gameType = "";

            if (NavigationContext.QueryString.TryGetValue("type", out gameType))
                engine = new Engine(gameType, App.gameID, App.opponent_ip);
            else
                engine = new Engine("single", "0", App.opponent_ip);

            engine.Initialize(SharedGraphicsDeviceManager.Current, contentManager, viewfinderBrush);

            // Start the timer
            timer.Start();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            timer.Stop();

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            engine.Dispose();

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Allows the page to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            engine.Update(e.ElapsedTime, this.IsEnabled);
        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            // Render the Silverlight controls using the UIElementRenderer
            elementRenderer.Render();

            if (engine.VideoBackground == null)
                engine.VideoBackground = elementRenderer.Texture;

            engine.Draw(e.ElapsedTime);
        }

        void help_btn_Click(object sender, EventArgs e)
        {
            if (!engine.isPaused(0))
            {
                pause_btn.Text = "Resume";
                pause_btn.IconUri = new Uri("Images/appbar.control.play.png", UriKind.Relative);
                engine.pause(0);
            }
            MessageBox.Show("TODO, add all of the help text...", "Help + Info", MessageBoxButton.OK);
        }

        void quit_btn_Click(object sender, EventArgs e)
        {
            if (!engine.isPaused(0))
            {
                Dispatcher.BeginInvoke(() =>
                    {
                        pause_btn.Text = "Resume";
                        pause_btn.IconUri = new Uri("Images/appbar.control.play.png", UriKind.Relative);
                        engine.pause(0);
                    });
            }
            MessageBoxResult user_resp = MessageBox.Show("Quit the game?", "Quit?", MessageBoxButton.OKCancel);
            if (user_resp == MessageBoxResult.OK)
                engine.quit();
        }

        void pause_btn_Click(object sender, EventArgs e)
        {
            if (engine.isPaused(0))
            {
                pause_btn.Text = "Pause";
                pause_btn.IconUri = new Uri("Images/appbar.control.pause.png", UriKind.Relative);
                engine.resume(0);
            }
            else
            {
                pause_btn.Text = "Resume";
                pause_btn.IconUri = new Uri("Images/appbar.control.play.png", UriKind.Relative);
                engine.pause(0);
            }
        }

        /// <summary>
        /// Provide haptic feedback to the user by vibrating the phone. Specify the length of
        /// the vibration in milliseconds.
        /// </summary>
        /// <param name="duration">Duration of haptic feedback in milliseconds.</param>
        public void hapticFeedback(int duration)
        {
            haptic.Start(TimeSpan.FromMilliseconds(duration));
        }
    }
}