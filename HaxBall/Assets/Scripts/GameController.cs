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
    public Vector3 InitPosition;
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
    protected NetworkController _networkController;
    protected Player _clientPlayer;
    protected Vector3[] _playersPositionsFromPacket;
    protected Vector3 _ballPositionFromPacket;
    protected Vector2 _movementFromPacket;
    protected bool _shootFromPacket;

    protected bool _shouldSetActive;
    protected bool _shouldResetGame;
    protected bool _shouldReturnToMenu;
    protected bool _shouldUpdateClientPlayerFromPacket;
    protected bool _shouldUpdatePlayersDataFromPacket;

    // Initial game state
    protected Vector3 _ballInitPosition;

    private GameState _currentGameState;
    public GameState CurrentGameState
    {
        get { return _currentGameState; }
        protected set
        {
            if(_currentGameState != value)
            {
                _currentGameState = value;
                _shouldSetActive = true;

                switch(_currentGameState)
                {
                    case GameState.Game:
                        _shouldResetGame = true;
                        break;
                    case GameState.HostJoinMenu:
                        break;
                    case GameState.WaitingForPlayers:
                        break;
                    case GameState.WaitingForServer:
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

    public PlayersInfo[] Players
    {
        get { return _players; }
    }

    public Transform Ball
    {
        get { return _ball; }
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

        _networkController = gameObject.AddComponent<Server>();
        _networkController.Initialize(IPAddress.Any, STATICS.SERVER_PORT_LISTEN, STATICS.SERVER_PORT_SEND);
        Player hostPlayer = _playersDictionary[EPlayer.Player1].PlayerGO.AddComponent<LocalPlayer>();
        hostPlayer.PlayerIndex = EPlayer.Player1;
        _clientPlayer = _playersDictionary[EPlayer.Player2].PlayerGO.AddComponent<Player>();
        _clientPlayer.PlayerIndex = EPlayer.Player2;
    }

    public void StartGameAsClient(IPAddress hostIP)
    {
        Role = NetworkRole.Client;
        CurrentGameState = GameState.WaitingForServer;
        _networkController = gameObject.AddComponent<Client>();
        _networkController.Initialize(hostIP, STATICS.SERVER_PORT_SEND, STATICS.SERVER_PORT_LISTEN);
        Client c = (Client)_networkController;
        c.Connect(hostIP, _playersDictionary[EPlayer.Player2]);
    }

    public void ClientLost()
    {
        if(Role == NetworkRole.Client)
        {
            // Client can't loose client :V
            return;
        }

        ResetGameState();
        CurrentGameState = GameState.WaitingForPlayers;
    }

    public void StartGame()
    {
        CurrentGameState = GameState.Game;
    }

    public void BackToHostJoinMenu()
    {
        _shouldReturnToMenu = true;
    }

    public void SetFromServerPacket(ServerPacket packet)
    {
        if(Role == NetworkRole.Host)
        {
            // Do not set this on server
            return;
        }

        _ballPositionFromPacket = packet.BallPosition;
        foreach(ServerPacket.PlayerPacketData ppd in packet.PlayersInfo)
        {
            _playersPositionsFromPacket[(int)ppd.Index] = ppd.Position;
            _playersDictionary[ppd.Index].Score = ppd.Score;
        }
        _shouldUpdatePlayersDataFromPacket = true;
    }

    public void SetFromClientPacket(ClientPacket packet)
    {
        if(_clientPlayer == null)
        {
            Debug.Log("Cannot get player component");
            return;
        }


        _shootFromPacket = packet.ShootInput;
        _movementFromPacket = packet.MovementInput;
        _shouldUpdateClientPlayerFromPacket = true;
    }

    #endregion

    #region Private and protected methods

    private void ResetGameState()
    {
        _shouldResetGame = false;
        ResetBall();
        ResetPlayers();

        Score.text = string.Format("{0} : {1}", _playersDictionary[EPlayer.Player1].Score, _playersDictionary[EPlayer.Player2].Score);
    }

    private void ResetBall()
    {
        _ball.transform.position = _ballInitPosition;
        _ball.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    private void ResetPlayers()
    {
        foreach(EPlayer player in _playersDictionary.Keys)
        {
            _playersDictionary[player].PlayerTransform.position = _playersDictionary[player].InitPosition;
            _playersDictionary[player].PlayerGO.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    private void SetGameObjects()
    {
        _shouldSetActive = false;
        _waitingForPlayersMenuGO.SetActive(CurrentGameState == GameState.WaitingForPlayers);
        _waitingForServerMenuGO.SetActive(CurrentGameState == GameState.WaitingForServer);
        _hostJoinMenuGO.SetActive(CurrentGameState == GameState.HostJoinMenu);
        foreach(GameObject go in _gameGOs)
        {
            go.SetActive(CurrentGameState == GameState.Game);
        }
    }

    protected void ReturnToHostJoinMenu()
    {
        _shouldReturnToMenu = false;
        CurrentGameState = GameState.HostJoinMenu;
        Destroy(_networkController);
        _networkController = null;
    }

    protected void UpdatePositionsFromServerPacket()
    {
        _shouldUpdatePlayersDataFromPacket = false;
        _ball.position = _ballPositionFromPacket;
        foreach(EPlayer player in _playersDictionary.Keys)
        {
            _playersDictionary[player].PlayerTransform.position = _playersPositionsFromPacket[(int)player];
        }
        Score.text = string.Format("{0} : {1}", _playersDictionary[EPlayer.Player1].Score, _playersDictionary[EPlayer.Player2].Score);
    }

    protected void UpdateInputFromClientPacket()
    {
        _shouldUpdateClientPlayerFromPacket = false;
        if(_clientPlayer != null)
        {
            if(_shootFromPacket)
            {
                _clientPlayer.TryShoot();
            }

            _clientPlayer.AddMovement(_movementFromPacket);
        }
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

        _playersPositionsFromPacket = new Vector3[_playersDictionary.Keys.Count];
	}

    protected void Start()
    {
        _ballInitPosition = _ball.position;
        foreach (EPlayer player in _playersDictionary.Keys)
        {
            _playersDictionary[player].InitPosition = _playersDictionary[player].PlayerTransform.position;
        }

        CurrentGameState = GameState.Game;
        CurrentGameState = GameState.HostJoinMenu;
    }

    protected void Update()
    {
        if(_shouldSetActive)
        {
            SetGameObjects();
        }

        if(_shouldResetGame)
        {
            ResetGameState();
        }

        if(_shouldReturnToMenu)
        {
            ReturnToHostJoinMenu();
        }

        if(_shouldUpdateClientPlayerFromPacket)
        {
            UpdateInputFromClientPacket();
        }

        if(_shouldUpdatePlayersDataFromPacket)
        {
            UpdatePositionsFromServerPacket();
        }
    }

    #endregion
}
