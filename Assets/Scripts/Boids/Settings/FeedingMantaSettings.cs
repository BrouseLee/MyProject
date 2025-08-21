using UnityEngine;

[CreateAssetMenu(fileName = "FeedingMantaSettings", menuName = "Scriptable Objects/FeedingMantaSettings")]
public class FeedingMantaSettings : BoidSettings
{
    [Header("Additional Feeding Manta Parameters")]
    [Range(1.0f, 5.0f)]
    public float somersaultRadius;
    [Range(0.0f, 1.0f)]
    public float somersaultProbability;
}
