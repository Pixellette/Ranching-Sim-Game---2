using UnityEngine;

public class Grass : MonoBehaviour
{
    public float maxHeight = 2f; // Max height of grass
    public float minHeight = 0.1f; // Minimum height of grass
    public float growthRate = 0.01f; // Speed at which grass grows
    public float hungerThreshold = 0.5f; // Height threshold for Boids to eat

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
            transform.localScale = new Vector3(1, Mathf.Min(currentHeight, maxHeight), 1);
        }
    }

    public void CutGrass(float amount)
    {
        if (currentHeight > minHeight)
        {
            Debug.Log("GRASS HAS BEEN CUT");
            currentHeight = Mathf.Max(currentHeight - amount, minHeight);
            transform.localScale = new Vector3(1, currentHeight, 1);
        }
    }

    public bool IsTallEnough()
    {
        return currentHeight >= hungerThreshold;
    }
}
