using UnityEngine;

[CreateAssetMenu(fileName = "NewBubbleProfile", menuName = "AI/Bubble Profile")]
public class BubbleProfile : ScriptableObject
{
    public enum Reaction {Attract, Ignore, MildAvoid, Flee }
    public Reaction reaction = Reaction.Flee;

    [Header("Bubble reaction parameters")]
    [Min(0)] public float distance = 25f;   // meters
    [Min(0)] public float boost = 2f;  // flee magnification

    [Header("Attract-specific")]
    [Min(0)] public float bubbleHeight = 2f;  // Hover meters above the player
    [Min(0)] public float orbitRadius = 3f;  // The circle around the bubble column?
    public float orbitSpeed = 1f;  // Surround speed ratio
}
