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
using GoblinXNA.Physics;

namespace Slime_Engine
{
    class VolleyBall
    {
        // The GeometryNode to be used by GoblinXNA
        private GeometryNode geomNode;

        // The TransformationNode to be used to manipulate the ball
        private TransformNode transNode;

        // The sound to make on collisions
        private SoundEffect bounceSound;

        // Keep track of your linear velocity
        private Vector3 linear_velocity;

        float scaling_const = 110f;

        public VolleyBall(float mass, float size, SoundEffect bounceSound)
        {
            createObj();
            scaleToSize(size);
            applyPhysics(mass);
            this.bounceSound = bounceSound;
            this.linear_velocity = Vector3.Zero;
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

        public string velocityToString()
        {
            return "X: " + linear_velocity.X.ToString() +
                    " Y: " + linear_velocity.Y.ToString() +
                    " Z: " + linear_velocity.Z.ToString();
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
            geomNode.Physics.MaterialName = "ball";
            geomNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Sphere;
            geomNode.Physics.Pickable = true;
            ((MataliObject)geomNode.Physics).Restitution = .999f;
            ((MataliObject)geomNode.Physics).DynamicFriction = 0;
            ((MataliObject)geomNode.Physics).StaticFriction = 0;
            geomNode.Physics.Interactable = true;
            geomNode.Physics.Mass = mass;
            geomNode.Physics.Collidable = true;
            geomNode.AddToPhysicsEngine = true;
            //((MataliObject)geomNode.Physics).CollisionStartCallback = ballCollisionStart;
            ((MataliObject)geomNode.Physics).CollisionEndCallback = ballCollisionDone;
        }

        private void ballCollisionDone(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {
            SoundEffectInstance instance = Sound.Instance.PlaySoundEffect(bounceSound);
            baseObject.MainWorldTransform.GetLinearVelocity(ref linear_velocity);
            linear_velocity = Vector3.Normalize(linear_velocity) * scaling_const;
            baseObject.MainWorldTransform.SetLinearVelocity(linear_velocity);
        }

        //private void ballCollisionStart(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        //{
        //    if (custom_collision && collision_stopped)
        //    {
        //        collision_stopped = false;
        //        String materialName = ((IPhysicsObject)collidingObject.UserTagObj).MaterialName;
        //        if (materialName.Equals("court"))
        //            ballCollideWithGround(baseObject, collidingObject);
        //        else if (materialName.Equals("paddle"))
        //            ballCollideWithPlayer(baseObject, collidingObject);
        //    }
        //}

        //public Vector3 getPlayerNormal(Vector3 contactPosition, MataliPhysicsObject collidingObject)
        //{
        //  Vector3 paddlePosition = Vector3.Zero;
        //  collidingObject.MainLocalTransform.GetPosition(ref paddlePosition);
        //  return contactPosition - paddlePosition;
        //}

        //public Vector3 getDeflectedVelocity(Vector3 initialVelocity, Vector3 playerNormal)
        //{
        //  Vector3 newVelocity = initialVelocity;
        //  Quaternion output = new Quaternion();
        //  output = Quaternion.CreateFromAxisAngle(playerNormal, 180f);
        //  Vector3.Transform(newVelocity, output);
        //  return newVelocity;
        //}

        //private void ballCollideWithGround(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        //{
        //    //Vector3 linearVelocity = Vector3.Zero;
        //    Vector3 contactPosition = Vector3.Zero;
        //    baseObject.MainWorldTransform.GetPosition(ref contactPosition);
        //    //[TODO]
        //    //if(!inBounds(contactPosition)) //lose
        //    baseObject.MainWorldTransform.GetLinearVelocity(ref linear_velocity);
        //    linear_velocity *= -1f;
        //    baseObject.MainWorldTransform.SetLinearVelocity(linear_velocity);
        //}

        //private void ballCollideWithPlayer(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        //{         
        //    Vector3 linearVelocity = Vector3.Zero;
        //    baseObject.MainWorldTransform.GetLinearVelocity(ref linearVelocity);
        //    Vector3 contactPosition = Vector3.Zero;
        //    baseObject.MainWorldTransform.GetPosition(ref contactPosition);
        //    Vector3 normal = getPlayerNormal(contactPosition, collidingObject);
        //    Vector3 newVelocity = getDeflectedVelocity(linearVelocity, normal);
        //    baseObject.MainWorldTransform.SetLinearVelocity(newVelocity);
        //}
    }
}
