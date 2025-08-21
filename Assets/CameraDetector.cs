using UnityEngine;

public class CameraDetector : MonoBehaviour
{

    public GameObject sphere;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Debug.Log(this.transform.position);
        Debug.Log(sphere.transform.position);

    }
}
