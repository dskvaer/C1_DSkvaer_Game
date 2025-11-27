using UnityEngine;

[CreateAssetMenu(fileName = "CollisionConfig", menuName = "Configs/CollisionConfig", order = 1)]
public class CollisionConfig : ScriptableObject {
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask pushableLayer;
    [SerializeField] private LayerMask[] additionalLayers;

    public LayerMask WallLayer => wallLayer;
    public LayerMask PushableLayer => pushableLayer;
    public LayerMask[] AdditionalLayers => additionalLayers;
}