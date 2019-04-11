using huqiang.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace huqiang.ModelManager2D
{
    public unsafe struct RigibodyData
    {
        public float rotation;
        public Vector2 velocity;
        public float angularVelocity;
        public bool useAutoMass;
        public float mass;
        /// <summary>
        /// name
        /// </summary>
        public Int32 sharedMaterial;
        public Vector2 centerOfMass;
        public float inertia;
        public float drag;
        public float angularDrag;
        public float gravityScale;
        public RigidbodyType2D bodyType;
        public bool useFullKinematicContacts;
        public bool isKinematic;
        public bool freezeRotation;
        public RigidbodyConstraints2D constraints;
        public bool simulated;
        public RigidbodyInterpolation2D interpolation;
        public RigidbodySleepMode2D sleepMode;
        public Vector2 position;
        public CollisionDetectionMode2D collisionDetectionMode;
    }
    public class RigidbodyModel:DataConversion
    {
        RigibodyData data;
        string sharedMaterial;
        public unsafe override void Load(FakeStruct fake)
        {
            data = *(RigibodyData*)fake.ip;
            sharedMaterial = fake.buffer.GetData(data.sharedMaterial) as string;
        }
        public override void LoadToObject(Component game)
        {
            LoadToObject(game, ref data, sharedMaterial);
        }
        public static void LoadToObject(Component game, ref RigibodyData data, string mat)
        {
            var obj = game as Rigidbody2D;
            if (obj == null)
                return;
            obj.rotation = data.rotation;
            obj.velocity = data.velocity;
            obj.angularVelocity = data.angularVelocity;
            obj.useAutoMass = data.useAutoMass;
            obj.mass = data.mass;
            obj.centerOfMass = data.centerOfMass;
            obj.inertia = data.inertia;
            obj.drag = data.drag;
            obj.angularDrag = data.angularDrag;
            obj.gravityScale = data.gravityScale;
            obj.bodyType = data.bodyType;
            obj.useFullKinematicContacts = data.useFullKinematicContacts;
            obj.isKinematic = data.isKinematic;
            obj.freezeRotation = data.freezeRotation;
            obj.constraints = data.constraints;
            obj.simulated = data.simulated;
            obj.interpolation = data.interpolation;
            obj.sleepMode = data.sleepMode;
            obj.position = data.position;
            obj.collisionDetectionMode = data.collisionDetectionMode;
            if (mat != null)
                obj.sharedMaterial = new PhysicsMaterial2D(mat);
        }
        public static unsafe FakeStruct LoadFromObject(Component com, DataBuffer buffer)
        {
            var obj = com as Rigidbody2D;
            if (obj == null)
                return null;
            FakeStruct fake = new FakeStruct(buffer, BoxColliderData.ElementSize);
            RigibodyData* rd = (RigibodyData*)fake.ip;
            rd->sharedMaterial = buffer.AddData(obj.sharedMaterial.name);
            rd->rotation = obj.rotation;
            rd->velocity = obj.velocity;
            rd->angularVelocity = obj.angularVelocity;
            rd->useAutoMass = obj.useAutoMass;
            rd->mass = obj.mass;
            rd->centerOfMass = obj.centerOfMass;
            rd->inertia = obj.inertia;
            rd->drag = obj.drag;
            rd->angularDrag = obj.angularDrag;
            rd->gravityScale = obj.gravityScale;
            rd->bodyType =obj.bodyType;
            rd->useFullKinematicContacts = obj.useFullKinematicContacts;
            rd->isKinematic = obj.isKinematic;
            rd->freezeRotation = obj.freezeRotation;
            rd->constraints = obj.constraints;
            rd->simulated = obj.simulated;
            rd->interpolation = obj.interpolation;
            rd->sleepMode = obj.sleepMode;
            rd->position = obj.position;
            rd->collisionDetectionMode = obj.collisionDetectionMode;
            return fake;
        }
    }
}
