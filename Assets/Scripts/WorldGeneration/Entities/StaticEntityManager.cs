using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class StaticEntityManager : MonoBehaviour {
    public GameObject[] regularTreePrefabs;
    public GameObject[] pinkTreePrefabs;
    public GameObject[] cactusPrefabs;

    public GameObject[] grassPrefabs;

    public void Spawn(List<Chunk> chunksToProcess) {
        foreach (Chunk chunk in chunksToProcess) {
            SpawnTree(chunk);
            chunk.hasEntitiesSpawned = true;
        }
    }

    public void SpawnTree(Chunk chunk) {
        float3 chunkSize = (float3)(WorldSettings.chunkDimension - 1) * WorldSettings.voxelSize;
        float x = chunk.id.ToWorldCoord().x + UnityEngine.Random.Range(0, chunkSize.x);
        float z = chunk.id.ToWorldCoord().z + UnityEngine.Random.Range(0, chunkSize.z);
        Vector3 rayStart = new Vector3(x, chunk.id.ToWorldCoord().y + chunkSize.y, z);

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, chunkSize.y, 1 << LayerMask.NameToLayer("Ground"))) {
            Vector3 offset = Vector3.down * Mathf.Abs(Vector3.Dot(hit.normal, Vector3.right)) * 2f;
            switch (BiomeController.GetBiome(new float2(hit.point.x, hit.point.z))) {
                case BiomeType.GRASS:
                    SpawnTree(regularTreePrefabs, chunk, hit.point + offset);
                    break;
                case BiomeType.PINK:
                    SpawnTree(pinkTreePrefabs, chunk, hit.point + offset);
                    break;
                case BiomeType.DESERT:
                    SpawnTree(cactusPrefabs, chunk, hit.point + offset);
                    break;
            }
        }
    }
    public void SpawnTree(GameObject[] treePrefabs, Chunk chunk, Vector3 position) {
        if (treePrefabs.Length == 0)
            return;
        GameObject treeObject = Instantiate(treePrefabs[UnityEngine.Random.Range(0, treePrefabs.Length)], position, Quaternion.identity);
        treeObject.tag = "StaticEntityRoot";
        treeObject.transform.parent = chunk.chunkGameObject.transform;
        treeObject.transform.Rotate(Vector3.up * UnityEngine.Random.Range(0, 360f));
        treeObject.transform.localScale *= UnityEngine.Random.Range(0.6f, 1);
    }

    public static void Deform(Vector3 hitPoint) {
        Collider[] staticEntityColliders = Physics.OverlapSphere(hitPoint, 1f, 1 << LayerMask.NameToLayer("Static Entity"));
        foreach (Collider staticEntityCollider in staticEntityColliders) {
            GameObject root = GetStaticEntityRoot(staticEntityCollider.gameObject);
            if (root && !root.GetComponent<Rigidbody>()) {
                root.AddComponent<Rigidbody>();
            }
        }
    }

    private static GameObject GetStaticEntityRoot(GameObject obj) {
        if (obj.tag.Equals("StaticEntityRoot")) {
            return obj;
        }
        if (obj.layer != LayerMask.NameToLayer("Static Entity")) {
            return null;
        }
        return GetStaticEntityRoot(obj.transform.parent.gameObject);
    }
}
