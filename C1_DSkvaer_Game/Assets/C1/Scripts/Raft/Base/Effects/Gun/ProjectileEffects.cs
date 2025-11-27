// ====================================================================================================
// ProjectileEffects.cs – ФИНАЛЬНАЯ ВЕРСИЯ (БЕЗ ОШИБОК, ЧИСТАЯ СЦЕНА)
// ====================================================================================================
using UnityEngine;
using System.Collections;

namespace Ship {
    [RequireComponent(typeof(SpriteRenderer))]
    public class ProjectileEffects : MonoBehaviour {
        [Header("Trail")]
        [SerializeField] private TrailRenderer trailEffect;

        [Header("Hit Effects")]
        [SerializeField] private ParticleSystem hitSplinterEffect;
        [SerializeField] private AudioClip hitSound;

        [Header("Miss Effects")]
        [SerializeField] private ParticleSystem waterSplashEffect;
        [SerializeField] private AudioClip waterSplashSound;

        [Header("УНИЧТОЖЕНИЕ")]
        [SerializeField] private float destroyAfterMiss = 2.0f;
        [SerializeField] private float destroySplashDelay = 0.1f;

        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private Coroutine destroyCoroutine;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                Debug.LogError("[ProjectileEffects] Нет SpriteRenderer!", this);
                Destroy(gameObject);
                return;
            }

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;

            if (trailEffect) trailEffect.emitting = false;
            if (hitSplinterEffect) hitSplinterEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (waterSplashEffect) waterSplashEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // ВСПОМОГАТЕЛЬНЫЙ МЕТОД — ОБЪЯВЛЕН ВЫШЕ
        private void DetachAndPlay(ParticleSystem ps, Vector3 pos)
        {
            if (ps == null) return;

            GameObject obj = ps.gameObject;
            obj.transform.SetParent(null, true);
            obj.transform.position = pos;

            ps.Clear();
            ps.Play();

            // УНИЧТОЖЕНИЕ ЭФФЕКТА ПОСЛЕ ОКОНЧАНИЯ
            float duration = ps.main.duration;
            Destroy(obj, duration + destroySplashDelay);
        }

        public void PlayTrailEffect()
        {
            if (trailEffect)
            {
                trailEffect.emitting = true;
                trailEffect.Clear();
            }
        }

        public void PlayHitEffect(Vector3 position)
        {
            DetachAndPlay(hitSplinterEffect, position);

            if (hitSound && audioSource)
            {
                audioSource.transform.position = position;
                audioSource.PlayOneShot(hitSound);
            }
        }

        public void PlayMissEffect(Vector3 position)
        {
            if (destroyCoroutine != null)
                StopCoroutine(destroyCoroutine);

            // БРЫЗГИ
            DetachAndPlay(waterSplashEffect, position);

            // ЗВУК
            if (waterSplashSound && audioSource)
            {
                audioSource.transform.position = position;
                audioSource.PlayOneShot(waterSplashSound);
            }

            // СНАРЯД: ЗАТУХАНИЕ + УДАЛЕНИЕ
            destroyCoroutine = StartCoroutine(DestroyProjectileAfterMiss());
        }

        private IEnumerator DestroyProjectileAfterMiss()
        {
            yield return StartCoroutine(FadeOut());

            float timer = 0f;
            while (timer < destroyAfterMiss)
            {
                if (!this || !gameObject) yield break;
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            if (this && gameObject)
            {
                Debug.Log($"[ProjectileEffects] СНАРЯД УДАЛЁН: {name}");
                Destroy(gameObject);
            }
        }

        private IEnumerator FadeOut()
        {
            if (!spriteRenderer) yield break;

            float duration = 0.5f;
            float timer = 0f;
            Color start = spriteRenderer.color;

            while (timer < duration)
            {
                if (!spriteRenderer) yield break;
                timer += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / duration);
                spriteRenderer.color = new Color(start.r, start.g, start.b, alpha);
                yield return null;
            }

            if (spriteRenderer)
                spriteRenderer.color = new Color(start.r, start.g, start.b, 0f);
        }

        // ФАЛЛБЭК
        private void OnEnable() => Invoke(nameof(ForceDestroy), destroyAfterMiss + 1f);
        private void OnDisable() => CancelInvoke(nameof(ForceDestroy));
        private void ForceDestroy()
        {
            if (this && gameObject)
            {
                Debug.LogWarning($"[ProjectileEffects] ФОРС-УДАЛЕНИЕ: {name}");
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            CancelInvoke(nameof(ForceDestroy));
            if (destroyCoroutine != null)
                StopCoroutine(destroyCoroutine);
        }
    }
}