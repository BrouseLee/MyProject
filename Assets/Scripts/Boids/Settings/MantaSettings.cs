using UnityEngine;

[CreateAssetMenu(fileName = "MantaSettings", menuName = "Scriptable Objects/MantaSettings")]
public class MantaSettings : BoidSettings
{
    [Header("Additional Manta Parameters")]
    [Range(0.0f, 50.0f)]
    public float towardsPlayerWeight;
    [Tooltip("Amount of time to wait before the ray can swim up to a player again.")]
    public int cooldownTime = 10;
    [Range(1.0f, 30.0f)]
    [Tooltip("Minimum distance to player for the ray to swim towards them.")]
    public float curiousRange;
}
