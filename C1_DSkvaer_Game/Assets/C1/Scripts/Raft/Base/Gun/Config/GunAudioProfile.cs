using UnityEngine;

namespace Ship {
    /// <summary>
    /// Файл настроек звука. Позволяет хранить клипы и их настройки отдельно от логики.
    /// </summary>
    [CreateAssetMenu(fileName = "New GunAudioProfile", menuName = "ShipConfigs/Gun/Audio Profile")]
    public class GunAudioProfile : ScriptableObject {

        [Header("Выстрел")]
        [Tooltip("Аудиоклип выстрела")]
        public AudioClip ShotClip;

        [Tooltip("Скорость воспроизведения (Pitch). 1 = норма, >1 = быстрее/выше, <1 = медленнее/ниже.")]
        [Range(0.1f, 3f)]
        public float ShotPitch = 1.0f;

        [Tooltip("Громкость выстрела (0-1)")]
        [Range(0f, 1f)]
        public float ShotVolume = 1.0f;

        [Header("Перезарядка")]
        [Tooltip("Аудиоклип перезарядки")]
        public AudioClip ReloadClip;

        [Tooltip("Скорость звука перезарядки. Удобно для синхронизации с анимацией.")]
        [Range(0.1f, 3f)]
        public float ReloadPitch = 1.0f;

        [Tooltip("Громкость перезарядки (0-1)")]
        [Range(0f, 1f)]
        public float ReloadVolume = 1.0f;
    }
}