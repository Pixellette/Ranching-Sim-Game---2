using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public GameObject grassPrefab;
    public int numberOfGrass = 100;
    public Terrain terrain;
    [SerializeField] float grassOffsetHeight;
    public Transform grassParent; // Parent GameObject for the spawned grass
    [SerializeField] float edgeOffset = 10f; // Offset distance from the terrain edges

    void Start()
    {
        SpawnGrass();
    }

    void SpawnGrass()
    {
        for (int i = 0; i < numberOfGrass; i++)
        {
            // Get terrain world position to adjust coordinate system
            Vector3 terrainPosition = terrain.transform.position;

            // Randomize within the terrain size, considering the edge offset
            float x = Random.Range(terrainPosition.x + edgeOffset, terrainPosition.x + terrain.terrainData.size.x - edgeOffset);
            float z = Random.Range(terrainPosition.z + edgeOffset, terrainPosition.z + terrain.terrainData.size.z - edgeOffset);

            // Sample height at the given x, z position, then add grass offset height
            float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPosition.y + grassOffsetHeight;
            Vector3 position = new Vector3(x, y, z);

            // Get the normal of the terrain at the sampled position for proper alignment
            Vector3 terrainNormal = terrain.terrainData.GetInterpolatedNormal((x - terrainPosition.x) / terrain.terrainData.size.x, (z - terrainPosition.z) / terrain.terrainData.size.z);
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, terrainNormal);

            // Instantiate grass with proper rotation
            GameObject grass = Instantiate(grassPrefab, position, rotation);
            
            // Set the parent of the spawned grass to the specified parent object
            if (grassParent != null)
            {
                grass.transform.SetParent(grassParent);
            }
        }
    }
}
