using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

public class GameController : Singleton<GameController>
{
    [SerializeField]
    private PlayersInfo[] _players;

    [SerializeField]
    private Transform _ball;

    [SerializeField]
    private Text Score;

    private Dictionary<EPlayer, PlayersInfo> _playersDictionary;

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

	// Use this for initialization
	public override void Awake()
    {
        base.Awake();

        _playersDictionary = new Dictionary<EPlayer, PlayersInfo>();

        foreach(PlayersInfo pi in _players)
        {
            _playersDictionary.Add(pi.PlayerIndex, pi);
            _playersDictionary[pi.PlayerIndex].Score = 0;
        }

        ResetGameState();
	}
}
