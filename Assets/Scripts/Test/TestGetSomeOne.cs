using UnityEngine;
using TMPro;
public class TestGetSomeOne : MonoBehaviour
{
    public TMP_InputField inputField;
    public void GetFish()
    {
        PlayerManager.Instance.TestGatherSomeAnimalToWareHouse(inputField.text);
    }
    public void GetPlant()
    {
        PlayerManager.Instance.TestGatherSomePlantToWareHouse(inputField.text);
    }
}
