using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class WaitingForPlayersMenuController : MonoBehaviour
{
    [SerializeField]
    protected Text _ipAddressText;

    void OnEnable()
    {
        string sHostName = Dns.GetHostName();
        IPHostEntry ipE = Dns.GetHostEntry(sHostName);
        IPAddress[] IpA = ipE.AddressList;
        _ipAddressText.text = "Server IP Addresses:";
        for (int i = 0; i < IpA.Length; i++)
        {
            if(IpA[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                _ipAddressText.text += "\n";
                _ipAddressText.text += IpA[i].ToString();
            }
        }
    }
}
