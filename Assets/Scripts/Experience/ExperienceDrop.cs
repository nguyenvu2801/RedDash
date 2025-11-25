using UnityEngine;

public class ExperienceDrop : MonoBehaviour
{
    public int value; // experience value this drop gives

    public void Init(int expValue)
    {
        value = expValue;
        gameObject.SetActive(true);
    }

    // Call this when player collects
    public void Collect()
    {
        ExperienceManager.Instance.AddExperience(value);
        PoolManager.Instance.ReturnToPool(PoolKey.experience, gameObject);
    }
}