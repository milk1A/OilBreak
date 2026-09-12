using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GameManager instance;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool MoveUp()
    {
        bool Press = false;
        if (Keyboard.current.wKey.isPressed) Press = true;
        return Press;
    }
    public bool MoveDown()
    {
        bool Press = false;
        if (Keyboard.current.sKey.isPressed) Press = true;
        return Press;
    }
    public bool MoveLeft()
    {
        bool Press = false;
        if (Keyboard.current.aKey.isPressed) Press = true;
        return Press;
    }
    public bool MoveRight()
    {
        bool Press = false;
        if (Keyboard.current.dKey.isPressed) Press = true;
        return Press;
    }
    public bool PressJump()
    {
        bool Press = false;
        if (Keyboard.current.spaceKey.wasPressedThisFrame) Press = true;
        return Press;
    }
}
