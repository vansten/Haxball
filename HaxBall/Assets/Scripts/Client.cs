using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : NetworkController
{
    [HideInInspector]
    public IPAddress HostIP;

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
    
    public void Connect(IPAddress hostIP)
    {
        HostIP = hostIP;
        byte[] ipPacket = new byte[1 + sizeof(int)];
        ipPacket[0] = STATICS.SYMBOL_PLAYER_CONNECTED;
        IPAddress myIP = GetClientIP();
        if(myIP == null)
        {
            return;
        }
        Array.Copy(myIP.GetAddressBytes(), 0, ipPacket, 1, sizeof(int));

        SendData(ipPacket, ipPacket.Length, HostIP);
    }

    protected IPAddress GetClientIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }

        return null;
    }
}