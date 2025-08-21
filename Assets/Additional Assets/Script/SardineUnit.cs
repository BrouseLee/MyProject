using UnityEngine;

public class SardineUnit : MonoBehaviour
{

    [Header("Behaviour Tweaks")]
    [Range(30, 300)] public float FOVAngle ;
    [Range(0.01f, 1f)] public float turnSmooth ;

    internal float speed;
    internal Transform tf;

    void Awake()
    {
        tf = transform;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
