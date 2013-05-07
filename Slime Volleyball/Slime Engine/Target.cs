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
using GoblinXNA.Physics;

namespace Slime_Engine
{
    class Target : Virtual_Object
    {
        public Target(float size, Vector4 colr) : base()
        {
            createObj(colr);
            scaleToSize(size);
        }

        protected override void createObj() { }

        private void createObj(Vector4 colr)
        {
            geomNode = new GeometryNode("target");

            geomNode.Model = new Sphere(1, 20, 20);

            // Create a material to apply to the target
            Material targetMaterial = new Material();
            targetMaterial.Diffuse = colr;
            targetMaterial.Specular = Color.White.ToVector4();
            targetMaterial.SpecularPower = 5;

            geomNode.Material = targetMaterial;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        protected override void obj_collision_start(Komires.MataliPhysics.PhysicsObject baseObject, Komires.MataliPhysics.PhysicsObject collidingObject) { }
        protected override void obj_collision_done(Komires.MataliPhysics.PhysicsObject baseObject, Komires.MataliPhysics.PhysicsObject collidingObject) { }
    }
}
