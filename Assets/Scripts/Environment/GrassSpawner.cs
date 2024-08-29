using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public GameObject grassPrefab;
    public int numberOfGrass = 100;
    public Terrain terrain;
    [SerializeField] float grassOffsetHeight;
    public Transform grassParent; // Parent GameObject for the spawned grass

    void Start()
    {
        SpawnGrass();
    }

    void SpawnGrass()
    {
        for (int i = 0; i < numberOfGrass; i++)
        {
            float x = Random.Range(0, terrain.terrainData.size.x);
            float z = Random.Range(0, terrain.terrainData.size.z);
            float y = terrain.SampleHeight(new Vector3(x, 0, z)) + grassOffsetHeight; // Adjust height offset as needed

            Vector3 position = new Vector3(x, y, z);
            GameObject grass = Instantiate(grassPrefab, position, Quaternion.identity);
            
            // Set the parent of the spawned grass to the specified parent object
            if (grassParent != null)
            {
                grass.transform.SetParent(grassParent);
            }
        }
    }
}
