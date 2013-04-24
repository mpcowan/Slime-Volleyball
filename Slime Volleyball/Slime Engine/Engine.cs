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
#endregion

namespace Slime_Engine
{
    public class Engine
    {
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

        AR_Object ball;
        AR_Object player_slime;
        AR_Object opponent_slime;

        public Engine() { }

        public Texture2D VideoBackground
        {
            get { return scene.BackgroundTexture; }
            set { scene.BackgroundTexture = value; }
        }

        public void Initialize(IGraphicsDeviceService service, ContentManager content, VideoBrush videoBrush)
        {
            //viewport = new Viewport(0, 0, 640, 480);
            viewport = new Viewport(0, 0, 750, 480);
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

        private void loadModels()
        {


        }

        private void createObjects()
        {
            // Create a marker node to track the ground array
            ground_marker_node = new MarkerNode(scene.MarkerTracker, "SlimeGroundArray.xml", NyARToolkitTracker.ComputationMethod.Average);
            scene.RootNode.AddChild(ground_marker_node);

            // Create some physical ground object
            Court court = new Court(float.MaxValue, new Vector3(4 * groundNodeSize, 6 * groundNodeSize, 2f), bounceSound);

            // Add it to the scene
            ground_marker_node.AddChild(court.getTransformNode());

            // Lets create the all important volleyball
            vball = new VolleyBall(1f, groundNodeSize / 2f, bounceSound);
            // Perform initial manipulations
            vball.translate(new Vector3(0, 0, 4 * groundNodeSize));
            // Add it to the scene
            ground_marker_node.AddChild(vball.getTransformNode());

            // Create a marker node to track the paddle
            player_marker_node = new MarkerNode(scene.MarkerTracker, "id511.xml", NyARToolkitTracker.ComputationMethod.Average);
            scene.RootNode.AddChild(player_marker_node);

            // For simplicity sake, start with paddles
            player_slime = new AR_Object("cube", true, 1000f, null);
            // Perform initial manipulations
            player_slime.translate(new Vector3(0, 0, 0));
            //player_slime.scaleToSize(wandSize);
            player_slime.scaleX(wandSize * 2);
            player_slime.scaleY(wandSize * 2);
            player_slime.scaleZ(2f);

            // Add it to the scene
            player_marker_node.AddChild(player_slime.getTransformNode());
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
            State.Device.Viewport = viewport;
            //Draw our text string at top center of screen
            UI2DRenderer.WriteText(new Vector2(0, 50), vball.nodeTranslationToString(), Color.White, font,
                GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.Top + 80);
            try
            {
                scene.Draw(elapsedTime, false);
            }
            catch (Exception exp) { }
        }
	
    }
}
