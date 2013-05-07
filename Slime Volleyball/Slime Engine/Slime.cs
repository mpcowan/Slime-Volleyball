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
    class Slime : Virtual_Object
    {
        public Slime(float mass, float size) : base()
        {
            createObj();
            setRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90)));
            scaleToSize(size);
            applyPhysics(mass, "slime", true, false, GoblinXNA.Physics.ShapeType.ConvexHull);
        }

        protected override void createObj()
        {
            geomNode = new GeometryNode("slime");
            ModelLoader loader = new ModelLoader();
            geomNode.Model = (Model)loader.Load("", "redslime");
            ((Model)geomNode.Model).UseInternalMaterials = true;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        protected override void obj_collision_start(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }
        protected override void obj_collision_done(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }
    }
}
