using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HostJoinMenuController : MonoBehaviour
{
    public InputField HostIPInputField;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyUp(KeyCode.Return))
        {
            if(HostIPInputField.isFocused)
            {
                OnJoinButtonClick();
            }
        }
    }

    public void OnHostButtonClick()
    {
        GameController.Me.StartGameAsServer();
    }

    public void OnJoinButtonClick()
    {
        string ipAddress = HostIPInputField.text;
        try
        {
            System.Net.IPAddress hostIP = System.Net.IPAddress.Parse(ipAddress);
            if (hostIP != null && hostIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                GameController.Me.StartGameAsClient(hostIP);
            }
            else
            {
                Debug.LogError("Wrong ip!");
            }
        }
        catch(Exception e)
        {
            Debug.LogErrorFormat("Exception occured: {0}", e.Message);
        }
    }
}
