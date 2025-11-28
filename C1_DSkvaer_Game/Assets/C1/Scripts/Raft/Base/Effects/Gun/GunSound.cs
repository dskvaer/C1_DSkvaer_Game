using UnityEngine;

namespace Ship {
    /// <summary>
    /// Компонент для управления звуковыми эффектами пушки.
    /// Использует ScriptableObject (GunAudioProfile) для гибкой настройки.
    /// </summary>
    public class GunSound : MonoBehaviour {

        [Header("Profile")]
        [SerializeField] private GunAudioProfile audioProfile; // Ссылка на настройки

        private void Awake()
        {
            if (audioProfile == null)
            {
                Debug.LogWarning($"[GunSound] AudioProfile не назначен для {gameObject.name}!", this);
            }
            else
            {
                Debug.Log($"[GunSound] Инициализирован с профилем: {audioProfile.name}", this);
            }
        }

        // Воспроизведение звука выстрела
        public void PlayShotSound(Vector3 position)
        {
            if (CanPlay(audioProfile?.ShotClip))
            {
                PlayClipWithPitch(audioProfile.ShotClip, position, audioProfile.ShotPitch, audioProfile.ShotVolume);
                // Debug.Log удалил, чтобы не спамить в консоль при частой стрельбе
            }
        }

        // Воспроизведение звука перезарядки
        public void PlayReloadSound(Vector3 position)
        {
            if (CanPlay(audioProfile?.ReloadClip))
            {
                PlayClipWithPitch(audioProfile.ReloadClip, position, audioProfile.ReloadPitch, audioProfile.ReloadVolume);
                Debug.Log($"[GunSound] Перезарядка...", this);
            }
        }

        // Проверка на валидность данных
        private bool CanPlay(AudioClip clip)
        {
            if (audioProfile == null) return false;
            if (clip == null) return false;
            return true;
        }

        /// <summary>
        /// Кастомный метод воспроизведения, так как AudioSource.PlayClipAtPoint не поддерживает Pitch.
        /// Создает временный объект, проигрывает звук и удаляет объект.
        /// </summary>
        private void PlayClipWithPitch(AudioClip clip, Vector3 position, float pitch, float volume)
        {
            // 1. Создаем временный GameObject
            GameObject tempGO = new GameObject("TempAudio_" + clip.name);
            tempGO.transform.position = position;

            // 2. Добавляем и настраиваем AudioSource
            AudioSource source = tempGO.AddComponent<AudioSource>();
            source.clip = clip;
            source.pitch = pitch;
            source.volume = volume;

            // Настройки 3D звука (чтобы затухал с расстоянием)
            source.spatialBlend = 1f; // 1 = полностью 3D
            source.minDistance = 5f;
            source.maxDistance = 50f;
            source.rolloffMode = AudioRolloffMode.Linear;

            // 3. Играем
            source.Play();

            // 4. Уничтожаем объект, когда клип закончится (с учетом скорости!)
            float duration = clip.length / Mathf.Max(0.01f, pitch); // Если pitch=2, звук играет в 2 раза быстрее
            Destroy(tempGO, duration + 0.1f);
        }
    }
}