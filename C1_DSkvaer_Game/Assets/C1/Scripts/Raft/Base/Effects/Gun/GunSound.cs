using UnityEngine;

namespace Ship {
    public class GunSound : MonoBehaviour {
        [Header("Звуки пушки")]
        [InspectorLabel("Аудио-профиль")]
        [Tooltip("ScriptableObject со звуками выстрела и перезарядки для этой пушки.")]
        [SerializeField] private GunAudioProfile audioProfile;

        public void PlayShotSound(Vector3 position)
        {
            if (CanPlay(audioProfile?.ShotClip))
            {
                PlayClipWithPitch(audioProfile.ShotClip, position, audioProfile.ShotPitch, audioProfile.ShotVolume);
            }
        }

        public void PlayReloadSound(Vector3 position)
        {
            if (CanPlay(audioProfile?.ReloadClip))
            {
                PlayClipWithPitch(audioProfile.ReloadClip, position, audioProfile.ReloadPitch, audioProfile.ReloadVolume);
            }
        }

        private bool CanPlay(AudioClip clip)
        {
            return audioProfile != null && clip != null;
        }

        private void PlayClipWithPitch(AudioClip clip, Vector3 position, float pitch, float volume)
        {
            GameObject tempGO = new GameObject("TempAudio_" + clip.name);
            tempGO.transform.position = position;

            AudioSource source = tempGO.AddComponent<AudioSource>();
            source.clip = clip;
            source.pitch = pitch;
            source.volume = volume;
            source.spatialBlend = 1f;
            source.minDistance = 5f;
            source.maxDistance = 50f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.Play();

            float duration = clip.length / Mathf.Max(0.01f, pitch);
            Destroy(tempGO, duration + 0.1f);
        }
    }
}
