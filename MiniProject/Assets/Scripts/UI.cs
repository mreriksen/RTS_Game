using System.Resources;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public GameObject ResourceBar; 
    public Text Gold;               
    public Text Lumber;             
    public Text Food;               

    void FixedUpdate()
    {
        Gold.text = ResourceManager.goldAmount.ToString();
        Lumber.text = ResourceManager.lumberAmount.ToString();
        Food.text = ResourceManager.food.ToString();
    }
}
