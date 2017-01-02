using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Server : NetworkController
{
    protected List<IPAddress> _clients = new List<IPAddress>();

    protected override void OnDataRead(byte[] bytes, int length)
    {
        if(length > 0)
        {
            if(bytes[0] == STATICS.SYMBOL_PLAYER_CONNECTED)
            {
                byte[] ip = new byte[sizeof(int)];
                Array.Copy(bytes, 1, ip, 0, sizeof(int));
                IPAddress clientIP = new IPAddress(ip);
                _clients.Add(clientIP);

                GameController.Me.StartGame();
            }
        }
    }

    protected void LateUpdate()
    {
        byte[] sendData = ServerPacket.ToRawData(GameController.Me.Players, GameController.Me.Ball);
        foreach(IPAddress ip in _clients)
        {
            SendData(sendData, sendData.Length, ip);
        }
    }
}