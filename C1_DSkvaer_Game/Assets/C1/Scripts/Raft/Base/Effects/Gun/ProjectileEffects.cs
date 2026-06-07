using UnityEngine;
using System.Collections;

namespace Ship {
    // [RequireComponent] — это защита от ошибок. 
    // Если ты повесишь этот скрипт на пустой объект, Unity сама добавит SpriteRenderer.
    // Это гарантирует, что GetComponent<SpriteRenderer>() никогда не вернет null.
    [RequireComponent(typeof(SpriteRenderer))]
    public class ProjectileEffects : MonoBehaviour {

        // [Header] создает красивый заголовок в Инспекторе Unity для удобства.
        [Header("След снаряда")]
        // TrailRenderer рисует шлейф за объектом.
        [InspectorLabel("TrailRenderer следа")]
        [Tooltip("Шлейф, который тянется за снарядом во время полета.")]
        [SerializeField] private TrailRenderer trailEffect;

        [Header("Попадание")]
        // ParticleSystem — это система частиц (искры, дым, щепки).
        [InspectorLabel("Эффект попадания")]
        [Tooltip("Частицы щепок, искр или дыма при попадании в цель.")]
        [SerializeField] private ParticleSystem hitSplinterEffect;
        [InspectorLabel("Звук попадания")]
        [Tooltip("Звук, который проигрывается при попадании снаряда в цель.")]
        [SerializeField] private AudioClip hitSound;

        [Header("Промах по воде")]
        [InspectorLabel("Всплеск воды")]
        [Tooltip("Частицы всплеска, если снаряд попадает в воду или промахивается.")]
        [SerializeField] private ParticleSystem waterSplashEffect;
        [InspectorLabel("Звук всплеска")]
        [Tooltip("Звук промаха или попадания снаряда в воду.")]
        [SerializeField] private AudioClip waterSplashSound;

        [Header("Настройки")]
        [InspectorLabel("Задержка удаления всплеска")]
        [Tooltip("Сколько секунд подождать перед очисткой эффекта всплеска.")]
        [SerializeField] private float destroySplashDelay = 0.1f;

        // [Range] создает ползунок в инспекторе от 0 до 1.
        // Spatial Blend: 0 = 2D звук (громкость везде одинаковая), 1 = 3D звук (затухает с расстоянием).
        // 0.8f — идеальный баланс для Top-Down, чтобы звук был объемным, но слышным издалека.
        [InspectorLabel("Объемность звука")]
        [Tooltip("0 = 2D звук без затухания, 1 = 3D звук с затуханием по расстоянию.")]
        [Range(0f, 1f)][SerializeField] private float soundSpatialBlend = 0.8f;

        // Приватные переменные для хранения ссылок на компоненты
        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private Collider2D col;

        // Флаг защиты. Если стрела попадет одновременно в два объекта, 
        // скрипт сработает только один раз благодаря этой переменной.
        private bool isFinished = false;

        // Awake вызывается СРАЗУ при создании объекта (раньше Start).
        // Идеальное место для настройки ссылок (GetComponent).
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Пытаемся найти коллайдер (физическую оболочку), чтобы отключить его при ударе.
            col = GetComponent<Collider2D>();

            // ПРОГРАММНОЕ СОЗДАНИЕ КОМПОНЕНТА
            // Вместо того чтобы просить тебя добавлять AudioSource руками в префаб,
            // мы создаем его прямо в коде. Это удобно и уменьшает рутину.
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // Не играть звук при рождении
            audioSource.spatialBlend = soundSpatialBlend;
            audioSource.minDistance = 5f;  // Звук слышен на 100% громкости в радиусе 5 метров
            audioSource.maxDistance = 50f; // Звук полностью исчезает через 50 метров
        }

        // Start вызывается перед первым кадром обновления.
        // Здесь мы сбрасываем эффекты в исходное состояние (нужно для пулинга объектов в будущем).
        private void Start()
        {
            // Убрали отсюда автозапуск trailEffect, так как он теперь вызывается через PlayTrailEffect()
            if (hitSplinterEffect) hitSplinterEffect.Stop(); // Останавливаем частицы, если они играли
            if (waterSplashEffect) waterSplashEffect.Stop();
        }

        // ==========================================
        // ПУБЛИЧНЫЕ МЕТОДЫ
        // Их вызывает скрипт Projectile.cs и GunEffects.cs
        // ==========================================

        // ИСПРАВЛЕНИЕ: Добавлен метод, который требовали другие скрипты
        public void PlayTrailEffect()
        {
            if (trailEffect)
            {
                trailEffect.Clear(); // Очищаем старый хвост (важно при переиспользовании стрел)
                trailEffect.emitting = true; // Включаем генерацию следа
            }
        }

        public void PlayHitEffect(Vector3 position)
        {
            if (isFinished) return; // Если уже сработали — выходим
            HandleImpact(hitSplinterEffect, hitSound, position);
        }

        public void PlayMissEffect(Vector3 position)
        {
            if (isFinished) return;
            HandleImpact(waterSplashEffect, waterSplashSound, position);
        }

        // ==========================================
        // ВНУТРЕННЯЯ ЛОГИКА (ГЛАВНАЯ МАГИЯ)
        // ==========================================

        // Универсальный метод обработки удара (неважно, вода это или корабль)
        private void HandleImpact(ParticleSystem effect, AudioClip clip, Vector3 position)
        {
            isFinished = true; // Блокируем повторные вызовы

            // ШАГ 1: Частицы
            // Мы должны "отцепить" частицы от стрелы. Иначе, когда стрела исчезнет, частицы исчезнут вместе с ней.
            DetachAndPlayParticles(effect, position);

            // ШАГ 2: Маскировка
            // Мы не удаляем объект сразу, потому что ему нужно доиграть звук.
            // Вместо этого мы делаем его невидимым и бесплотным.
            HideProjectile();

            // ШАГ 3: Звук и Смерть
            // Запускаем Корутину (процесс, растянутый во времени).
            StartCoroutine(PlaySoundAndDestroy(clip));
        }

        private void DetachAndPlayParticles(ParticleSystem ps, Vector3 pos)
        {
            if (ps == null) return;

            // ВАЖНЫЙ ТРЮК UNITY:
            // transform.SetParent(null) делает объект частиц независимым.
            // Теперь он не "ребенок" стрелы, а самостоятельный объект в мире.
            ps.transform.SetParent(null);
            ps.transform.position = pos;
            ps.Play();

            // Мы уничтожаем объект частиц отдельно, с задержкой.
            // ps.main.duration — это длительность анимации частиц.
            Destroy(ps.gameObject, ps.main.duration + destroySplashDelay);
        }

        private void HideProjectile()
        {
            // Выключаем картинку (игрок думает, что стрела исчезла)
            if (spriteRenderer) spriteRenderer.enabled = false;

            // Выключаем генерацию следа
            if (trailEffect) trailEffect.emitting = false;

            // Выключаем физику, чтобы невидимая стрела никого не ударила
            if (col) col.enabled = false;
        }

        // IEnumerator — это тип возвращаемого значения для Корутин.
        // Корутины позволяют ставить выполнение кода "на паузу" (yield).
        private IEnumerator PlaySoundAndDestroy(AudioClip clip)
        {
            // Если звука нет, удаляем объект сразу, ждать нечего
            if (clip == null)
            {
                Destroy(gameObject);
                yield break; // Досрочный выход из корутины
            }

            // Перемещаем источник звука в точку удара
            audioSource.transform.position = transform.position;
            // PlayOneShot позволяет проиграть звук, не прерывая другие звуки на этом же источнике
            audioSource.PlayOneShot(clip);

            // МАГИЯ КОРУТИНЫ:
            // WaitForSeconds говорит Unity: "Останови выполнение этого метода здесь,
            // подожди N секунд, и потом продолжи со следующей строки".
            // Это не вешает игру (в отличие от Thread.Sleep).
            yield return new WaitForSeconds(clip.length);

            // Звук доиграл. Теперь можно безопасно удалить пустой объект стрелы.
            Destroy(gameObject);
        }
    }
}
