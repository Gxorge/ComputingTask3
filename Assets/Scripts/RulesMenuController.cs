using UnityEngine;

public class RulesMenuController : MonoBehaviour
{
    public void StartButton()
    {
        GetComponent<Canvas>().enabled = false;
        GameController.Instance.BeginGame();
    }
}
