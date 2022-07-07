using HutongGames.PlayMaker;
using UnityEngine;

namespace AspyCompanion
{
    [ActionCategory(ActionCategory.Audio)]
    [HutongGames.PlayMaker.Tooltip("Set an audio source's clip to a random audio clip.")]
    public class SetRandomAudioClip : FsmStateAction
    {
        [RequiredField]
        [CheckForComponent(typeof(AudioSource))]
        [HutongGames.PlayMaker.Tooltip("The GameObject with an AudioSource component.")]
        public FsmOwnerDefault gameObject;

        [CompoundArray("Audio Clips", "Audio Clip", "Weight")]
        public AudioClip[] audioClips;

        [HasFloatSlider(0f, 1f)]
        public FsmFloat[] weights;

        public override void Reset()
        {
            audioClips = new AudioClip[3];
            weights = new FsmFloat[3] { 1, 1, 1 };
        }

        public override void OnEnter()
        {
            DoPlayRandomClip();
            Finish();
        }

        private void DoPlayRandomClip()
        {
            var audioSource = gameObject.GameObject.Value.GetComponent<AudioSource>();
            if (audioClips.Length == 0 || audioSource.isPlaying)
            {
                return;
            }

            int randomWeightedIndex = ActionHelpers.GetRandomWeightedIndex(weights);
            if (randomWeightedIndex != -1)
            {
                AudioClip audioClip = audioClips[randomWeightedIndex];
                if (audioClip != null)
                {
                    audioSource.clip = audioClip;
                }
            }
        }
    }
}