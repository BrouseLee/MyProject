using UnityEngine;

namespace MantaSim.Utilities
{
    public static class Utils
    {
        // Get random position in bounds
        public static Vector3 GetRandomPosition(Bounds b)
        {
            return new Vector3(Random.Range(b.min.x, b.max.x),
                               Random.Range(b.min.y, b.max.y),
                               Random.Range(b.min.z, b.max.z));
        }

        public static Vector3 GetRandomPointInsideCollider( this BoxCollider boxCollider )
        {
            Vector3 extents = boxCollider.size / 2f;
            Vector3 point = new Vector3(
                Random.Range( -extents.x, extents.x ),
                Random.Range( -extents.y, extents.y ),
                Random.Range( -extents.z, extents.z )
            );

            return boxCollider.transform.TransformPoint( point );
        }
    }
}