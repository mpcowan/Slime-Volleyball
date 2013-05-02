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
    class Court
    {
        // The GeometryNode to be used by GoblinXNA
        private GeometryNode geomNode;

        // The TransformationNode to be used to manipulate the ball
        private TransformNode transNode;

        private bool point_scored;

        public Court(float mass, Vector3 size)
        {
            point_scored = false;
            createObj(size);
            applyPhysics(mass);
        }

        public bool is_point_scored()
        {
            return point_scored;
        }

        public void reset_point_scored()
        {
            point_scored = false;
        }

        public TransformNode getTransformNode()
        {
            return transNode;
        }

        public Vector3 getDimensions()
        {
            return Vector3Helper.GetDimensions(geomNode.Model.MinimumBoundingBox);
        }

        public void scaleToSize(float size)
        {
            float scale = 1f;
            Vector3 dimensions = getDimensions();
            if (dimensions.X > dimensions.Y)
                scale = size / Math.Max(dimensions.X, dimensions.Z);
            else
                scale = size / Math.Max(dimensions.Y, dimensions.Z);
            transNode.Scale = new Vector3(scale, scale, scale);
        }

        public void scaleX(float size)
        {
            float scale = 1f;
            Vector3 dimensions = getDimensions();
            scale = size / dimensions.X;
            Vector3 oldScales = transNode.Scale;
            transNode.Scale = new Vector3(scale, oldScales.Y, oldScales.Z);
        }

        public void scaleY(float size)
        {
            float scale = 1f;
            Vector3 dimensions = getDimensions();
            scale = size / dimensions.Y;
            Vector3 oldScales = transNode.Scale;
            transNode.Scale = new Vector3(oldScales.X, scale, oldScales.Z);
        }

        public void scaleZ(float size)
        {
            float scale = 1f;
            Vector3 dimensions = getDimensions();
            scale = size / dimensions.Z;
            Vector3 oldScales = transNode.Scale;
            transNode.Scale = new Vector3(oldScales.X, oldScales.Y, scale);
        }

        public void translate(Vector3 translationVector)
        {
            transNode.Translation += translationVector;
        }

        public string nodeTranslationToString()
        {
            return  "X: " + geomNode.WorldTransformation.Translation.X.ToString() +
                    " Y: " + geomNode.WorldTransformation.Translation.Y.ToString() +
                    " Z: " + geomNode.WorldTransformation.Translation.Z.ToString();
        }

        public string translationToString()
        {
            return  "X: " + transNode.Translation.X.ToString() +
                    " Y: " + transNode.Translation.Y.ToString() +
                    " Z: " + transNode.Translation.Z.ToString();
        }

        public string worldTransformToString()
        {
            return  "X: " + transNode.WorldTransformation.Translation.X.ToString() +
                    " Y: " + transNode.WorldTransformation.Translation.Y.ToString() +
                    " Z: " + transNode.WorldTransformation.Translation.Z.ToString();
        }

        private void createObj(Vector3 dim)
        {
            geomNode = new GeometryNode("court");

            geomNode.Model = new Box(dim.X, dim.Y, dim.Z);

            // Create a material to apply to the court
            Material courtMaterial = new Material();
            courtMaterial.Diffuse = Color.DarkGreen.ToVector4();
            courtMaterial.Specular = Color.White.ToVector4();
            courtMaterial.SpecularPower = 5;

            geomNode.Material = courtMaterial;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        private void applyPhysics(float mass)
        {
            geomNode.Physics = new MataliObject(geomNode);
            geomNode.Physics.MaterialName = "court";
            geomNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
            geomNode.Physics.Pickable = true;
            geomNode.Physics.Interactable = false;
            ((MataliObject)geomNode.Physics).Restitution = 0;
            ((MataliObject)geomNode.Physics).DynamicFriction = 0;
            ((MataliObject)geomNode.Physics).StaticFriction = 0;
            geomNode.Physics.Mass = mass;
            geomNode.Physics.Collidable = true;
            geomNode.AddToPhysicsEngine = true;
            ((MataliObject)geomNode.Physics).CollisionStartCallback = courtCollisionStart;
            ((MataliObject)geomNode.Physics).CollisionEndCallback = courtCollisionDone;
        }

        private void courtCollisionDone(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {
            point_scored = true;
        }

        private void courtCollisionStart(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {

        }
    }
}
