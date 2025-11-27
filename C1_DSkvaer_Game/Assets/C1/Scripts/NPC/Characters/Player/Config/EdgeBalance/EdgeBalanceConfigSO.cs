using UnityEngine;

[CreateAssetMenu(fileName = "EdgeBalanceConfig", menuName = "Character/EdgeBalanceConfig")]
public class EdgeBalanceConfigSO : ScriptableObject {
    [SerializeField] private float edgeCheckDistance = 0.3f;
    [SerializeField] private float edgeCheckDepth = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask pushableLayer;

    public float EdgeCheckDistance => edgeCheckDistance;
    public float EdgeCheckDepth => edgeCheckDepth;
    public LayerMask GroundLayer => groundLayer;
    public LayerMask PushableLayer => pushableLayer;

    public bool IsValid()
    {
        return edgeCheckDistance > 0 && edgeCheckDepth > 0 &&
               IsLayerValid(groundLayer) && IsLayerValid(pushableLayer);
    }

    private bool IsLayerValid(LayerMask layerMask)
    {
        int layer = (int)Mathf.Log(layerMask.value, 2);
        return layer >= 0 && layer < 32;
    }
}