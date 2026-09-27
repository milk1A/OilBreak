using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoManager : MonoBehaviour
{
    [SerializeField] public GameObject normalPlayer;
    [SerializeField] public GameObject gymPlayer;
    [SerializeField] public Transform rotatingObject;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        rotatingObject.Rotate(Vector3.up * Time.deltaTime * 100);
    }

    public void BtnChangeCharacter()
    {
        if (normalPlayer.activeSelf)
        {
            normalPlayer.SetActive(false);
            gymPlayer.SetActive(true);
        }
        else
        {
            normalPlayer.SetActive(true);
            gymPlayer.SetActive(false);
        }
    }
}
