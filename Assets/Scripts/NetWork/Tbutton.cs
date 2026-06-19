using UnityEngine;
using UnityEngine.UI;

public class Tbutton : MonoBehaviour
{
    public Button button;
    public WebSocketServerManager server;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            server.SendMessage("Hello from Unity!");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
