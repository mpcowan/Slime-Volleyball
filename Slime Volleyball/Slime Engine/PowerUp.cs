using GoblinXNA.Graphics;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Helpers;
using GoblinXNA.Physics.Matali;
using GoblinXNA.SceneGraph;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model = GoblinXNA.Graphics.Model;
using Komires.MataliPhysics;
using MataliPhysicsObject = Komires.MataliPhysics.PhysicsObject;
using GoblinXNA.Sounds;
using Microsoft.Xna.Framework.Audio;
using GoblinXNA.Physics;

namespace Slime_Engine
{
    class PowerUp : Virtual_Object
    {
        int powerUpType = 1;
        int powerDownType = 0;
        public bool isDeleted = false;
        int type;
        int disappearRate;
        int timeAlive = 0;
        string powerUpName;
        public bool affectsPlayerOne;
        bool removed = false;
        float size;

        private bool obtained = false;
        public int powerUpActive = 300;

        bool forceMyPowerUp = false;
        bool forceGood = false;


        public PowerUp(int number, int t, Vector3 s, int rate)
        {


            this.disappearRate = rate;
            this.type = t;
            if (forceGood)
                type = powerUpType;

            createObj(type, s);
            this.size = s.X;
            powerUpName = "powerup" + type + ' ' + number;
            applyPhysics(40, powerUpName, true, true, GoblinXNA.Physics.ShapeType.Box);
        }

        public bool deleted()
        {
            return isDeleted;
        }

        public void delete()
        {
            isDeleted = true;
        }

        public void setLocation(int corner, float width, float length)
        {
            Vector3 position = Vector3.Zero;
            int powerUpLength = (int)size;
            int netKeepAway = (int)4f / 2 + powerUpLength / 2;
            int x = (int)width / 2 - (netKeepAway * 3);
            int y = (int)length / 2 - (netKeepAway * 3);
            if (1 == corner)
                position = new Vector3(x, y, 40);
            if (2 == corner)
                position = new Vector3(-x, y, 40);
            if (3 == corner)
                position = new Vector3(x, -y, 40);
            if (4 == corner)
                position = new Vector3(-x, -y, 40);


            affectsPlayerOne = position.Y <= 0;
            translate(position);
        }

        public void randomLocation(float width, float length, float thickness)
        {
            Random rand = new Random();
            

            int powerUpLength = (int)size;
            int netKeepAway = (int)thickness / 2 + powerUpLength / 2;
            int x = (int)width / 2 - (netKeepAway * 3);
            int y = (int)length / 2 - (netKeepAway * 3);

            //int x = rand.Next(netKeepAway, (int)width / 2 - netKeepAway);
            //int y = rand.Next(netKeepAway, (int)length / 2 - powerUpLength / 2);

            int y_side = rand.Next(2);
            int x_side = rand.Next(2);

            if (0 == y_side) { y_side = -1; }
            if (0 == x_side) { x_side = -1; }
            //goodPowerUp.translate(new Vector3(x*x_side,y*y_side,0));
            y = y_side * y;
            x = x_side * x;
            if (forceMyPowerUp)
                y = Math.Abs(y) * -1;
            affectsPlayerOne = y <= 0;
            translate(new Vector3(x, y, 15)); //keep on my side for now

        }

        public float getPowerUpEffect()
        {
            if (type == powerUpType)
            {
                return 55f * 2;
            }
            else if (type == powerDownType)
            {
                return 55f / 2;
            }

            return 0; //do nothing
        }

        public int turnsAlive()
        {
            return timeAlive;
        }

        protected override void createObj() { }

        public void tick()
        {
            if (!obtained)
            {
                Vector4 color = geomNode.Material.Diffuse;
                float newAlphaChannel = color.W - color.W / disappearRate;
                Vector4 newColor = new Vector4(color.X, color.Y, color.Z, newAlphaChannel);
                geomNode.Material.Diffuse = newColor;
                geomNode.Material.Specular = newColor;
                timeAlive++;
            }
            else
            {
                timeAlive++;
            }
        }

        private void createObj(int type, Vector3 dim)
        {
            geomNode = new GeometryNode("powerup" + type);

            geomNode.Model = new Box(dim.X, dim.Y, dim.Z);

            // Create a material to apply to the wall
            Material powerMaterial = new Material();
            Vector4 color;
            if (powerUpType == type)
                color = Color.Black.ToVector4();
            else
                color = Color.DarkRed.ToVector4();

            powerMaterial.Diffuse = color;
            powerMaterial.Specular = color;
            powerMaterial.SpecularPower = 5;

            geomNode.Material = powerMaterial;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        public bool isObtained()
        {
            return obtained;
        }
        public bool isRemoved()
        {
            return removed;
        }

        public void remove()
        {
            removed = true;
        }

        protected override void obj_collision_start(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {
            baseObject.MainWorldTransform.SetLinearVelocity(Vector3.Zero);
            String materialName = ((IPhysicsObject)collidingObject.UserTagObj).MaterialName;
            if (materialName.Equals("slime"))
            {
                obtained = true;
                timeAlive = 0;
            }
        }
        protected override void obj_collision_done(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {
            String materialName = ((IPhysicsObject)collidingObject.UserTagObj).MaterialName;
            if (materialName.Equals("slime"))
            {
                obtained = true;
                timeAlive = 0;
            }
            baseObject.MainWorldTransform.SetLinearVelocity(Vector3.Zero);
        }
    }
}
