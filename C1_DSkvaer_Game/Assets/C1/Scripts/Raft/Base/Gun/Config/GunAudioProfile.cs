using UnityEngine;

namespace Ship {
    /// <summary>
    /// Аудиопрофиль пушки. Хранит звуки выстрела и перезарядки.
    /// </summary>
    [CreateAssetMenu(fileName = "New GunAudioProfile", menuName = "ShipConfigs/Gun/Audio Profile")]
    public class GunAudioProfile : ScriptableObject {

        [Header("Выстрел")]
        [InspectorLabel("Звук выстрела")]
        [Tooltip("Аудиоклип, который проигрывается при выстреле.")]
        public AudioClip ShotClip;

        [InspectorLabel("Высота звука выстрела")]
        [Tooltip("Скорость и высота воспроизведения выстрела. 1 = норма, больше 1 = быстрее/выше, меньше 1 = медленнее/ниже.")]
        [Range(0.1f, 3f)]
        public float ShotPitch = 1.0f;

        [InspectorLabel("Громкость выстрела")]
        [Tooltip("Громкость звука выстрела от 0 до 1.")]
        [Range(0f, 1f)]
        public float ShotVolume = 1.0f;

        [Header("Перезарядка")]
        [InspectorLabel("Звук перезарядки")]
        [Tooltip("Аудиоклип, который проигрывается при переходе пушки в перезарядку.")]
        public AudioClip ReloadClip;

        [InspectorLabel("Высота звука перезарядки")]
        [Tooltip("Скорость и высота звука перезарядки. Удобно для синхронизации с анимацией.")]
        [Range(0.1f, 3f)]
        public float ReloadPitch = 1.0f;

        [InspectorLabel("Громкость перезарядки")]
        [Tooltip("Громкость звука перезарядки от 0 до 1.")]
        [Range(0f, 1f)]
        public float ReloadVolume = 1.0f;
    }
}
