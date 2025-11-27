using UnityEngine;

namespace Ship {
    /// <summary>
    /// Компонент для управления звуковыми эффектами пушки (арбалета).
    /// Используется в GunWeaponSystem для воспроизведения звуков при стрельбе и перезарядке.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - ShotSound: Звук выстрела (AudioClip).
    /// - ReloadSound: Звук перезарядки (AudioClip).
    /// Настройка сцены:
    /// - Привязать к объекту пушки или дочернему объекту (например, Player_Ship/ArbalestGun/Sound).
    /// - ShotSound: AudioClip с коротким звуком выстрела арбалета (длина ~0.5с).
    /// - ReloadSound: AudioClip с звуком перезарядки арбалета (длина ~1-2с, в зависимости от FireRate).
    /// Логика работы:
    /// - Awake: Проверяет наличие ShotSound и ReloadSound.
    /// - PlayShotSound: Воспроизводит звук выстрела.
    /// - PlayReloadSound: Воспроизводит звук перезарядки.
    /// </remarks>
    public class GunSound : MonoBehaviour {
        [SerializeField] private AudioClip shotSound; // Звук выстрела
        [SerializeField] private AudioClip reloadSound; // Звук перезарядки

        // Инициализация при старте
        private void Awake()
        {
            if (shotSound == null) // Проверяем наличие звука выстрела
            {
                Debug.LogWarning($"ShotSound не привязан для {gameObject.name}!", this); // Логируем предупреждение
            }

            if (reloadSound == null) // Проверяем наличие звука перезарядки
            {
                Debug.LogWarning($"ReloadSound не привязан для {gameObject.name}!", this); // Логируем предупреждение
            }

            Debug.Log($"GunSound инициализирован для {gameObject.name}"); // Логируем инициализацию
        }

        // Воспроизведение звука выстрела
        public void PlayShotSound(Vector3 position)
        {
            if (shotSound != null) // Проверяем наличие звука
            {
                AudioSource.PlayClipAtPoint(shotSound, position); // Проигрываем звук выстрела
                Debug.Log($"Воспроизведён ShotSound для {gameObject.name} на позиции {position}"); // Логируем воспроизведение
            }
            else
            {
                Debug.LogWarning($"ShotSound отсутствует для {gameObject.name}!", this); // Логируем предупреждение
            }
        }

        // Воспроизведение звука перезарядки
        public void PlayReloadSound(Vector3 position)
        {
            if (reloadSound != null) // Проверяем наличие звука
            {
                AudioSource.PlayClipAtPoint(reloadSound, position); // Проигрываем звук перезарядки
                Debug.Log($"Воспроизведён ReloadSound для {gameObject.name} на позиции {position}"); // Логируем воспроизведение
            }
            else
            {
                Debug.LogWarning($"ReloadSound отсутствует для {gameObject.name}!", this); // Логируем предупреждение
            }
        }
    }
}