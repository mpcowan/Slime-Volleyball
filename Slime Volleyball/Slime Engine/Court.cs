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
    class Court : Virtual_Object
    {
        private bool point_scored;

        public Court(float mass, Vector3 size) : base()
        {
            point_scored = false;
            createObj(size);
            applyPhysics(mass, "court", true, false, GoblinXNA.Physics.ShapeType.Box);
        }

        public bool is_point_scored()
        {
            return point_scored;
        }

        public void reset_point_scored()
        {
            point_scored = false;
        }

        protected override void createObj() { }

        private void createObj(Vector3 dim)
        {
            geomNode = new GeometryNode("court");

            geomNode.Model = new Box(dim.X, dim.Y, dim.Z);

            // Create a material to apply to the court
            Material courtMaterial = new Material();
            courtMaterial.Diffuse = Color.DarkGreen.ToVector4();
            courtMaterial.Specular = Color.DarkGreen.ToVector4();
            courtMaterial.SpecularPower = 0.3f;

            geomNode.Material = courtMaterial;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        protected override void obj_collision_start(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }

        protected override void obj_collision_done(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {
            String materialName = ((IPhysicsObject)collidingObject.UserTagObj).MaterialName;
            if(materialName.Equals("ball"))
                point_scored = true;
        }
    }
}
