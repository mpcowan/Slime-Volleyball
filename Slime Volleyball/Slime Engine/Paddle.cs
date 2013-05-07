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
    class Paddle : Virtual_Object
    {
        public Paddle(float mass, Vector3 size, Vector4 selected_color) : base()
        {
            createObj(size, selected_color);
            applyPhysics(mass, "paddle", true, false, GoblinXNA.Physics.ShapeType.Box);
        }

        public override void setTranslation(Vector3 translationVector)
        {
            transNode.Translation = new Vector3(translationVector.Y, -1 * translationVector.X, transNode.Translation.Z);
        }

        protected override void createObj() { }

        protected void createObj(Vector3 size, Vector4 selected_color)
        {
            geomNode = new GeometryNode("paddle");

            geomNode.Model = new Box(size.X, size.Y, size.Z);

            // Create a material to apply to the ball
            Material paddleMaterial = new Material();
            paddleMaterial.Diffuse = selected_color;
            paddleMaterial.Specular = Color.White.ToVector4();
            paddleMaterial.SpecularPower = 5;

            geomNode.Material = paddleMaterial;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        protected override void obj_collision_start(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }
        protected override void obj_collision_done(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }
    }
}
