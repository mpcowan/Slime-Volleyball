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
    abstract class Virtual_Object
    {
        // The GeometryNode to be used by GoblinXNA
        protected GeometryNode geomNode;

        // The TransformationNode to be used to manipulate the ball
        protected TransformNode transNode;

        public Virtual_Object() { }

        public TransformNode getTransformNode()
        {
            return transNode;
        }

        public Vector3 getDimensions()
        {
            return Vector3Helper.GetDimensions(geomNode.Model.MinimumBoundingBox);
        }

        public float getXDim()
        {
            return Vector3Helper.GetDimensions(geomNode.Model.MinimumBoundingBox).X;
        }

        public float getYDim()
        {
            return Vector3Helper.GetDimensions(geomNode.Model.MinimumBoundingBox).Y;
        }

        public float getZDim()
        {
            return Vector3Helper.GetDimensions(geomNode.Model.MinimumBoundingBox).Z;
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

        public void setRotation(Quaternion q)
        {
            transNode.Rotation = q;
        }

        public virtual void setTranslation(Vector3 translationVector)
        {
            transNode.Translation = translationVector;
        }

        public void setXYTranslation(Vector3 translationVector)
        {
            transNode.Translation = new Vector3(translationVector.X, translationVector.Y, transNode.Translation.Z);
        }

        public void translate(Vector3 translationVector)
        {
            transNode.Translation += translationVector;
        }

        public Vector3 getWorldTransformationTranslation()
        {
            return geomNode.WorldTransformation.Translation;
        }

        public string nodeTranslationToString()
        {
            return  geomNode.WorldTransformation.Translation.X.ToString("0.00") +
                    " " + geomNode.WorldTransformation.Translation.Y.ToString("0.00") +
                    " " + geomNode.WorldTransformation.Translation.Z.ToString("0.00");
        }

        protected abstract void createObj();

        protected virtual void applyPhysics(float mass, string name, bool active, bool is_interactable, ShapeType approx_shape)
        {
            if (active)
            {
                geomNode.Physics = new MataliObject(geomNode);
                geomNode.Physics.MaterialName = name;
                geomNode.Physics.Shape = approx_shape;
                geomNode.Physics.Pickable = true;
                ((MataliObject)geomNode.Physics).Restitution = .999f;
                ((MataliObject)geomNode.Physics).DynamicFriction = 0;
                ((MataliObject)geomNode.Physics).StaticFriction = 0;
                geomNode.Physics.Interactable = is_interactable;
                geomNode.Physics.Mass = mass;
                geomNode.Physics.Collidable = true;
                geomNode.AddToPhysicsEngine = true;
                ((MataliObject)geomNode.Physics).CollisionStartCallback = obj_collision_start;
                ((MataliObject)geomNode.Physics).CollisionEndCallback = obj_collision_done;
            }
        }

        protected abstract void obj_collision_start(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject);
        protected abstract void obj_collision_done(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject);
    }
}
