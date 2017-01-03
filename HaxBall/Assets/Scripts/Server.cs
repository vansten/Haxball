using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Server : NetworkController
{
    protected IPAddress _client = null;
    protected float _lastTime;
    protected float _lastSendTime;

    protected override void OnDataRead(byte[] bytes, int length)
    {
        if(length > 0)
        {
            Debug.Log("Server received data!");
            if(bytes[0] == STATICS.SYMBOL_PLAYER_CONNECTED && _client == null)
            {
                byte[] ip = new byte[sizeof(int)];
                _lastTime = BitConverter.ToSingle(bytes, 1) * 0.001f;
                Array.Copy(bytes, 1 + sizeof(float), ip, 0, sizeof(int));
                _client = new IPAddress(ip);

                Debug.Log("Player connected " + _client.ToString());

                byte[] ack = new byte[1];
                ack[0] = STATICS.SYMBOL_ACCEPT_PLAYER;
                SendData(ack, 1, _client);

                GameController.Me.StartGame();
            }
            else if(bytes[0] == STATICS.SYMBOL_PLAYER_DISCONNECTED && _client != null)
            {
                byte[] ip = new byte[sizeof(int)];
                _lastTime = BitConverter.ToSingle(bytes, 1) * 0.001f;
                Array.Copy(bytes, 1 + sizeof(float), ip, 0, sizeof(int));
                IPAddress comp = new IPAddress(ip);
                if (comp == _client)
                {
                    Debug.Log("Player connected " + _client.ToString());
                    _client = null;
                }
            }
            else
            {
                ClientPacket cp = ClientPacket.FromRawData(bytes);
                if(cp != null)
                {
                    _lastTime = cp.Timestamp;
                    GameController.Me.SetFromClientPacket(cp);
                }
            }
        }
    }

    protected void ForceDisconnection()
    {
        if(_client == null)
        {
            // Client already disconnected
            return;
        }

        Debug.Log("Client lost!");
        byte[] forceDisconnectData = new byte[1];
        forceDisconnectData[0] = STATICS.SYMBOL_FORCE_DISCONNECT;
        SendData(forceDisconnectData, 1, _client);
        _client = null;

        GameController.Me.ClientLost();
    }

    protected void OnApplicationQuit()
    {
        ForceDisconnection();
    }

    protected void LateUpdate()
    {
        if(_client != null && Time.realtimeSinceStartup - _lastTime > 30.0f)
        {
            ForceDisconnection();
        }

        byte[] sendData = ServerPacket.ToRawData(GameController.Me.Players, GameController.Me.Ball);
        if(_client != null)
        {
            _lastSendTime = Time.realtimeSinceStartup;
            Debug.Log("Send data!");
            SendData(sendData, sendData.Length, _client);
        }
    }
}