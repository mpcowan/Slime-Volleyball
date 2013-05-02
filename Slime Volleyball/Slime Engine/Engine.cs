#region Using Statements
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Matrix = Microsoft.Xna.Framework.Matrix;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Generic;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Helpers;
using GoblinXNA.UI;
using GoblinXNA.UI.UI2D;
using System.Windows.Media;
using GoblinXNA.Physics.Matali;
using GoblinXNA.Physics;
using MataliPhysicsObject = Komires.MataliPhysics.PhysicsObject;
using GoblinXNA.Sounds;
using GoblinXNA.UI.UI3D;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Net;
#endregion

namespace Slime_Engine
{
    public class Engine
    {
        enum game_types { single, leader, opponent };
        int selected_game_type;
        Scene scene;
        SpriteFont font;
        bool betterFPS = true;

        SoundEffect bounceSound;

        CameraNode cameraNode;
        Viewport viewport;

        MarkerNode ground_marker_node;
        MarkerNode player_marker_node;

        float wandSize = 39f;
        float groundNodeSize = 55f;

        VolleyBall vball;
        Paddle player_slime;
        Paddle opponent_slime;
        //Slime player_slime;
        //Slime opponent_slime;

        Vector3 prior_wand_movement;
        bool first_find = true;

        // Networking vars
        const int listening_port = 9002;
        string opponent_ip;
        string gameID;
        Socket receiverSocket;
        Socket senderSocket;
        bool sendData = true;
        int seq = 0;
        int receive_seq = 0;

        public Engine(string gameType, string gameID, string opponent_ip) 
        {
            this.gameID = gameID;
            if (this.gameID == "")
                this.gameID = "0";
            this.opponent_ip = opponent_ip;
            if (!gameType.Equals("single"))
            {
                receiverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                SocketAsyncEventArgs sendDataEventArg = new SocketAsyncEventArgs();
                sendDataEventArg.RemoteEndPoint = new DnsEndPoint("160.39.234.102", 9002);
                byte[] payload = Encoding.UTF8.GetBytes("receiver initializing");
                sendDataEventArg.SetBuffer(payload, 0, payload.Length);
                receiverSocket.SendToAsync(sendDataEventArg);
                
                Thread.Sleep(1200);

                SocketAsyncEventArgs receiveEventArg = new SocketAsyncEventArgs();
                receiveEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Any, listening_port);
                receiveEventArg.SetBuffer(new byte[1024], 0, 1024);
                receiveEventArg.Completed += receiveEventArg_Completed;
                receiverSocket.ReceiveFromAsync(receiveEventArg);
            }

            if (gameType.Equals("single"))
                selected_game_type = (int)game_types.single;
            else if (gameType.Equals("leader"))
                selected_game_type = (int)game_types.leader;
            else if (gameType.Equals("opponent"))
                selected_game_type = (int)game_types.opponent;
            else
                selected_game_type = (int)game_types.single;
        }

        void receiveEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Retrieve the data from the buffer
                string response = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                response = response.Trim('\0');
                string[] response_parts = response.Split(' ');

                if (selected_game_type == (int)game_types.leader)
                {
                    if (response_parts.Length == 4)
                    {
                        try
                        {
                            int seq_num = Convert.ToInt32(response_parts[0]);
                            float opp_x = (float)Convert.ToDouble(response_parts[1]);
                            float opp_y = (float)Convert.ToDouble(response_parts[2]);
                            float opp_z = (float)Convert.ToDouble(response_parts[3]);

                            if (seq_num > receive_seq)
                            {
                                receive_seq = seq_num;
                                opponent_slime.setTranslation(new Vector3(opp_x, opp_y, opp_z));
                            }
                        }
                        catch (Exception exp)
                        {

                        }
                    }
                }
                else if (selected_game_type == (int)game_types.opponent)
                {
                    if (response_parts.Length == 7)
                    {
                        try
                        {
                            int seq_num = Convert.ToInt32(response_parts[0]);
                            float ball_x = (float)Convert.ToDouble(response_parts[1]);
                            float ball_y = (float)Convert.ToDouble(response_parts[2]);
                            float ball_z = (float)Convert.ToDouble(response_parts[3]);
                            float opp_x = (float)Convert.ToDouble(response_parts[4]);
                            float opp_y = (float)Convert.ToDouble(response_parts[5]);
                            float opp_z = (float)Convert.ToDouble(response_parts[6]);

                            if (seq_num > receive_seq)
                            {
                                receive_seq = seq_num;
                                vball.setTranslation(new Vector3(ball_x, ball_y, ball_z));
                                opponent_slime.setTranslation(new Vector3(opp_x, opp_y, opp_z));
                            }
                        }
                        catch (Exception exp)
                        {

                        }
                    }
                }
            }
            continueListening();
        }

        private void continueListening()
        {
            SocketAsyncEventArgs receiveEventArg = new SocketAsyncEventArgs();
            receiveEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Any, listening_port);
            receiveEventArg.SetBuffer(new byte[1024], 0, 1024);
            receiveEventArg.Completed += receiveEventArg_Completed;
            receiverSocket.ReceiveFromAsync(receiveEventArg);
        }

        public Texture2D VideoBackground
        {
            get { return scene.BackgroundTexture; }
            set { scene.BackgroundTexture = value; }
        }

        public void Initialize(IGraphicsDeviceService service, ContentManager content, VideoBrush videoBrush)
        {
            viewport = new Viewport(0, 0, 640, 480);
            //viewport = new Viewport(0, 0, 750, 480);
            viewport.MaxDepth = service.GraphicsDevice.Viewport.MaxDepth;
            viewport.MinDepth = service.GraphicsDevice.Viewport.MinDepth;
            service.GraphicsDevice.Viewport = viewport;

            // Initialize the GoblinXNA framework
            State.InitGoblin(service, content, "");

            LoadContent(content);

            // Initialize the scene graph
            scene = new Scene();
            scene.BackgroundColor = Color.Black;
            scene.PhysicsEngine = new MataliPhysics();
            scene.PhysicsEngine.Gravity = 40;
            scene.PhysicsEngine.GravityDirection = new Vector3(0, 0, -1);
            ((MataliPhysics)scene.PhysicsEngine).SimulationTimeStep = 1 / 30f;

            // Set up the lights used in the scene
            CreateLights();

            CreateCamera();

            SetupMarkerTracking(videoBrush);

            createObjects();

            State.ShowNotifications = true;
            Notifier.Font = font;

            State.ShowFPS = false;
        }

        private void CreateCamera()
        {
            // Create a camera 
            Camera camera = new Camera();
            // Put the camera at the origin
            camera.Translation = new Vector3(0, 0, 0);
            // Set the vertical field of view to be 60 degrees
            camera.FieldOfViewY = MathHelper.ToRadians(60);
            // Set the near clipping plane to be 0.1f unit away from the camera
            camera.ZNearPlane = 0.1f;
            // Set the far clipping plane to be 2000 units away from the camera
            camera.ZFarPlane = 2000;

            // Now assign this camera to a camera node, and add this camera node to our scene graph
            cameraNode = new CameraNode(camera);
            scene.RootNode.AddChild(cameraNode);

            // Assign the camera node to be our scene graph's current camera node
            scene.CameraNode = cameraNode;
        }

        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(0, 0, -1);
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            lightNode.AmbientLightColor = new Vector4(0.2f, 0.2f, 0.2f, 1);
            lightNode.LightSource = lightSource;

            scene.RootNode.AddChild(lightNode);
        }

        private void SetupMarkerTracking(VideoBrush videoBrush)
        {
            IVideoCapture captureDevice = null;

            captureDevice = new PhoneCameraCapture(videoBrush);
            captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._640x480, ImageFormat.B8G8R8A8_32, false);
            ((PhoneCameraCapture)captureDevice).UseLuminance = true;

            if (betterFPS)
                captureDevice.MarkerTrackingImageResizer = new HalfResizer();

            // Add this video capture device to the scene so that it can be used for the marker tracker
            scene.AddVideoCaptureDevice(captureDevice);

            NyARToolkitIdTracker tracker = new NyARToolkitIdTracker();

            if (captureDevice.MarkerTrackingImageResizer != null)
                tracker.InitTracker((int)(captureDevice.Width * captureDevice.MarkerTrackingImageResizer.ScalingFactor),
                    (int)(captureDevice.Height * captureDevice.MarkerTrackingImageResizer.ScalingFactor),
                    "camera_para.dat");
            else
                tracker.InitTracker(captureDevice.Width, captureDevice.Height, "camera_para.dat");

            // Set the marker tracker to use for our scene
            scene.MarkerTracker = tracker;
        }

        private void createObjects()
        {
            // Create a marker node to track the ground array
            ground_marker_node = new MarkerNode(scene.MarkerTracker, "SlimeGroundArray.xml", NyARToolkitTracker.ComputationMethod.Average);
            scene.RootNode.AddChild(ground_marker_node);

            // Create some physical ground object
            Court court = new Court(float.MaxValue, new Vector3(5 * groundNodeSize, 7 * groundNodeSize, 4f));
            // Initial translation
            court.translate(new Vector3(0, 0, 2f));
            // Add it to the scene
            ground_marker_node.AddChild(court.getTransformNode());

            // Time to create the net
            Net net = new Net(float.MaxValue, new Vector3(5 * groundNodeSize, 5f, groundNodeSize));
            // Initial translation
            net.translate(new Vector3(0, 0, 1f + (groundNodeSize / 2)));
            // Add it to the scene
            ground_marker_node.AddChild(net.getTransformNode());

            // Lets create the all important volleyball
            vball = new VolleyBall(1f, groundNodeSize / 2f, bounceSound);
            // Perform initial manipulations
            vball.translate(new Vector3(0, -2 * groundNodeSize, 4 * groundNodeSize));
            // Add it to the scene
            ground_marker_node.AddChild(vball.getTransformNode());
            
            // Create a marker node to track the paddle
            player_marker_node = new MarkerNode(scene.MarkerTracker, "id511.xml", NyARToolkitTracker.ComputationMethod.Average);
            scene.RootNode.AddChild(player_marker_node);
            
            // Create the slime for the player
            //player_slime = new Slime(100f, wandSize);
            player_slime = new Paddle(float.MaxValue, new Vector3(wandSize * 1.5f, wandSize * 1.5f, 3f));
            // Initial translation
            player_slime.translate(new Vector3(0, -1 * groundNodeSize, 12));
            player_slime.setRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(-20)));
            // Add it to the wand node
            //player_marker_node.AddChild(player_slime.getTransformNode());
            ground_marker_node.AddChild(player_slime.getTransformNode());

            // Create the slime for the opponent
            opponent_slime = new Paddle(float.MaxValue, new Vector3(wandSize  * 1.5f, wandSize * 1.5f, 3f));
            // Initial translation
            opponent_slime.translate(new Vector3(0, 2 * groundNodeSize, 12));
            opponent_slime.setRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(20)));
            // Add it to the scene
            ground_marker_node.AddChild(opponent_slime.getTransformNode());
        }

        private void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>("font");
            bounceSound = content.Load<SoundEffect>("rubber_ball_01");
        }

        public void Dispose()
        {
            scene.Dispose();
        }

        public void Update(TimeSpan elapsedTime, bool isActive)
        {
            scene.Update(elapsedTime, false, isActive);
        }

        public void Draw(TimeSpan elapsedTime)
        {
            if (ground_marker_node.MarkerFound)
            {
                if (sendData && selected_game_type != (int)game_types.single && senderSocket != null)
                    sendOutData();
            }

            sendData = !sendData;

            State.Device.Viewport = viewport;

            if (player_marker_node.MarkerFound)
            {
                Vector3 wand_node_translation = player_marker_node.WorldTransformation.Translation;
                if (first_find)
                {
                    prior_wand_movement = wand_node_translation;
                    first_find = false;
                }
                Vector3 delta = wand_node_translation - prior_wand_movement;
                prior_wand_movement = wand_node_translation;
                player_slime.translate(delta);
            }

            //Draw our text string at top center of screen
            //UI2DRenderer.WriteText(new Vector2(0, 10), ground_node_translation.ToString(), Color.White, font,
            //    GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.Top + 80);
            //UI2DRenderer.WriteText(new Vector2(0, 50), wand_node_translation.ToString(), Color.White, font,
            //    GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.Top + 80);
            //UI2DRenderer.WriteText(new Vector2(0, 90), delta.ToString(), Color.White, font,
            //    GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.Top + 80);
            try
            {
                scene.Draw(elapsedTime, false);
            }
            catch (Exception exp) { }
        }

        private string getDataString()
        {
            seq++;
            string dataString = gameID + " " + opponent_ip + " " + seq.ToString();
            //string dataString = gameID + " " + "self" + " " + seq.ToString();
            if (selected_game_type == (int)game_types.leader)
                dataString += " " + vball.nodeTranslationToString();
            dataString += " " + player_slime.nodeTranslationToString();
            return dataString;
        }

        private void sendOutData()
        {
            SocketAsyncEventArgs sendDataEventArg = new SocketAsyncEventArgs();
            sendDataEventArg.RemoteEndPoint = new DnsEndPoint("160.39.234.102", 9002);
            byte[] payload = Encoding.UTF8.GetBytes(getDataString());
            sendDataEventArg.SetBuffer(payload, 0, payload.Length);
            senderSocket.SendToAsync(sendDataEventArg);
        }
    }
}
