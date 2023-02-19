using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Modding.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Vasi;
using Random = UnityEngine.Random;

namespace AspyCompanion
{
    internal class AspyCompanion : Mod
    {
        internal static AspyCompanion Instance { get; private set; }
        public AspyCompanion() : base("Aspy Companion") { }

        private Dictionary<string, AudioClip> _clips = new();
        private Texture2D _aspyTexture;
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            Instance = this;

            LoadAssets();

            On.PlayMakerFSM.Start += OnPFSMStart;
        }

        private void OnPFSMStart(On.PlayMakerFSM.orig_Start orig, PlayMakerFSM self)
        {
            if (self.name.Contains("Grimmchild") && self.FsmName == "Control")
            {
                self.GetComponent<tk2dSprite>().GetCurrentSpriteDef().material.mainTexture = _aspyTexture;

                self.transform.Find("Enemy Range").GetComponent<CircleCollider2D>().radius *= 2;
                var floats = self.FsmVariables.FloatVariables.ToList();
                var angleFloat = new FsmFloat
                {
                    Name = "Angle",
                    Value = 0,
                };
                floats.Add(angleFloat);
                self.FsmVariables.FloatVariables = floats.ToArray();

                var voiceLoop = self.Fsm.GetFsmGameObject("Voice Loop");
                voiceLoop.Value.GetComponent<AudioSource>().clip = _clips["Idle " + Random.Range(1, 3)];

                var restState = self.GetState("Rest Start");
                var shootState = self.GetState("Shoot");
                var changeState = self.GetState("Change");

                var setRandomClip = (AudioSource audioSource) => audioSource.clip = _clips["Idle " + Random.Range(1, 3)];

                self.GetState("Follow").InsertAction(0, new SetRandomAudioClip
                {
                    gameObject = new FsmOwnerDefault
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = voiceLoop,
                    },
                    audioClips = new[]
                    {
                        _clips["Idle 1"],
                        _clips["Idle 2"],
                    },
                    weights = new FsmFloat[2] { 1, 1 },
                });

                var anticAudio = self.GetAction<AudioPlayerOneShot>("Antic", 6);
                anticAudio.audioClips[0] = _clips["Attack 1"];
                anticAudio.audioClips[1] = _clips["Attack 2"];

                restState.RemoveAction<AudioStop>();
                restState.AddAction(new SetAudioClip
                {
                    gameObject = new FsmOwnerDefault
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = voiceLoop,
                    },
                    audioClip = _clips["Sleep"],
                });
                restState.AddAction(new AudioPlaySimple
                {
                    gameObject = new FsmOwnerDefault
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = voiceLoop,
                    },
                    volume = 1,
                    oneShotClip = new FsmObject
                    {
                        Value = null,
                    },
                });

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
                self.gameObject.GetOrAddComponent<Chaser>();
            }

            orig(self);
        }

        private void LoadAssets()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) continue;
                    if (!resourceName.Contains("aspy")) continue;

                    var bundle = AssetBundle.LoadFromStream(stream);

                    bundle.LoadAllAssets<AudioClip>().ToList().ForEach(clip =>
                    {
                        _clips.Add(clip.name, clip);
                    });

                    _aspyTexture = bundle.LoadAsset<Texture2D>("Texture");

                    var charmTexture = bundle.LoadAsset<Texture2D>("Charm");
                    var charmSprite = Sprite.Create(charmTexture, new Rect(0, 0, charmTexture.width, charmTexture.height), new Vector2(0.5f, 0.5f));
                    GameCameras.instance.hudCamera.transform.Find("Charm Icons").GetComponent<CharmIconList>().grimmchildLevel4 = charmSprite;

                    stream.Dispose();
                }
            }
        }
    }
}