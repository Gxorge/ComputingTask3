using TMPro;
using UnityEngine;

public class SetupMenuController : MonoBehaviour
{
    private TMP_InputField _p1Input;
    private TMP_InputField _p2Input;
    
    public void Start()
    {
        _p1Input = GameObject.Find("PlayerOneInput").GetComponent<TMP_InputField>();
        _p2Input = GameObject.Find("PlayerTwoInput").GetComponent<TMP_InputField>();
    }

    public void StartButtonPress()
    {
        if (_p1Input.text == "" || _p2Input.text == "" || _p1Input.text.StartsWith(" ") ||
            _p2Input.text.StartsWith(" ") || _p1Input.text.ToLower() == _p2Input.text.ToLower())
        {
            Debug.Log("user didnt enter name or is the same");
            return;
        }

        // Set names
        GameController controller = GameController.Instance;
        controller.PlayerOne = _p1Input.text;
        controller.PlayerTwo = _p2Input.text;
        
        GetComponent<Canvas>().enabled = false;
        controller.RulesMenu.enabled = true;
    }
}
