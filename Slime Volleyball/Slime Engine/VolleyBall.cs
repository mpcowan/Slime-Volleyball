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

namespace Engine
{
    class VolleyBall
    {
        // The GeometryNode to be used by GoblinXNA
        GeometryNode geomNode;

        // The TransformationNode to be used to manipulate the ball
        TransformNode transNode;

        public VolleyBall()
        {
            createObj();
            applyPhysics();
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
            {
                scale = size / Math.Max(dimensions.X, dimensions.Z);
            }
            else
            {
                scale = size / Math.Max(dimensions.Y, dimensions.Z);
            }
            transNode.Scale = new Vector3(scale, scale, scale);
        }

        private void createObj()
        {
            geomNode.Model = new Sphere(1, 20, 20);

            // Create a material to apply to the ball
            Material ballMaterial = new Material();
            ballMaterial.Diffuse = Color.SteelBlue.ToVector4();
            ballMaterial.Specular = Color.White.ToVector4();
            ballMaterial.SpecularPower = 5;

            geomNode.Material = ballMaterial;
        }

        private void applyPhysics()
        {

        }
    }
}
