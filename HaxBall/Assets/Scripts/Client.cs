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

    protected void OnApplicationQuit()
    {
        Disconnect();
    }

    protected override void OnDataRead(byte[] bytes, int length)
    {
        if(length > 0)
        {
            Debug.Log("Client received data!");
            if (bytes[0] == STATICS.SYMBOL_FORCE_DISCONNECT)
            {
                Debug.Log("Server lost!");
                GameController.Me.BackToHostJoinMenu();
                return;
            }
            ServerPacket sp = ServerPacket.FromRawData(bytes);
            if(sp != null)
            {
                Debug.Log(sp.ToString());
            }
        }
    }
    
    public void Connect(IPAddress hostIP)
    {
        Debug.Log("Trying to connect");
        HostIP = hostIP;
        byte[] ipPacket = new byte[1 + sizeof(float) + sizeof(int)];
        ipPacket[0] = STATICS.SYMBOL_PLAYER_CONNECTED;
        IPAddress myIP = GetClientIP();
        if(myIP == null)
        {
            return;
        }
        byte[] timestampBytes = BitConverter.GetBytes(Time.realtimeSinceStartup * 1000.0f);
        Array.Copy(timestampBytes, 0, ipPacket, 1, sizeof(float));
        Debug.Log("Connecting");
        Array.Copy(myIP.GetAddressBytes(), 0, ipPacket, 1 + sizeof(float), sizeof(int));

        SendData(ipPacket, ipPacket.Length, HostIP);
    }

    public void Disconnect()
    {
        Debug.Log("Trying to disconnect");
        if(HostIP == null)
        {
            return;
        }
        Debug.Log("Trying to disconnect 2");
        byte[] ipPacket = new byte[1 + sizeof(float) + sizeof(int)];
        ipPacket[0] = STATICS.SYMBOL_PLAYER_DISCONNECTED;
        IPAddress myIP = GetClientIP();
        if (myIP == null)
        {
            return;
        }
        byte[] timestampBytes = BitConverter.GetBytes(Time.realtimeSinceStartup * 1000.0f);
        Array.Copy(timestampBytes, 0, ipPacket, 1, sizeof(float));
        Debug.Log("Disconnecting");
        Array.Copy(myIP.GetAddressBytes(), 0, ipPacket, 1 + sizeof(float), sizeof(int));

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