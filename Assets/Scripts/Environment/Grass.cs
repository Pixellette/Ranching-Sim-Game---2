using UnityEngine;

public class Grass : MonoBehaviour
{
    public float maxHeight = 2f; // Max height of grass
    public float minHeight = 0.1f; // Minimum height of grass
    public float growthRate = 0.01f; // Speed at which grass grows
    public float hungerThreshold = 0.5f; // Height threshold for Boids to eat
    public float grassChonkiness = 4;

    [SerializeField] private float currentHeight;
    private bool isGrowing = true;

    void Start()
    {
        currentHeight = Random.Range(minHeight, maxHeight);
        transform.localScale = new Vector3(1, currentHeight, 1);
    }

    void Update()
    {
        if (isGrowing)
        {
            GrowGrass();
        }
    }

    private void GrowGrass()
    {
        if (currentHeight < maxHeight)
        {
            currentHeight += growthRate * Time.deltaTime;
            transform.localScale = new Vector3(grassChonkiness, Mathf.Min(currentHeight, maxHeight), grassChonkiness);
        }
    }

    public void CutGrass(float amount)
    {
        if (currentHeight > minHeight)
        {
            currentHeight = Mathf.Max(currentHeight - amount, minHeight);
            transform.localScale = new Vector3(grassChonkiness, currentHeight, grassChonkiness);
        }
    }

    public bool IsTallEnough()
    {
        return currentHeight >= hungerThreshold;
    }
}
