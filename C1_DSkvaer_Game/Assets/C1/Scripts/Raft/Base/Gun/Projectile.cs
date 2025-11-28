using UnityEngine;

namespace Ship {
    // RequireComponent гарантирует, что если ты повесишь этот скрипт на пустой объект,
    // Unity автоматически добавит Rigidbody2D и Collider2D. Это защита от забывчивости.
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class Projectile : MonoBehaviour {

        // =================================================================================
        // НАСТРОЙКИ (INSPECTOR)
        // =================================================================================

        [Header("Конфигурация")]
        [SerializeField, Tooltip("Файл настроек (ScriptableObject) с уроном, скоростью и т.д.")]
        public ProjectileConfig config;

        [Header("Ссылки")]
        [SerializeField, Tooltip("Ссылка на наш скрипт визуальных эффектов")]
        private ProjectileEffects projectileEffects;

        // =================================================================================
        // ПРИВАТНЫЕ ПЕРЕМЕННЫЕ
        // =================================================================================

        private Rigidbody2D rb;
        private float lifetime;      // Таймер жизни снаряда
        private bool hasHit = false; // Флаг: "Мы уже во что-то попали?". Нужен, чтобы не нанести урон дважды за 1 кадр.

        // =================================================================================
        // ИНИЦИАЛИЗАЦИЯ (AWAKE)
        // =================================================================================
        // Awake вызывается один раз, в самый момент создания объекта.
        // Здесь мы кэшируем компоненты и проверяем ошибки.
        private void Awake()
        {
            // 1. Получаем ссылку на физическое тело
            rb = GetComponent<Rigidbody2D>();

            // 2. Проверяем, есть ли конфиг. Если нет - стрела бесполезна, удаляем её.
            if (config == null)
            {
                Debug.LogError($"[Projectile] ОШИБКА: У {name} нет ProjectileConfig!", this);
                Destroy(gameObject);
                return;
            }

            // 3. Настраиваем физику
            // Kinematic означает, что мы двигаем объект кодом, а не гравитацией или толчками.
            rb.bodyType = RigidbodyType2D.Kinematic;
            // Включаем более точную проверку контактов
            rb.useFullKinematicContacts = true;
            // Делаем коллайдер триггером (чтобы он проходил сквозь объекты, вызывая событие, а не отскакивал)
            GetComponent<Collider2D>().isTrigger = true;

            // 4. Рассчитываем время жизни: Расстояние / Скорость
            // Mathf.Max защищает от деления на ноль (если скорость случайно 0)
            lifetime = config.Range / Mathf.Max(0.001f, config.ProjectileSpeed);

            // 5. Запускаем эффект следа (Trail), если он есть
            if (projectileEffects) projectileEffects.PlayTrailEffect();
        }

        // =================================================================================
        // ФИЗИЧЕСКИЙ ЦИКЛ (FIXED UPDATE)
        // =================================================================================
        // FixedUpdate вызывается фиксированное количество раз в секунду (обычно 50).
        // ВСЕ движения через Rigidbody должны быть здесь.
        private void FixedUpdate()
        {
            // Если мы уже попали или конфига нет - ничего не делаем
            if (hasHit || config == null) return;

            // 1. Движение
            // rb.position - текущая позиция
            // transform.up - направление "вперед" для 2D спрайта (зеленая стрелка)
            // Time.fixedDeltaTime - время, прошедшее с прошлого кадра физики
            Vector2 newPos = rb.position + (Vector2)transform.up * config.ProjectileSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);

            // 2. Таймер жизни
            lifetime -= Time.fixedDeltaTime;

            // Если время вышло (снаряд улетел слишком далеко)
            if (lifetime <= 0f)
            {
                HandleMiss(); // Вызываем промах
            }
        }

        // =================================================================================
        // СТОЛКНОВЕНИЯ (TRIGGER ENTER)
        // =================================================================================
        // Вызывается Unity, когда триггер стрелы входит в другой коллайдер
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Фильтры:
            // 1. Не реагируем, если уже попали
            // 2. Не реагируем на null
            // 3. Не реагируем на другие снаряды (чтобы стрелы не взрывались друг об друга)
            if (hasHit || other == null || other.CompareTag("Projectile")) return;

            // СЦЕНАРИЙ: ПОПАДАНИЕ В ЗОНУ УРОНА КОРАБЛЯ
            if (other.CompareTag("HitArea"))
            {
                HandleShipHit(other);
                return;
            }

            // СЦЕНАРИЙ: ПОПАДАНИЕ В ПРЕПЯТСТВИЕ (Остров, скала)
            // !other.isTrigger означает, что мы врезались в твердый объект
            if (!other.isTrigger)
            {
                HandleObstacleHit();
            }
        }

        // =================================================================================
        // ЛОГИКА ОБРАБОТКИ СОБЫТИЙ
        // =================================================================================

        // Обработка попадания в корабль
        private void HandleShipHit(Collider2D hitCollider)
        {
            // Пытаемся получить компонент зоны попадания
            var hitArea = hitCollider.GetComponent<ShipHitArea>();
            if (hitArea == null) return;

            // Ищем здоровье корабля (обычно оно на родительском объекте)
            var health = hitArea.GetComponentInParent<IShipHealth>();

            // Если у объекта нет здоровья или он уже мертв - игнорируем
            if (health == null || health.IsDead) return;

            // Помечаем, что попадание состоялось (остановка движения)
            hasHit = true;

            // 1. Расчет урона (Базовый урон * Множитель зоны, например, x2 за попадание в пороховой склад)
            float finalDamage = config.Damage * hitArea.DamageMultiplier;

            // 2. Нанесение ПРЯМОГО урона
            health.TakeShipDamage(Mathf.RoundToInt(finalDamage));

            // 3. Нанесение AOE урона (Area of Effect - урон по площади)
            // Если радиус взрыва больше 0
            if (config.AreaOfEffectRadius > 0f)
            {
                ApplyAreaDamage(hitCollider, finalDamage);
            }

            // 4. Визуал и Звук
            // Мы НЕ удаляем объект здесь. Мы просим ProjectileEffects сыграть шоу.
            if (projectileEffects)
            {
                projectileEffects.PlayHitEffect(transform.position);
            }
            else
            {
                // Если эффектов нет - удаляем сразу, чтобы не висел мусор
                Destroy(gameObject);
            }

            // Отключаем этот скрипт, чтобы FixedUpdate больше не двигал стрелу
            this.enabled = false;
        }

        // Логика AOE (взрыва)
        private void ApplyAreaDamage(Collider2D directHitTarget, float damage)
        {
            // Создаем круг и ищем все коллайдеры внутри
            var hits = Physics2D.OverlapCircleAll(
                transform.position,
                config.AreaOfEffectRadius,
                LayerMask.GetMask("HitArea") // Ищем только слои HitArea
            );

            foreach (var h in hits)
            {
                // Не наносим урон тому, в кого и так попали прямым выстрелом (чтобы не дублировать)
                if (h == directHitTarget) continue;

                var hh = h.GetComponentInParent<IShipHealth>();
                if (hh != null && !hh.IsDead)
                {
                    hh.TakeShipDamage(Mathf.RoundToInt(damage));
                }
            }
        }

        // Обработка попадания в стену/скалу
        private void HandleObstacleHit()
        {
            hasHit = true;

            // Просто играем эффект попадания (щепки)
            if (projectileEffects)
            {
                projectileEffects.PlayHitEffect(transform.position);
            }
            else
            {
                Destroy(gameObject);
            }
            this.enabled = false;
        }

        // Обработка промаха (выстрел в воду)
        private void HandleMiss()
        {
            hasHit = true;

            if (projectileEffects)
            {
                projectileEffects.PlayMissEffect(transform.position);
            }
            else
            {
                Destroy(gameObject);
            }
            this.enabled = false;
        }

        // Метод для получения конфига (используется пушками)
        public ProjectileConfig GetProjectileConfig()
        {
            return config;
        }
    }
}