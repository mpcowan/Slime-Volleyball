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
    class Paddle
    {
        // The GeometryNode to be used by GoblinXNA
        private GeometryNode geomNode;

        // The TransformationNode to be used to manipulate the ball
        private TransformNode transNode;

        // The sound to make on collisions
        private SoundEffect bounceSound;

        public Paddle(float mass, float size, SoundEffect bounceSound)
        {
            createObj();
            scaleToSize(size);
            applyPhysics(mass);
            this.bounceSound = bounceSound;
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

        public void translate(Vector3 translationVector)
        {
            transNode.Translation += translationVector;
        }

        public string nodeTranslationToString()
        {
            return "X: " + geomNode.WorldTransformation.Translation.X.ToString() +
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
            return "X: " + transNode.WorldTransformation.Translation.X.ToString() +
                    " Y: " + transNode.WorldTransformation.Translation.Y.ToString() +
                    " Z: " + transNode.WorldTransformation.Translation.Z.ToString();
        }

        private void createObj()
        {
            geomNode = new GeometryNode("volleyball");

            geomNode.Model = new Box(10);

            // Create a material to apply to the ball
            Material paddleMaterial = new Material();
            paddleMaterial.Diffuse = Color.SteelBlue.ToVector4();
            paddleMaterial.Specular = Color.White.ToVector4();
            paddleMaterial.SpecularPower = 5;

            geomNode.Material = paddleMaterial;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        private void applyPhysics(float mass)
        {
            geomNode.Physics = new MataliObject(geomNode);
            geomNode.Physics.MaterialName = "paddle";
            geomNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
            geomNode.Physics.Pickable = true;
            geomNode.Physics.Interactable = false;
            ((MataliObject)geomNode.Physics).Restitution = 0;
            ((MataliObject)geomNode.Physics).DynamicFriction = 0;
            ((MataliObject)geomNode.Physics).StaticFriction = 0;
            geomNode.Physics.Mass = mass;
            geomNode.Physics.Collidable = true;
            geomNode.AddToPhysicsEngine = true;
            ((MataliObject)geomNode.Physics).CollisionStartCallback = paddleCollisionStart;
            ((MataliObject)geomNode.Physics).CollisionEndCallback = paddleCollisionDone;
        }

        private void paddleCollisionDone(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {

        }

        private void paddleCollisionStart(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {

        }
    }
}
