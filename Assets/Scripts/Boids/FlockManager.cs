using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public static FlockManager FM;
    GameObject sheepParent;
    GameObject cowParent;


    [Header("Spawn Settings")]
        public GameObject[] sheepVariants; // 0: Ram (Male), 1: Ewe (Female), 2: Lamb (Baby)
        public List<GameObject> allSheep = new List<GameObject>();
        public int numSheep;
        public int spawnRangeSheep;

        public GameObject[] cowVariants;   // 0: Bull (Male), 1: Cow (Female), 2: Calf (Baby)
        public List<GameObject> allCows = new List<GameObject>();
        public int numCow;
        public int spawnRangeCow;


        // Total counts for animals
        public int totalCows;
        public int totalSheep;
        public int totalAnimals; // Total of all animals (cows + sheep)


    [Header("Behaviour Settings")]
        [Range(1, 5)]
        public int minWait;

        [Range(1, 10)]
        public int maxWait;

        [Header("Wander Settings")]
        [SerializeField] public float wanderRadius = 10;
        [SerializeField] public float wanderDistance = 20;
        [SerializeField] public float wanderJitter = 1;

    [Header("Flocking Settings")]
        [Range(1.0f, 30.0f)]
        public float neighbourDistance;
        public float aheadDistance = 5;

        [Range(1, 100)]
        public int flockingChance;

        [Range(0.01f, 5.0f)]
        public float separationWeight;

        [Range(0.01f, 5.0f)]
        public float alignmentWeight;

        [Range(0.01f, 5.0f)]
        public float fleeAlignmentWeight;

        [Range(0.01f, 5.0f)]
        public float cohesionWeight;

        [Range(0.01f, 5.0f)]
        public float fleeCohesionWeight;

        [Range(0.01f, 5.0f)]
        public float fleeWeight;

    [Header("Animal Awareness Settings")]
        [Range(30.0f, 90.0f)]
        public float viewAngle;

        [Range(30.0f, 90.0f)]
        public float viewRange;

        [Range(1.0f, 30.0f)]
        public float senseRange;

    

    void Start()
    {
        // Create parent GameObjects for sheep and cows
        sheepParent = new GameObject("SheepParent");
        cowParent = new GameObject("CowParent");

        // Create and position all sheep (males and females only, higher chance of females)
        for (int i = 0; i < numSheep; i++)
        {
            Vector3 pos = GetRandomSpawnPosition(spawnRangeSheep);
            GameObject selectedSheepPrefab = SelectGenderPrefab(sheepVariants); // 80% chance of female
            GameObject newSheep = Instantiate(selectedSheepPrefab, pos, Quaternion.identity, sheepParent.transform);
            allSheep.Add(newSheep);
        }

        // Create and position all cows (males and females only, higher chance of females)
        for (int i = 0; i < numCow; i++)
        {
            Vector3 pos = GetRandomSpawnPosition(spawnRangeCow);
            GameObject selectedCowPrefab = SelectGenderPrefab(cowVariants); // 80% chance of female
            GameObject newCow = Instantiate(selectedCowPrefab, pos, Quaternion.identity, cowParent.transform);
            allCows.Add(newCow);
        }

        // Initialize total counts
        totalCows = allCows.Count;
        totalSheep = allSheep.Count;
        totalAnimals = totalCows + totalSheep;

        FM = this;
    }

    void Update()
    {
        
    }


    // Function to spawn animals (species is "sheep" or "cow", variant is 0: male, 1: female, 2: baby)
    public void SpawnAnimal(string species, int variantIndex, Vector3 location)
    {
        GameObject[] variants;

        if (species.ToLower() == "sheep")
        {
            variants = sheepVariants;
        }
        else if (species.ToLower() == "cattle")
        {
            variants = cowVariants;
        }
        else
        {
            Debug.LogError("Invalid species specified for spawning.");
            return;
        }

        GameObject selectedPrefab = variants[variantIndex];
        GameObject newAnimal = Instantiate(selectedPrefab, location, Quaternion.identity);

        // Assign the new animal to its respective parent and list
        if (species.ToLower() == "sheep")
        {
            newAnimal.transform.parent = sheepParent.transform;
            allSheep.Add(newAnimal);
            totalSheep++;
        }
        else if (species.ToLower() == "cattle")
        {
            newAnimal.transform.parent = cowParent.transform;
            allCows.Add(newAnimal);
            totalCows++;
        }

        // Update the total number of all animals
        totalAnimals = totalCows + totalSheep;
    }


    // Function to remove an animal (when it dies or is removed from the game)
    public void RemoveAnimal(GameObject animal, string species)
    {
        if (species.ToLower() == "sheep")
        {
            if (allSheep.Remove(animal))
            {
                Destroy(animal);
                totalSheep--;
            }
        }
        else if (species.ToLower() == "cattle")
        {
            if (allCows.Remove(animal))
            {
                Destroy(animal);
                totalCows--;
            }
        }
        else
        {
            Debug.LogError("Invalid species specified for removal.");
            return;
        }

        // Update the total number of all animals
        totalAnimals = totalCows + totalSheep;
    }


    // Helper function to select a gender prefab (80% chance of female, 20% chance of male)
    private GameObject SelectGenderPrefab(GameObject[] variants)
    {
        float chance = Random.Range(0f, 1f);
        if (chance < 0.8f)
        {
            return variants[1]; // Female
        }
        else
        {
            return variants[0]; // Male
        }
    }


    // Helper function to get a random spawn position
    private Vector3 GetRandomSpawnPosition(int spawnRange)
    {
        Vector3 pos = this.transform.position + new Vector3(
            Random.Range(-spawnRange, spawnRange),
            50f,
            Random.Range(-spawnRange, spawnRange));

        if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit))
        {
            pos.y = hit.point.y;
        }

        return pos;
    }


}
