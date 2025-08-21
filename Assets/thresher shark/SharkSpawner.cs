using UnityEngine;

public class SharkSpawner : MonoBehaviour
{
    [Header("Prefab & Number & Range")]
    [SerializeField] public GameObject sharkPrefab;
    [SerializeField] public int initialCount;
    [SerializeField] public BoxCollider spawnBounds;
    [SerializeField] private bool randomRotation = true;

    void Start()
    {
        for (int i = 0; i < initialCount; i++)
            SpawnOneShark();
    }

    void SpawnOneShark()
    {
        Vector3 pos = RandomPointInBounds(spawnBounds);
        Quaternion rot = randomRotation ? Random.rotation : Quaternion.identity;
        GameObject shark = Instantiate(sharkPrefab, pos, rot, transform);
        var sb = shark.GetComponent<SharkBehaviour>();
    }

    Vector3 RandomPointInBounds(BoxCollider b)
    {
        Vector3 c = b.transform.position + b.center;
        Vector3 s = b.size * 0.5f;
        return new Vector3(
            Random.Range(c.x - s.x, c.x + s.x),
            Random.Range(c.y - s.y, c.y + s.y),
            Random.Range(c.z - s.z, c.z + s.z));
    }
}
