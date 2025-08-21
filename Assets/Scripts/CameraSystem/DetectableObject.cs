using System.Collections.Generic;
using UnityEngine;

public class DetectableObject : MonoBehaviour
{
    public List<ObjectType> objectTypes;

    private Renderer cachedRenderer;
    public Renderer Renderer => cachedRenderer ??= GetComponent<Renderer>();
}