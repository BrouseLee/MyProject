using UnityEngine;
using System.Collections.Generic;
using MantaSim.Utilities;
using UnityEngine.UIElements;

public class FishManager : BoidManager
{
    public static FishManager Instance;
    
    // Start is called before the first frame update
    protected void Start()
    {
        SpawnBoids();
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }
}