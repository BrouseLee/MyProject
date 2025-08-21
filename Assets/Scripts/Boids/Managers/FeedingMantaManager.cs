using UnityEngine;
using System.Collections.Generic;
using MantaSim.Utilities;

public class FeedingMantaManager : BoidManager
{
    public static FeedingMantaManager Instance;
    protected FeedingMantaSettings feedingMantaSettings;
    
    // Start is called before the first frame update
    protected void Start()
    {
        SpawnBoids();
        feedingMantaSettings = settings as FeedingMantaSettings;
        Instance = this;
    }

    public Vector3? CanSomersault(Transform transform)
    {
        Vector3 somersaultCentre = transform.position + transform.up * feedingMantaSettings.somersaultRadius;

        if (IsInsideBounds(transform.position) && IsInsideBounds(somersaultCentre))
        {
            return somersaultCentre;
        }   

        return null;
    }

}