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
    class InvisibleWall : Virtual_Object
    {
        public InvisibleWall(float mass, Vector3 size) : base()
        {
            createObj(size);
            applyPhysics(mass, "wall", true, false, GoblinXNA.Physics.ShapeType.Box);
        }

        protected override void createObj() { }

        private void createObj(Vector3 dim)
        {
            geomNode = new GeometryNode("wall");

            geomNode.Model = new Box(dim.X, dim.Y, dim.Z);

            // Create a material to apply to the wall
            Material wallMaterial = new Material();
            wallMaterial.Diffuse = new Vector4(0, 0, 0, 0);
            wallMaterial.Specular = new Vector4(0, 0, 0, 0);
            wallMaterial.SpecularPower = 5;

            geomNode.Material = wallMaterial;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        protected override void obj_collision_start(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }
        protected override void obj_collision_done(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }
    }
}