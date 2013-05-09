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
        float MAX_X, MIN_X;
        float MAX_Y, MIN_Y;

        public Slime(float mass, float size, Vector4 color, float maxx, float minx, float maxy, float miny) : base()
        {
            MAX_X = maxx - (size / 2);
            MIN_X = minx + (size / 2);
            MAX_Y = maxy - (size / 2);
            MIN_Y = miny + (size / 2);
            createObj(color);
            setRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90)));
            scaleToSize(size);
            applyPhysics(mass, "slime", true, false, GoblinXNA.Physics.ShapeType.Sphere);
        }

        public void setAbsoluteTranslation(Vector3 translationVector)
        {
            float updX = translationVector.X;
            float updY = translationVector.Y;

            if (updX > MAX_X)
                updX = MAX_X;
            else if (updX < MIN_X)
                updX = MIN_X;
            if (updY > MAX_Y)
                updY = MAX_Y;
            else if (updY < MIN_Y)
                updY = MIN_Y;

            transNode.Translation = new Vector3(updX, updY, transNode.Translation.Z);
        }

        public override void setTranslation(Vector3 translationVector)
        {
            float updX = translationVector.Y;
            float updY = -1 * translationVector.X;

            if (updX > MAX_X) 
                updX = MAX_X;
            else if (updX < MIN_X)
                updX = MIN_X;
            if (updY > MAX_Y)
                updY = MAX_Y;
            else if (updY < MIN_Y)
                updY = MIN_Y;

            transNode.Translation = new Vector3(updX, updY, transNode.Translation.Z);
        }

        protected override void createObj() { }

        protected void createObj(Vector4 color)
        {
            geomNode = new GeometryNode("slime");
            geomNode.Model = new Sphere(1, 20, 20);

            Material paddleMaterial = new Material();
            paddleMaterial.Diffuse = color;
            paddleMaterial.Specular = color;
            paddleMaterial.SpecularPower = .15f;

            geomNode.Material = paddleMaterial;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        protected override void obj_collision_start(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }
        protected override void obj_collision_done(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }
    }
}