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
    class VolleyBall: Virtual_Object
    {
        // The sound to make on collisions
        private SoundEffect bounceSound;

        // Hold onto the linear velocity
        private Vector3 linear_velocity;

        float scaling_const = 95f;

        bool stopped;

        public VolleyBall(float mass, float size, SoundEffect bounceSound, bool dummy) : base()
        {
            createObj();
            stopped = dummy;
            applyPhysics(mass, "ball", !dummy, !dummy, GoblinXNA.Physics.ShapeType.Sphere);
            scaleToSize(size);
            this.bounceSound = bounceSound;
            this.linear_velocity = Vector3.Zero;
        }

        protected override void createObj()
        {
            geomNode = new GeometryNode("volleyball");

            geomNode.Model = new Sphere(1, 20, 20);

            // Create a material to apply to the ball
            Material ballMaterial = new Material();
            ballMaterial.Diffuse = new Vector4(0.752941f, 0.223529f, 0.168627f, 1f);
            ballMaterial.Specular = new Vector4(0.752941f, 0.223529f, 0.168627f, 1f);
            ballMaterial.SpecularPower = .3f;

            geomNode.Material = ballMaterial;

            transNode = new TransformNode();
            transNode.AddChild(geomNode);
        }

        protected override void obj_collision_start(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject) { }

        protected override void obj_collision_done(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {
            SoundEffectInstance instance = Sound.Instance.PlaySoundEffect(bounceSound);
            baseObject.MainWorldTransform.GetLinearVelocity(ref linear_velocity);
            linear_velocity = Vector3.Normalize(linear_velocity) * scaling_const;
            baseObject.MainWorldTransform.SetLinearVelocity(linear_velocity);
        }
    }
}
