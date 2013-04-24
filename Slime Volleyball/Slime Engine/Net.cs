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
    class Net
    {
        // The GeometryNode to be used by GoblinXNA
        private GeometryNode geomNode;

        // The TransformationNode to be used to manipulate the ball
        private TransformNode transNode;

        // The sound to make on collisions
        private SoundEffect bounceSound;

        public Net(float mass, float size, SoundEffect bounceSound)
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

            geomNode.Model = new Sphere(1, 20, 20);

            // Create a material to apply to the ball
            Material ballMaterial = new Material();
            ballMaterial.Diffuse = Color.SteelBlue.ToVector4();
            ballMaterial.Specular = Color.White.ToVector4();
            ballMaterial.SpecularPower = 5;

            geomNode.Material = ballMaterial;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        private void applyPhysics(float mass)
        {
            geomNode.Physics = new MataliObject(geomNode);
            geomNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Sphere;
            geomNode.Physics.Pickable = true;
            ((MataliObject)geomNode.Physics).Restitution = 1.5f;
            geomNode.Physics.Interactable = true;
            geomNode.Physics.Mass = mass;
            geomNode.Physics.Collidable = true;
            geomNode.AddToPhysicsEngine = true;
            ((MataliObject)geomNode.Physics).CollisionStartCallback = ballCollision;
            ((MataliObject)geomNode.Physics).CollisionEndCallback = ballCollisionDone;
        }

        private void ballCollisionDone(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {
            SoundEffectInstance instance = Sound.Instance.PlaySoundEffect(bounceSound);
        }

        private void ballCollision(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {

        }
    }
}
