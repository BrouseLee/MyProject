using UnityEngine;

public class CycloneFeeding : MonoBehaviour
{
    public float speed;
    public float radius;

    public Vector3 center; // Center of the circle

    private float angle; // Angle for movement

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speed = CycloneManager.CM.speed;
        radius = Random.Range(CycloneManager.CM.minRadius, CycloneManager.CM.maxRadius);
        angle = Random.Range(0, 2*Mathf.PI);
        center = CycloneManager.CM.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Increase angle over time
        angle += speed * Time.deltaTime;

        // Calculate new position
        float x = center.x + Mathf.Cos(angle) * radius;
        float z = center.z + Mathf.Sin(angle) * radius;

        // Apply new position
        transform.position = new Vector3(x, transform.position.y, z);

        // Apply rotation
        Quaternion rotation = Quaternion.Euler(0, -angle * Mathf.Rad2Deg, 0);
        transform.rotation = rotation;

    }
}
