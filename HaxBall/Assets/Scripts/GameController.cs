using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

#region Helper classes, structs and enums

[System.Serializable]
public class PlayersInfo
{
    public EPlayer PlayerIndex;
    public GameObject PlayerGO;
    public Transform PlayerTransform;
    public string HorizontalAxisName;
    public string VerticalAxisName;
    public string ShootButtonName;
    [HideInInspector]
    public int Score;
}

public enum NetworkRole
{
    Host,
    Client
}

public enum GameState
{
    HostJoinMenu,
    Game,
    WaitingForPlayers,
    WaitingForServer
}

#endregion

public class GameController : Singleton<GameController>
{
    #region Consts

    private const int _protocolPort = 3001;

    #endregion

    #region Variables and Properties

    [SerializeField]
    private PlayersInfo[] _players;
    [SerializeField]
    private Transform _ball;
    [SerializeField]
    private Text Score;
    [SerializeField]
    protected GameObject _hostJoinMenuGO;
    [SerializeField]
    protected GameObject _waitingForPlayersMenuGO;
    [SerializeField]
    protected GameObject _waitingForServerMenuGO;
    [SerializeField]
    protected GameObject[] _gameGOs;

    private Dictionary<EPlayer, PlayersInfo> _playersDictionary;

    // Networking
    private Socket _clientSocket;
    private EndPoint _clientEndPoint;

    private Socket _serverSocket;
    private EndPoint _serverEndPoint;
    private byte[] _receivedBytes;

    private GameState _currentGameState;
    public GameState CurrentGameState
    {
        get { return _currentGameState; }
        protected set
        {
            if(_currentGameState != value)
            {
                _currentGameState = value;
                SetGameObjects();

                switch (_currentGameState)
                {
                    case GameState.Game:
                        ResetGameState();
                        break;
                    case GameState.HostJoinMenu:
                        break;
                    case GameState.WaitingForPlayers:
                        break;
                }
            }
        }
    }

    public NetworkRole Role
    {
        get;
        protected set;
    }

    #endregion

    #region Public methods

    public PlayersInfo GetInfo(EPlayer playerIndex)
    {
        if(_playersDictionary.ContainsKey(playerIndex))
        {
            return _playersDictionary[playerIndex];
        }

        return null;
    }

    public void IncreaseScore(EPlayer index)
    {
        if (!_playersDictionary.ContainsKey(index))
        {
            return;
        }

        ++_playersDictionary[index].Score;
        ResetGameState();
    }

    public void StartGameAsServer()
    {
        Role = NetworkRole.Host;
        CurrentGameState = GameState.WaitingForPlayers;

        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _serverEndPoint = new IPEndPoint(IPAddress.Any, _protocolPort);
        _receivedBytes = new byte[1];
        _serverSocket.Bind(_serverEndPoint);
        _serverSocket.BeginReceiveFrom(_receivedBytes, 0, _receivedBytes.Length, SocketFlags.None, ref _serverEndPoint, new AsyncCallback(MessageReceivedCallback), (object)this);
    }
    public void StartGameAsClient(IPAddress hostIP)
    {
        Role = NetworkRole.Client;
        CurrentGameState = GameState.WaitingForServer;
        TryToConnect(hostIP);
    }

    #endregion

    #region Private and protected methods

    private void ResetGameState()
    {
        ResetBall();
        ResetPlayers();

        Score.text = string.Format("{0} : {1}", _playersDictionary[EPlayer.Player1].Score, _playersDictionary[EPlayer.Player2].Score);
    }

    private void ResetBall()
    {
        _ball.transform.localPosition = new Vector3(2.0f, 1.0f, 9.0f);
        _ball.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    private void ResetPlayers()
    {
        _players[0].PlayerTransform.localPosition = new Vector3(-8.0f, 1.5f, 9.0f);
        _players[0].PlayerGO.GetComponent<Rigidbody>().velocity = Vector3.zero;

        _players[1].PlayerTransform.localPosition = new Vector3(12.0f, 1.5f, 9.0f);
        _players[1].PlayerGO.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private void SetGameObjects()
    {
        _waitingForPlayersMenuGO.SetActive(CurrentGameState == GameState.WaitingForPlayers);
        _waitingForServerMenuGO.SetActive(CurrentGameState == GameState.WaitingForServer);
        _hostJoinMenuGO.SetActive(CurrentGameState == GameState.HostJoinMenu);
        foreach(GameObject go in _gameGOs)
        {
            go.SetActive(CurrentGameState == GameState.Game);
        }
    }
    
    private void TryToConnect(IPAddress hostIP)
    {
        if(Role == NetworkRole.Host)
        {
            //Trying to connect with host while being host isn't very wise
            return;
        }

        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _clientEndPoint = new IPEndPoint(hostIP, _protocolPort);
        byte[] buffer = new byte[1];
        buffer[0] = 1;
        _clientSocket.SendTo(buffer, 1, SocketFlags.None, _clientEndPoint);
    }

    private void MessageReceivedCallback(IAsyncResult result)
    {
        EndPoint remoteEndPoint = new IPEndPoint(0, 0);
        try
        {
            int bytesRead = _serverSocket.EndReceiveFrom(result, ref remoteEndPoint);
            foreach(byte b in _receivedBytes)
            {
                Debug.Log(b);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("Error: {0} {1}", e.ErrorCode, e.Message);
        }

        _serverSocket.BeginReceiveFrom(_receivedBytes, 0, _receivedBytes.Length,
            SocketFlags.None, ref _serverEndPoint,
            new AsyncCallback(MessageReceivedCallback), (object)this);
    }


    private void ReceiveDataFromClients()
    {

    }

    private void SendDataToHost()
    {

    }

    private void SendDataToClients()
    {

    }

    #endregion

    #region Unity methods

    public override void Awake()
    {
        base.Awake();

        _playersDictionary = new Dictionary<EPlayer, PlayersInfo>();

        foreach(PlayersInfo pi in _players)
        {
            _playersDictionary.Add(pi.PlayerIndex, pi);
            _playersDictionary[pi.PlayerIndex].Score = 0;
        }
	}

    protected void Start()
    {
        CurrentGameState = GameState.Game;
        CurrentGameState = GameState.HostJoinMenu;
    }

    protected void Update()
    {
        if(CurrentGameState == GameState.Game)
        {
            if(Role == NetworkRole.Host)
            {
                ReceiveDataFromClients();
            }
            else
            {
                SendDataToHost();
            }
        }
    }

    protected void LateUpdate()
    {
        if (CurrentGameState == GameState.Game)
        {
            if (Role == NetworkRole.Host)
            {
                SendDataToClients();
            }
        }
    }

    #endregion
}
