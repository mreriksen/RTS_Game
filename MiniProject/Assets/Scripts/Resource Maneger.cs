using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static int goldAmount;
    public static int lumberAmount;
    public static int food;
    public static int foodMax;
    // Start is called before the first frame update
    void Start()
    {
        //Give the initial resources
        goldAmount = 500;
        lumberAmount = 500;
        food = 5;
        foodMax = 120;
    }
}
