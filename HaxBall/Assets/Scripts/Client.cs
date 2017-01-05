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

    protected PlayersInfo _clientPlayer;
    protected float _lastTime;

    protected override void OnDataRead(byte[] bytes, int length)
    {
        if (length > 0)
        {
            Debug.Log(GameController.Me.Seconds.ToString() + " Cleint::OnDataRead");
            if (bytes[0] == STATICS.SYMBOL_FORCE_DISCONNECT)
            {
                Debug.Log(GameController.Me.Seconds.ToString() + " Client::ForceDisconnect");
                _lastTime = GameController.Me.Seconds;
                GameController.Me.BackToHostJoinMenu();
                return;
            }
            else if (bytes[0] == STATICS.SYMBOL_ACCEPT_PLAYER)
            {
                Debug.Log(GameController.Me.Seconds.ToString() + " Client::ClientAccepted");
                _lastTime = GameController.Me.Seconds;
                GameController.Me.StartGame();
            }
            else
            {
                ServerPacket sp = ServerPacket.FromRawData(bytes);
                if (sp != null)
                {
                    Debug.Log(GameController.Me.Seconds.ToString() + " Client::ServerPacketReceived");
                    _lastTime = sp.Timestamp;
                    GameController.Me.SetFromServerPacket(sp);
                }
            }
        }
    }
    
    public void Connect(IPAddress hostIP, PlayersInfo clientPlayer)
    {
        _clientPlayer = clientPlayer;
        HostIP = hostIP;
        byte[] ipPacket = new byte[1 + sizeof(float) + sizeof(int)];
        ipPacket[0] = STATICS.SYMBOL_PLAYER_CONNECTED;
        IPAddress myIP = GetClientIP();
        if(myIP == null)
        {
            return;
        }
        Debug.Log(GameController.Me.Seconds.ToString() + " Client::Connect");
        byte[] timestampBytes = BitConverter.GetBytes(GameController.Me.Seconds);
        Array.Copy(timestampBytes, 0, ipPacket, 1, sizeof(float));
        Array.Copy(myIP.GetAddressBytes(), 0, ipPacket, 1 + sizeof(float), sizeof(int));

        SendData(ipPacket, ipPacket.Length, HostIP);
    }

    public void Disconnect()
    {
        if(HostIP == null)
        {
            return;
        }
        byte[] ipPacket = new byte[1 + sizeof(float) + sizeof(int)];
        ipPacket[0] = STATICS.SYMBOL_PLAYER_DISCONNECTED;
        IPAddress myIP = GetClientIP();
        if (myIP == null)
        {
            return;
        }
        byte[] timestampBytes = BitConverter.GetBytes(GameController.Me.Seconds);
        Array.Copy(timestampBytes, 0, ipPacket, 1, sizeof(float));
        Debug.Log(GameController.Me.Seconds.ToString() + " Client::Disconnect");
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

    protected override void OnApplicationQuit()
    {
        Disconnect();
        base.OnApplicationQuit();
    }

    protected void Update()
    {
        if(HostIP != null)
        {
            if (GameController.Me.CurrentGameState == GameState.WaitingForServer)
            {
                Connect(HostIP, _clientPlayer);
            }
            else if (GameController.Me.CurrentGameState == GameState.Game)
            {
                Debug.Log(GameController.Me.Seconds.ToString() + " Client::SendData");
                byte[] bytes = ClientPacket.ToRawData(_clientPlayer);
                SendData(bytes, bytes.Length, HostIP);
            }
        }

        if (GameController.Me.CurrentGameState == GameState.Game)
        {
            if (GameController.Me.Seconds - _lastTime > _timeout)
            {
                Debug.Log(GameController.Me.Seconds.ToString() + " Client::Timeout");
                Disconnect();
                GameController.Me.BackToHostJoinMenu();
            }
        }
    }
}