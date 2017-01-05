using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Server : NetworkController
{
    protected IPAddress _client = null;
    protected float _lastTime;
    protected bool _shouldDisconnectClient = false;

    protected override void OnDataRead(byte[] bytes, int length)
    {
        if(length > 0)
        {
            Debug.Log(GameController.Me.Seconds.ToString() + " Server::OnDataRead");
            if (bytes[0] == STATICS.SYMBOL_PLAYER_CONNECTED && _client == null)
            {
                byte[] ip = new byte[sizeof(int)];
                _lastTime = BitConverter.ToSingle(bytes, 1);
                Array.Copy(bytes, 1 + sizeof(float), ip, 0, sizeof(int));
                _client = new IPAddress(ip);

                Debug.Log(GameController.Me.Seconds.ToString() + " Server::ClientConnected");

                byte[] ack = new byte[1];
                ack[0] = STATICS.SYMBOL_ACCEPT_PLAYER;
                SendData(ack, 1, _client);

                GameController.Me.StartGame();
            }
            else if(bytes[0] == STATICS.SYMBOL_PLAYER_DISCONNECTED && _client != null)
            {
                byte[] ip = new byte[sizeof(int)];
                _lastTime = BitConverter.ToSingle(bytes, 1);
                Array.Copy(bytes, 1 + sizeof(float), ip, 0, sizeof(int));
                IPAddress comp = new IPAddress(ip);
                if (comp.Equals(_client))
                {
                    Debug.Log(GameController.Me.Seconds.ToString() + " Server::ClientDisconnected");
                    _shouldDisconnectClient = true;
                }
            }
            else
            {
                ClientPacket cp = ClientPacket.FromRawData(bytes);
                if(cp != null)
                {
                    Debug.Log(GameController.Me.Seconds.ToString() + " Server::ClientPacketReceived");
                    _lastTime = cp.Timestamp;
                    GameController.Me.SetFromClientPacket(cp);
                }
            }
        }
    }

    protected void ForceDisconnection(bool sendForceDisconnectPacket)
    {
        if(_client == null)
        {
            // Client already disconnected
            return;
        }

        Debug.Log(GameController.Me.Seconds.ToString() + " Server::ForceDisconnection");
        if (sendForceDisconnectPacket)
        {
            byte[] forceDisconnectData = new byte[1];
            forceDisconnectData[0] = STATICS.SYMBOL_FORCE_DISCONNECT;
            SendData(forceDisconnectData, 1, _client);
        }
        _client = null;

        GameController.Me.ClientLost();
    }

    protected override void OnApplicationQuit()
    {
        ForceDisconnection(true);
        base.OnApplicationQuit();
    }

    protected void LateUpdate()
    {
        if (GameController.Me.CurrentGameState == GameState.Game)
        {
            if (_client != null && (GameController.Me.Seconds - _lastTime) > _timeout)
            {
                Debug.Log(GameController.Me.Seconds.ToString() + " Server::Timeout");
                ForceDisconnection(true);
            }

            if(_client != null && _shouldDisconnectClient)
            {
                _shouldDisconnectClient = false;
                ForceDisconnection(false);
            }

            byte[] sendData = ServerPacket.ToRawData(GameController.Me.Players, GameController.Me.Ball);
            if (_client != null)
            {
                Debug.Log(GameController.Me.Seconds.ToString() + " Server::SendData");
                SendData(sendData, sendData.Length, _client);
            }
        }
    }
}