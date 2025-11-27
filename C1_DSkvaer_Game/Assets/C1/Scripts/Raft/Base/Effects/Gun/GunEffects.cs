using UnityEngine;

namespace Ship {
    /// <summary>
    /// Компонент для управления визуальными эффектами пушки (арбалета).
    /// Используется в GunWeaponSystem для воспроизведения визуальных эффектов при стрельбе.
    /// </summary>
    /// <remarks>
    /// Привязка в Unity Inspector:
    /// - MuzzleFlash: Эффект вспышки выстрела (ParticleSystem, PlayOnAwake=false).
    /// Настройка сцены:
    /// - Привязать к объекту пушки или дочернему объекту (например, Player_Ship/ArbalestGun/Effects).
    /// - MuzzleFlash: Emission=5-10 частиц, Shape=Cone, Lifetime=0.1s, Sorting Layer=Default, Order=6.
    /// Логика работы:
    /// - Awake: Проверяет наличие MuzzleFlash и его настройки.
    /// - PlayMuzzleFlash: Воспроизводит эффект вспышки выстрела.
    /// - PlayTrailEffect: Запускает эффект трассировки на снаряде через ProjectileEffects.
    /// </remarks>
    public class GunEffects : MonoBehaviour {
        [SerializeField] private ParticleSystem muzzleFlash; // Эффект вспышки выстрела

        // Инициализация при старте
        private void Awake()
        {
            if (muzzleFlash == null) // Проверяем наличие эффекта вспышки
            {
                Debug.LogWarning($"MuzzleFlash не привязан для {gameObject.name}!", this); // Логируем предупреждение
            }
            else
            {
                var renderer = muzzleFlash.GetComponent<ParticleSystemRenderer>(); // Получаем рендерер
                if (renderer != null) // Проверяем наличие рендерера
                {
                    renderer.sortingLayerName = "Default"; // Устанавливаем слой сортировки
                    renderer.sortingOrder = 6; // Устанавливаем порядок в слое
                    Debug.Log($"MuzzleFlash для {gameObject.name} настроен: SortingLayer={renderer.sortingLayerName}, SortingOrder={renderer.sortingOrder}"); // Логируем настройку
                }
                else
                {
                    Debug.LogWarning($"ParticleSystemRenderer не найден в MuzzleFlash для {gameObject.name}!", this); // Логируем предупреждение
                }
            }

            Debug.Log($"GunEffects инициализирован для {gameObject.name}"); // Логируем инициализацию
        }

        // Воспроизведение вспышки выстрела
        public void PlayMuzzleFlash(Vector3 position, Quaternion rotation)
        {
            if (muzzleFlash != null) // Проверяем наличие эффекта
            {
                muzzleFlash.transform.position = position; // Устанавливаем позицию
                muzzleFlash.transform.rotation = rotation; // Устанавливаем поворот
                muzzleFlash.Play(); // Воспроизводим эффект
                Debug.Log($"Воспроизведён MuzzleFlash для {gameObject.name} на позиции {position}"); // Логируем воспроизведение
            }
            else
            {
                Debug.LogWarning($"MuzzleFlash отсутствует для {gameObject.name}!", this); // Логируем предупреждение
            }
        }

        // Воспроизведение эффекта трассировки снаряда
        public void PlayTrailEffect(GameObject projectile)
        {
            if (projectile == null) // Проверяем наличие снаряда
            {
                Debug.LogWarning($"Projectile null при вызове PlayTrailEffect для {gameObject.name}!", this); // Логируем предупреждение
                return;
            }

            ProjectileEffects projectileEffects = projectile.GetComponent<ProjectileEffects>(); // Получаем компонент эффектов снаряда
            if (projectileEffects != null) // Проверяем наличие ProjectileEffects
            {
                projectileEffects.PlayTrailEffect(); // Запускаем эффект трассировки
                Debug.Log($"Воспроизведён TrailEffect для снаряда {projectile.name} от {gameObject.name}"); // Логируем воспроизведение
            }
            else
            {
                Debug.LogWarning($"ProjectileEffects отсутствует на {projectile.name} для {gameObject.name}!", this); // Логируем предупреждение
            }
        }
    }
}