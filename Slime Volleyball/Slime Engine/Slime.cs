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
        public Slime(float mass, float size, Vector4 color)
            : base()
        {
            createObj(color);
            setRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90)));
            scaleToSize(size);
            applyPhysics(mass, "slime", true, false, GoblinXNA.Physics.ShapeType.Sphere);
        }

        public override void setTranslation(Vector3 translationVector)
        {
            transNode.Translation = new Vector3(translationVector.Y, -1 * translationVector.X, transNode.Translation.Z);
        }

        protected override void createObj() { }

        protected void createObj(Vector4 color)
        {
            geomNode = new GeometryNode("slime");
            geomNode.Model = new Sphere(1, 20, 20);

            Material paddleMaterial = new Material();
            paddleMaterial.Diffuse = color;
            paddleMaterial.Specular = color;
            paddleMaterial.SpecularPower = .3f;

            geomNode.Material = paddleMaterial;


            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        protected override void obj_collision_start(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }
        protected override void obj_collision_done(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }
    }
}