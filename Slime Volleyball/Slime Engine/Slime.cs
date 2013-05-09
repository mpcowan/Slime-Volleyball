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
        float current_size;
        float MAX_X, MIN_X;
        float MAX_Y, MIN_Y;

        public Slime(float mass, float size, Vector4 color, float maxx, float minx, float maxy, float miny) : base()
        {
            current_size = size;
            MAX_X = maxx;
            MIN_X = minx;
            MAX_Y = maxy;
            MIN_Y = miny;
            createObj(color);
            setRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90)));
            scaleToSize(size);
            applyPhysics(mass, "slime", true, false, GoblinXNA.Physics.ShapeType.Sphere);
        }

        public float getHeight()
        {
            return current_size / 2;
        }

        public float getSize()
        {
            return current_size;
        }

        public override void scaleToSize(float size)
        {
            current_size = size;
            float scale = 1f;
            Vector3 dimensions = getDimensions();
            if (dimensions.X > dimensions.Y)
                scale = size / Math.Max(dimensions.X, dimensions.Z);
            else
                scale = size / Math.Max(dimensions.Y, dimensions.Z);
            transNode.Scale = new Vector3(scale, scale, scale);
        }

        public void setAbsoluteTranslation(Vector3 translationVector)
        {
            float updX = translationVector.X;
            float updY = translationVector.Y;

            if (updX > MAX_X - current_size / 2)
                updX = MAX_X - current_size / 2;
            else if (updX < MIN_X + current_size / 2)
                updX = MIN_X + current_size / 2;
            if (updY > MAX_Y - current_size / 2)
                updY = MAX_Y - current_size / 2;
            else if (updY < MIN_Y + current_size / 2)
                updY = MIN_Y + current_size / 2;

            transNode.Translation = new Vector3(updX, updY, transNode.Translation.Z);
        }

        public override void setTranslation(Vector3 translationVector)
        {
            float updX = translationVector.Y;
            float updY = -1 * translationVector.X;

            if (updX > MAX_X - current_size / 2)
                updX = MAX_X - current_size / 2;
            else if (updX < MIN_X + current_size / 2)
                updX = MIN_X + current_size / 2;
            if (updY > MAX_Y - current_size / 2)
                updY = MAX_Y - current_size / 2;
            else if (updY < MIN_Y + current_size / 2)
                updY = MIN_Y + current_size / 2;

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