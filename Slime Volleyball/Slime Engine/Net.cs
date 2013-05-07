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

namespace Slime_Engine
{
    class Net : Virtual_Object
    {
        public Net(float mass, Vector3 dim) : base()
        {
            createObj(dim);
            applyPhysics(mass, "net", true, false, GoblinXNA.Physics.ShapeType.Box);
        }

        protected override void createObj() { }

        private void createObj(Vector3 dim)
        {
            geomNode = new GeometryNode("net");

            geomNode.Model = new Box(dim.X, dim.Y, dim.Z);

            // Create a material to apply to the court
            Material netMaterial = new Material();
            netMaterial.Diffuse = Color.Brown.ToVector4();
            netMaterial.Specular = Color.White.ToVector4();
            netMaterial.SpecularPower = 5;

            geomNode.Material = netMaterial;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        protected override void obj_collision_start(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }
        protected override void obj_collision_done(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }
    }
}
