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
        #region CONSTANTS
        const float WAND_MARKER_SIZE = 39f;
        const float GROUND_MARKER_SIZE = 55f;
        const float PLANAR_THICKNESS = 4f;
        const float COURT_WIDTH = 5 * GROUND_MARKER_SIZE;
        const float COURT_LENGTH = 7 * GROUND_MARKER_SIZE;
        const float PADDLE_ANGLE = 25f;
        const float SLIME_HEIGHT = 15f;
        const float BALL_INIT_HEIGHT = 4 * GROUND_MARKER_SIZE;
        const float PHYSICS_SPEED = 1 / 15f;
        const float GRAVITY = 30f;
        Vector3 PLAYER_BALL_START = new Vector3(0, -2 * GROUND_MARKER_SIZE, BALL_INIT_HEIGHT);
        Vector3 OPPONENT_BALL_START = new Vector3(0, 2 * GROUND_MARKER_SIZE, BALL_INIT_HEIGHT);
        Vector3 PLAYER_SLIME_START = new Vector3(0, -2 * GROUND_MARKER_SIZE, SLIME_HEIGHT);
        Vector3 OPPONENT_SLIME_START = new Vector3(0, 2 * GROUND_MARKER_SIZE, SLIME_HEIGHT);
        #endregion CONSTANTS

        enum game_types { single, leader, opponent };
        int selected_game_type;
        Scene scene;
        SpriteFont font;
        bool betterFPS = true;
        bool paused = true;

        SoundEffect bounceSound;

        #region NODES
        CameraNode cameraNode;
        Viewport viewport;

        MarkerNode ground_marker_node;
        MarkerNode player_marker_node;

        Court court;
        VolleyBall vball;
        Target target;
        Paddle player_slime;
        Paddle opponent_slime;
        //Slime player_slime;
        //Slime opponent_slime;
        #endregion NODES

        int playerOneScore = 0;
        int playerTwoScore = 0;

        #region NETWORKING_VARS
        const int listening_port = 9002;
        const string listening_ip = "160.39.234.102";
        string opponent_ip;
        string gameID;
        Socket receiverSocket;
        Socket senderSocket;
        bool sendData = true;
        int seq = 0;
        int receive_seq = 0;
        #endregion NETWORKING_VARS

        public Engine(string gameType, string gameID, string opponent_ip) 
        {
            this.gameID = gameID;
            if (this.gameID == "")
                this.gameID = "0";
            this.opponent_ip = opponent_ip;

            if (!gameType.Equals("single"))
            {
                #region START_NETWORKING
                receiverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                SocketAsyncEventArgs sendDataEventArg = new SocketAsyncEventArgs();
                sendDataEventArg.RemoteEndPoint = new DnsEndPoint(listening_ip, listening_port);
                byte[] payload = Encoding.UTF8.GetBytes("receiver initializing");
                sendDataEventArg.SetBuffer(payload, 0, payload.Length);
                receiverSocket.SendToAsync(sendDataEventArg);
                
                Thread.Sleep(1000);

                SocketAsyncEventArgs receiveEventArg = new SocketAsyncEventArgs();
                receiveEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Any, listening_port);
                receiveEventArg.SetBuffer(new byte[1024], 0, 1024);
                receiveEventArg.Completed += receiveEventArg_Completed;
                receiverSocket.ReceiveFromAsync(receiveEventArg);
                #endregion START_NETWORKING
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

        #region NETWORKING_METHODS

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
                                if (opponent_slime != null)
                                    opponent_slime.setTranslation(new Vector3(opp_y, -1 * opp_x, opp_z));
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
                                vball.setTranslation(new Vector3(ball_x, -1 * ball_y, ball_z));
                                opponent_slime.setTranslation(new Vector3(opp_y, -1 * opp_x, opp_z));
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
            sendDataEventArg.RemoteEndPoint = new DnsEndPoint(listening_ip, listening_port);
            byte[] payload = Encoding.UTF8.GetBytes(getDataString());
            sendDataEventArg.SetBuffer(payload, 0, payload.Length);
            senderSocket.SendToAsync(sendDataEventArg);
        }

        #endregion NETWORKING_METHODS

        /// <summary>
        /// Stops running physics, which should stop the ball.
        /// </summary>
        public void pause()
        {
            if (!paused)
            {
                paused = true;
                ((MataliPhysics)scene.PhysicsEngine).SimulationTimeStep = 0f;
            }
        }

        /// <summary>
        /// Resumes running physics, which should reanimate the ball.
        /// </summary>
        public void resume()
        {
            if (paused)
            {
                paused = false;
                ((MataliPhysics)scene.PhysicsEngine).SimulationTimeStep = PHYSICS_SPEED;
            }
        }

        /// <summary>
        /// Getter method for the paused property of the engine.
        /// </summary>
        /// <returns></returns>
        public bool isPaused()
        {
            return paused;
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

            if (selected_game_type != (int)game_types.opponent)
            {
                scene.PhysicsEngine = new MataliPhysics();
                scene.PhysicsEngine.Gravity = GRAVITY;
                scene.PhysicsEngine.GravityDirection = new Vector3(0, 0, -1);
                ((MataliPhysics)scene.PhysicsEngine).SimulationTimeStep = PHYSICS_SPEED;
            }

            // Set up the lights used in the scene
            CreateLights();

            CreateCamera();

            SetupMarkerTracking(videoBrush);

            createObjects();

            if (paused)
                pause();

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

            InvisibleWall invisWallOne = new InvisibleWall(float.MaxValue, new Vector3(COURT_WIDTH, PLANAR_THICKNESS, COURT_LENGTH));
            InvisibleWall invisWallTwo = new InvisibleWall(float.MaxValue, new Vector3(PLANAR_THICKNESS, COURT_LENGTH, COURT_LENGTH));
            InvisibleWall invisWallThree = new InvisibleWall(float.MaxValue, new Vector3(COURT_WIDTH, PLANAR_THICKNESS, COURT_LENGTH));
            InvisibleWall invisWallFour = new InvisibleWall(float.MaxValue, new Vector3(PLANAR_THICKNESS, COURT_LENGTH, COURT_LENGTH));

            invisWallOne.translate(new Vector3(0, COURT_LENGTH / 2, COURT_LENGTH / 2));
            invisWallTwo.translate(new Vector3(-(COURT_WIDTH / 2), 0, COURT_LENGTH / 2));
            invisWallThree.translate(new Vector3(0, -(COURT_LENGTH / 2), COURT_LENGTH / 2));
            invisWallFour.translate(new Vector3(COURT_WIDTH / 2, 0, COURT_LENGTH / 2));

            // Add the walls onto the ground marker
            ground_marker_node.AddChild(invisWallOne.getTransformNode());
            ground_marker_node.AddChild(invisWallTwo.getTransformNode());
            ground_marker_node.AddChild(invisWallThree.getTransformNode());
            ground_marker_node.AddChild(invisWallFour.getTransformNode());

            // Create the court
            court = new Court(float.MaxValue, new Vector3(COURT_WIDTH, COURT_LENGTH, PLANAR_THICKNESS));
            // Initial translation
            court.translate(new Vector3(0, 0, PLANAR_THICKNESS / 2));
            // Add it to the scene
            ground_marker_node.AddChild(court.getTransformNode());

            // Time to create the net
            Net net = new Net(float.MaxValue, new Vector3(COURT_WIDTH, PLANAR_THICKNESS, GROUND_MARKER_SIZE));
            // Initial translation
            net.translate(new Vector3(0, 0, 1f + (GROUND_MARKER_SIZE / 2)));
            // Add it to the scene
            ground_marker_node.AddChild(net.getTransformNode());

            // Lets create the all important volleyball
            if (selected_game_type == (int)game_types.opponent)
                vball = new VolleyBall(1f, GROUND_MARKER_SIZE / 2f, bounceSound, true);
            else
                vball = new VolleyBall(1f, GROUND_MARKER_SIZE / 2f, bounceSound, false);
            // Perform initial manipulations
            vball.translate(PLAYER_BALL_START);
            // Add it to the scene
            ground_marker_node.AddChild(vball.getTransformNode());
            
            // Create a marker node to track the paddle
            player_marker_node = new MarkerNode(scene.MarkerTracker, "id511.xml", NyARToolkitTracker.ComputationMethod.Average);
            scene.RootNode.AddChild(player_marker_node);
            
            // Create the slime for the player
            player_slime = new Paddle(float.MaxValue, new Vector3(WAND_MARKER_SIZE * 1.5f, WAND_MARKER_SIZE * 1.5f, 3f), Color.Red.ToVector4());
            // Initial translation
            player_slime.translate(PLAYER_SLIME_START);
            player_slime.setRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(-PADDLE_ANGLE)));
            // Add it to the scene
            ground_marker_node.AddChild(player_slime.getTransformNode());

            // Create the slime for the opponent
            opponent_slime = new Paddle(float.MaxValue, new Vector3(WAND_MARKER_SIZE  * 1.5f, WAND_MARKER_SIZE * 1.5f, 3f), Color.Purple.ToVector4());
            // Initial translation
            opponent_slime.translate(OPPONENT_SLIME_START);
            opponent_slime.setRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(PADDLE_ANGLE)));
            // Add it to the scene
            ground_marker_node.AddChild(opponent_slime.getTransformNode());
        }

        private void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>("font");
            bounceSound = content.Load<SoundEffect>("rubber_ball_01");
        }

        private void resetRound()
        {
            court.reset_point_scored();
            ground_marker_node.RemoveChild(vball.getTransformNode());
            vball = new VolleyBall(1f, GROUND_MARKER_SIZE / 2f, bounceSound, false);
            // Perform initial manipulations
            vball.translate(PLAYER_BALL_START);
            //player_slime.setTranslation(PLAYER_SLIME_START);
            //opponent_slime.setTranslation(OPPONENT_SLIME_START);
            ground_marker_node.AddChild(vball.getTransformNode());
        }

        public void Dispose()
        {
            scene.Dispose();
        }

        public void Update(TimeSpan elapsedTime, bool isActive)
        {
            scene.Update(elapsedTime, false, isActive);
        }

        private void writeText(string s)
        {
            UI2DRenderer.WriteText(new Vector2(0, 5), s, Color.White, font,
                GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.Top + 80);
        }

        private void writeText(string s, int offset)
        {
            UI2DRenderer.WriteText(new Vector2(0, 5 + offset), s, Color.White, font,
                GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.Top + 80);
        }

        public void Draw(TimeSpan elapsedTime)
        {
            // send over your information
            if (ground_marker_node.MarkerFound)
            {
                if (sendData && selected_game_type != (int)game_types.single && senderSocket != null)
                    sendOutData();
                sendData = !sendData;
            }
            else
            {
                if (!paused)
                    pause();
            }

            State.Device.Viewport = viewport;

            if (player_marker_node.MarkerFound)
            {
                Vector3 wand_node_translation = player_marker_node.WorldTransformation.Translation;
                player_slime.setTranslation(wand_node_translation);
            }

            if (selected_game_type == (int)game_types.single)
            {
                TransformNode slimeAI_TransformNode = opponent_slime.getTransformNode();
                Vector3 vballPosition = vball.getWorldTransformationTranslation();

                Random rand = new Random();
                int i = rand.Next(3);
                int b = rand.Next(3);
                if (vballPosition.Y > 0)
                {
                    //near post
                    if (vballPosition.Y < 10)
                        slimeAI_TransformNode.Translation = new Vector3(vballPosition.X + i,
                        vballPosition.Y + b + 20, slimeAI_TransformNode.Translation.Z);
                    else
                        slimeAI_TransformNode.Translation = new Vector3(vballPosition.X + i,
                            vballPosition.Y + b + 5, slimeAI_TransformNode.Translation.Z);
                }
            }
            if (court.is_point_scored())
            {
                Vector3 vballPosition = vball.getWorldTransformationTranslation();
                if (vballPosition.Y < 0)
                    playerOneScore++;
                else
                    playerTwoScore++;
                resetRound();
            }
            writeText(playerOneScore + " - " + playerTwoScore);
            if (player_marker_node.MarkerFound)
                writeText("I CAN SEE YOU!", 40);
            else
                writeText("WHERE THE FUCK?", 40);
            try
            {
                scene.Draw(elapsedTime, false);
            }
            catch (Exception exp) { }
        }
    }
}
