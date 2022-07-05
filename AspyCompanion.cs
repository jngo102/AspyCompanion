using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UObject = UnityEngine.Object;
using Vasi;

namespace AspyCompanion
{
    internal class AspyCompanion : Mod
    {
        internal static AspyCompanion Instance { get; private set; }

        public AspyCompanion() : base("Aspy Companion") { }

        public override string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public override void Initialize()
        {
            Instance = this;

            On.PlayMakerFSM.Start += OnPFSMStart;
        }

        private void OnPFSMStart(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            if (self.name.Contains("Grimmchild") && self.FsmName == "Control")
            {
                self.transform.Find("Enemy Range").GetComponent<CircleCollider2D>().radius *= 2;
                var floats = self.FsmVariables.FloatVariables.ToList();
                var angleFloat = new FsmFloat
                {
                    Name = "Angle",
                    Value = 0,
                };
                floats.Add(angleFloat);
                self.FsmVariables.FloatVariables = floats.ToArray();
                var shootState = self.GetState("Shoot");
                var fireAtTarget = shootState.GetAction<FireAtTarget>(7);
                fireAtTarget.spread = 0;
                var target = fireAtTarget.target;
                var fire = shootState.GetAction<SpawnObjectFromGlobalPool>(4);
                fire.gameObject.Value.CreatePool(3);
                var flameBall = fire.storeObject;
                shootState.AddAction(new GetAngleToTarget2D
                {
                    gameObject = new FsmOwnerDefault(),
                    target = target,
                    offsetX = 0,
                    offsetY = 0,
                    storeAngle = angleFloat,
                    everyFrame = false,
                });
                shootState.AddAction(fire);
                shootState.AddAction(new FloatOperator
                {
                    float1 = angleFloat,
                    float2 = 45,
                    operation = FloatOperator.Operation.Add,
                    storeResult = angleFloat,
                    everyFrame = false,
                });
                shootState.AddAction(new SetVelocityAsAngle
                {
                    gameObject = new FsmOwnerDefault
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = flameBall,
                    },
                    angle = angleFloat,
                    speed = 30,
                    everyFrame = false,
                });
                shootState.AddAction(fire);
                shootState.AddAction(new FloatOperator
                {
                    float1 = angleFloat,
                    float2 = 90,
                    operation = FloatOperator.Operation.Subtract,
                    storeResult = angleFloat,
                    everyFrame = false,
                });
                shootState.AddAction(new SetVelocityAsAngle
                {
                    gameObject = new FsmOwnerDefault
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = flameBall,
                    },
                    angle = angleFloat,
                    speed = 30,
                    everyFrame = false,
                });
            }
            else if (self.name.Contains("Grimmball") && self.FsmName == "Control")
            {
                self.gameObject.AddComponent<Chaser>();
            }

            orig(self);
        }
    }
}