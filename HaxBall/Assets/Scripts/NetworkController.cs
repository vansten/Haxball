using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public struct STATICS
{
    public const int MAX_MESSAGE_LENGTH = 1024;
    public const int SERVER_PORT_LISTEN = 3001;
    public const int SERVER_PORT_SEND = 3002;

    public const int SYMBOL_PLAYER_DISCONNECTED = 0x07;
    public const int SYMBOL_PLAYER_CONNECTED = 0x08;
    public const int SYMBOL_INFO = 0x09;
    public const int SYMBOL_DATA = 0x10;
    public const int SYMBOL_FORCE_DISCONNECT = 0x11;
    public const int SYMBOL_ACCEPT_PLAYER = 0x12;
}

//Packet from server to clients
public class ServerPacket
{
    public const int DATA_OFFSET = 1 + 3 * sizeof(int);
                                        //Index         Position            Score
    public const int ONE_PLAYER_OFFSET = sizeof(int) + 3 * sizeof(float) + sizeof(int);

    public struct PlayerPacketData
    {
        public EPlayer Index;
        public Vector3 Position;
        public int Score;
    }
    public PlayerPacketData[] PlayersInfo;
    public Vector3 BallPosition;
    public float Timestamp;

    public static byte[] ToRawData(PlayersInfo[] playersData, Transform ball)
    {
        byte[] toRet = new byte[STATICS.MAX_MESSAGE_LENGTH];

        toRet[DATA_OFFSET] = STATICS.SYMBOL_DATA;
        for(int i = 0; i < playersData.Length; ++i)
        {
            PlayersInfo pd = playersData[i];
            int offset = i * ONE_PLAYER_OFFSET;
            byte[] index = BitConverter.GetBytes((int)pd.PlayerIndex);
            byte[] posX = BitConverter.GetBytes(pd.PlayerTransform.position.x);
            byte[] posY = BitConverter.GetBytes(pd.PlayerTransform.position.y);
            byte[] posZ = BitConverter.GetBytes(pd.PlayerTransform.position.z);
            byte[] score = BitConverter.GetBytes(pd.Score);
            Array.Copy(index, 0, toRet, DATA_OFFSET + offset + 1, sizeof(int));
            Array.Copy(posX, 0, toRet, DATA_OFFSET + offset + 1 + sizeof(int), sizeof(float));
            Array.Copy(posY, 0, toRet, DATA_OFFSET + offset + 1 + sizeof(int) + sizeof(float), sizeof(float));
            Array.Copy(posZ, 0, toRet, DATA_OFFSET + offset + 1 + sizeof(int) + 2 * sizeof(float), sizeof(float));
            Array.Copy(score, 0, toRet, DATA_OFFSET + offset + 1 + sizeof(int) + 3 * sizeof(float), sizeof(int));
        }
        int pdOffset = playersData.Length * ONE_PLAYER_OFFSET + 1;
        byte[] ballPosX = BitConverter.GetBytes(ball.position.x);
        byte[] ballPosY = BitConverter.GetBytes(ball.position.y);
        byte[] ballPosZ = BitConverter.GetBytes(ball.position.z);
        Array.Copy(ballPosX, 0, toRet, DATA_OFFSET + pdOffset, sizeof(float));
        Array.Copy(ballPosY, 0, toRet, DATA_OFFSET + pdOffset + sizeof(float), sizeof(float));
        Array.Copy(ballPosZ, 0, toRet, DATA_OFFSET + pdOffset + 2 * sizeof(float), sizeof(float));

        int dataSize = pdOffset + 3 * sizeof(float);
        int checksum = 0;
        for(int i = 0; i < dataSize; ++i)
        {
            checksum += toRet[DATA_OFFSET + i];
        }
        checksum &= 0xFF;

        toRet[0] = STATICS.SYMBOL_INFO;
        byte[] checksumBytes = BitConverter.GetBytes(checksum);
        Array.Copy(checksumBytes, 0, toRet, 1, sizeof(int));
        byte[] timestampBytes = BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(Time.realtimeSinceStartup * 1000.0f), 0));
        Array.Copy(timestampBytes, 0, toRet, 5, sizeof(int));
        int players = playersData.Length;
        byte[] playersBytes = BitConverter.GetBytes(players);
        Array.Copy(playersBytes, 0, toRet, 9, sizeof(int));

        return toRet;
    }

    public static ServerPacket FromRawData(byte[] rawData)
    {
        ServerPacket p = new ServerPacket();
        
        if(rawData.Length < DATA_OFFSET + 1 || rawData[0] != STATICS.SYMBOL_INFO || rawData[DATA_OFFSET] != STATICS.SYMBOL_DATA)
        {
            Debug.Log("Cause length and symbols");
            return null;
        }

        int checksumRead = BitConverter.ToInt32(rawData, 1);
        p.Timestamp = BitConverter.ToSingle(rawData, 5) * 0.001f;
        int playersRead = BitConverter.ToInt32(rawData, 9);

        p.PlayersInfo = new PlayerPacketData[playersRead];
        for(int i = 0; i < playersRead; ++i)
        {
            p.PlayersInfo[i].Index = (EPlayer)BitConverter.ToInt32(rawData, DATA_OFFSET + i * ONE_PLAYER_OFFSET + 1);
            p.PlayersInfo[i].Position.x = BitConverter.ToSingle(rawData, DATA_OFFSET + i * ONE_PLAYER_OFFSET + 1 + sizeof(int));
            p.PlayersInfo[i].Position.y = BitConverter.ToSingle(rawData, DATA_OFFSET + i * ONE_PLAYER_OFFSET + 1 + sizeof(int) + sizeof(float));
            p.PlayersInfo[i].Position.z = BitConverter.ToSingle(rawData, DATA_OFFSET + i * ONE_PLAYER_OFFSET + 1 + sizeof(int) + 2*sizeof(float));
            p.PlayersInfo[i].Score = BitConverter.ToInt32(rawData, DATA_OFFSET + i * ONE_PLAYER_OFFSET + 1 + sizeof(int) + 3 * sizeof(float));
        }
        int ballDataOffset = playersRead * ONE_PLAYER_OFFSET + DATA_OFFSET + 1;
        p.BallPosition.x = BitConverter.ToSingle(rawData, ballDataOffset);
        p.BallPosition.y = BitConverter.ToSingle(rawData, ballDataOffset + sizeof(float));
        p.BallPosition.z = BitConverter.ToSingle(rawData, ballDataOffset + 2*sizeof(float));

        int checksumCalc = 0;
        int dataSize = ballDataOffset + 3 * sizeof(float);
        for (int i = 0; i < dataSize; ++i)
        {
            checksumCalc += rawData[DATA_OFFSET + i];
        }
        checksumCalc &= 0xFF;

        if(checksumCalc != checksumRead)
        {
            Debug.Log("Cause checksum");
            return null;
        }

        return p;
    }

    public override string ToString()
    {
        string s = "";

        foreach(PlayerPacketData ppd in PlayersInfo)
        {
            s += string.Format("Player {0}: {1}\n", ppd.Index.ToString(), ppd.Position.ToString());
        }
        s += string.Format("Ball: {0}", BallPosition.ToString());

        return s;
    }
}

//Packet from clients to server
public class ClientPacket
{
    public const int DATA_OFFSET = 1 + 2 * sizeof(int);
    public const int ONE_PLAYER_OFFSET = sizeof(int) + 3 * sizeof(float);

    public Vector2 MovementInput;
    public EPlayer PlayerIndex;
    public float Timestamp;
    public bool ShootInput;

    public static byte[] ToRawData(PlayersInfo playerData)
    {
        byte[] toRet = new byte[STATICS.MAX_MESSAGE_LENGTH];

        toRet[DATA_OFFSET] = STATICS.SYMBOL_DATA;
        byte[] playerIndexBytes = BitConverter.GetBytes((int)playerData.PlayerIndex);
        byte[] horizontalBytes = BitConverter.GetBytes(Input.GetAxis(playerData.HorizontalAxisName));
        byte[] verticalBytes = BitConverter.GetBytes(Input.GetAxis(playerData.VerticalAxisName));
        byte[] shootBytes = BitConverter.GetBytes(Input.GetButtonDown(playerData.ShootButtonName));

        Array.Copy(playerIndexBytes, 0, toRet, DATA_OFFSET + 1, sizeof(int));
        Array.Copy(horizontalBytes, 0, toRet, DATA_OFFSET + 1 + sizeof(int), sizeof(float));
        Array.Copy(verticalBytes, 0, toRet, DATA_OFFSET + 1 + sizeof(int) + sizeof(float), sizeof(float));
        Array.Copy(shootBytes, 0, toRet, DATA_OFFSET + 1 + sizeof(int) + 2 * sizeof(float), sizeof(bool));

        int checksum = 0;
        int dataSize = ONE_PLAYER_OFFSET;
        for (int i = 0; i < dataSize; ++i)
        {
            checksum += toRet[DATA_OFFSET + i];
        }
        checksum &= 0xFF;

        toRet[0] = STATICS.SYMBOL_INFO;
        byte[] checksumBytes = BitConverter.GetBytes(checksum);
        Array.Copy(checksumBytes, 0, toRet, 1, sizeof(int));
        byte[] timestampBytes = BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(Time.realtimeSinceStartup * 1000.0f), 0));
        Array.Copy(timestampBytes, 0, toRet, 5, sizeof(int));

        return toRet;
    }

    public static ClientPacket FromRawData(byte[] rawData)
    {
        ClientPacket p = new ClientPacket();

        if (rawData.Length < DATA_OFFSET + 1 || rawData[0] != STATICS.SYMBOL_INFO || rawData[DATA_OFFSET] != STATICS.SYMBOL_DATA)
        {
            Debug.Log("Cause length and symbols");
            return null;
        }

        int checksumRead = BitConverter.ToInt32(rawData, 1);
        p.Timestamp = BitConverter.ToSingle(rawData, 5) * 0.001f;

        p.PlayerIndex = (EPlayer)BitConverter.ToInt32(rawData, DATA_OFFSET + 1);
        float horizontal = BitConverter.ToSingle(rawData, DATA_OFFSET + 1 + sizeof(int));
        float vertical = BitConverter.ToSingle(rawData, DATA_OFFSET + 1 + sizeof(int) + sizeof(float));
        p.MovementInput = new Vector2(horizontal, vertical);
        p.ShootInput = BitConverter.ToBoolean(rawData, DATA_OFFSET + 1 + sizeof(int) + 2 * sizeof(float));

        int checksumCalc = 0;
        int dataSize = ONE_PLAYER_OFFSET;
        for (int i = 0; i < dataSize; ++i)
        {
            checksumCalc += rawData[DATA_OFFSET + i];
        }
        checksumCalc &= 0xFF;

        if (checksumCalc != checksumRead)
        {
            Debug.Log("Cause checksum");
            return null;
        }

        return p;
    }
}

public abstract class NetworkController : MonoBehaviour
{
    protected Socket _sendSocket;
    protected Socket _receiveSocket;
    protected EndPoint _sendEndPoint;
    protected EndPoint _receiveEndPoint;

    protected byte[] _receivedBytes = new byte[STATICS.MAX_MESSAGE_LENGTH];

    public void Initialize(IPAddress targetIP, int receivePort, int sendPort)
    {
        _sendEndPoint = new IPEndPoint(targetIP, sendPort);
        _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _receiveEndPoint = new IPEndPoint(IPAddress.Any, receivePort);
        _receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _receiveSocket.Bind(_receiveEndPoint);
        _receiveSocket.BeginReceiveFrom(_receivedBytes, 0, STATICS.MAX_MESSAGE_LENGTH, SocketFlags.None, ref _receiveEndPoint, MessageReceivedCallback, this);
    }

    public void SendData(byte[] bytes, int length, IPAddress ip)
    {
        ((IPEndPoint)_sendEndPoint).Address = ip;
        _sendSocket.SendTo(bytes, length, SocketFlags.None, _sendEndPoint);
    }

    protected void MessageReceivedCallback(IAsyncResult result)
    {
        try
        {
            int bytesRead = _receiveSocket.EndReceiveFrom(result, ref _receiveEndPoint);
            _receiveSocket.BeginReceiveFrom(_receivedBytes, 0, STATICS.MAX_MESSAGE_LENGTH, SocketFlags.None, ref _receiveEndPoint, MessageReceivedCallback, this);

            if (CheckData(_receivedBytes, bytesRead))
            {
                OnDataRead(_receivedBytes, bytesRead);
            }
        }
        catch(SocketException e)
        {
            Debug.LogError(e.Message);
        }
    }

    protected virtual void OnDataRead(byte[] bytes, int length)
    {
        Debug.Log("Data read");
        for(int i = 0; i < length; ++i)
        {
            Debug.Log(bytes[i]);
        }
    }

    protected bool CheckData(byte[] bytes, int length)
    {
        if (!(length > 0 && length <= STATICS.MAX_MESSAGE_LENGTH))
        {
            return false;
        }

        return true;
    }

    protected virtual void OnApplicationQuit()
    {
        _receiveSocket.Close();
        _receiveSocket.Dispose();
        _sendSocket.Close();
        _sendSocket.Dispose();
    }
}
