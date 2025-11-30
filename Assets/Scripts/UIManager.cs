using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI birdsLeftText;

    public void SetBirdsLeft(int birdsLeft)
    {
        birdsLeftText.text = "Birds Left: " + birdsLeft;
    }
}
