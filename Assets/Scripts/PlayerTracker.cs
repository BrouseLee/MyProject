using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
    public static PlayerTracker Instance;
    public Vector3 playerPos;

    void Awake()
    {
        playerPos = Camera.main.transform.position; // Initialize the player's position
        Instance = this; // Set the singleton instance
    }

    void Update()
    {
        playerPos = Camera.main.transform.position; // Update the player's position
    }
}