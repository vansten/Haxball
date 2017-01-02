using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : NetworkController
{
    protected override void OnDataRead(byte[] bytes, int length)
    {
        if(length > 0)
        {
            ServerPacket sp = ServerPacket.FromRawData(bytes);
            if(sp != null)
            {
                Debug.Log(sp.ToString());
            }
        }
    }
}