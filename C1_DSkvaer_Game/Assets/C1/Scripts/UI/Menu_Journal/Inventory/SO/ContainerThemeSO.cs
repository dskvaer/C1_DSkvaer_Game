using UnityEngine;

namespace Menu_Journal.Data {
    [CreateAssetMenu(fileName = "NewContainerTheme", menuName = "Inventory/Container Theme")]
    public class ContainerThemeSO : ScriptableObject {
        [Header("Идентификация")]
        [InspectorLabel("Префикс ID")]
        [Tooltip("Префикс для генерации ID контейнера. Например: PIRATE, MERCH, PLAYER.")]
        public string IDPrefix = "GENERIC";

        [InspectorLabel("Название в UI")]
        [Tooltip("Красивое имя, которое будет отображаться игроку. Например: 'Хабар пирата'.")]
        public string DisplayName = "Контейнер";

        [Header("Визуал")]
        [InspectorLabel("Малый контейнер")]
        [Tooltip("Спрайт для малого веса груза: меньше 10 кг.")]
        public Sprite SmallSprite;

        [InspectorLabel("Средний контейнер")]
        [Tooltip("Спрайт для среднего веса груза: от 10 до 50 кг.")]
        public Sprite MediumSprite;

        [InspectorLabel("Большой контейнер")]
        [Tooltip("Спрайт для большого веса груза: больше 50 кг.")]
        public Sprite LargeSprite;

        /// <summary>
        /// Возвращает спрайт контейнера по его размеру.
        /// </summary>
        public Sprite GetSprite(ContainerSize size)
        {
            switch (size)
            {
                case ContainerSize.Small: return SmallSprite;
                case ContainerSize.Medium: return MediumSprite;
                case ContainerSize.Large: return LargeSprite;
                default: return SmallSprite;
            }
        }
    }
}
