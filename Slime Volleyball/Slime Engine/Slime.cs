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
    class Slime
    {
        // The GeometryNode to be used by GoblinXNA
        private GeometryNode geomNode;

        // The TransformationNode to be used to manipulate the ball
        private TransformNode transNode;

        public Slime(float mass, float size)
        {
            createObj();
            rotate();
            scaleToSize(size);
            applyPhysics(mass);
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

        public void setTranslation(Vector3 translationVector)
        {
            transNode.Translation = translationVector;
        }

        public void translate(Vector3 translationVector)
        {
            transNode.Translation += translationVector;
            float newX = transNode.Translation.X;
            if (transNode.Translation.X > 110)
                newX = 110f;
            else if (transNode.Translation.X < -110)
                newX = -110f;
            float newY = transNode.Translation.Y;
            if (transNode.Translation.Y > 150)
                newY = 150f;
            else if (transNode.Translation.Y < -150)
                newY = -150f;
            float newZ = transNode.Translation.Z;
            if (transNode.Translation.Z > 30)
                newZ = 30f;
            else if (transNode.Translation.Z < 5)
                newZ = 5f;
            transNode.Translation = new Vector3(newX, newY, newZ);
        }

        public void rotate()
        {
            transNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90));
        }

        public string nodeTranslationToString()
        {
            return geomNode.WorldTransformation.Translation.X.ToString("0.00") +
                    " " + geomNode.WorldTransformation.Translation.Y.ToString("0.00") +
                    " " + geomNode.WorldTransformation.Translation.Z.ToString("0.00");
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

        private void createObj()
        {
            geomNode = new GeometryNode("slime");
            ModelLoader loader = new ModelLoader();
            geomNode.Model = (Model)loader.Load("", "redslime");
            ((Model)geomNode.Model).UseInternalMaterials = true;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        private void applyPhysics(float mass)
        {
            geomNode.Physics = new MataliObject(geomNode);
            geomNode.Physics.MaterialName = "slime";
            geomNode.Physics.Shape = GoblinXNA.Physics.ShapeType.ConvexHull;
            geomNode.Physics.Pickable = true;
            geomNode.Physics.Interactable = false;
            ((MataliObject)geomNode.Physics).Restitution = 0;
            ((MataliObject)geomNode.Physics).DynamicFriction = 0;
            ((MataliObject)geomNode.Physics).StaticFriction = 0;
            geomNode.Physics.Mass = mass;
            geomNode.Physics.Collidable = true;
            geomNode.AddToPhysicsEngine = true;
            ((MataliObject)geomNode.Physics).CollisionStartCallback = slimeCollisionStart;
            ((MataliObject)geomNode.Physics).CollisionEndCallback = slimeCollisionDone;
        }

        private void slimeCollisionDone(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {
            
        }

        private void slimeCollisionStart(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {

        }
    }
}
