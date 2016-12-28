using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : Singleton<GameController>
{
    [SerializeField]
    private Transform[] _players;

    [SerializeField]
    private Transform _ball;

    private int _player1Score;
    private int _player2Score;

    [SerializeField]
    private Text Score;

    public void IncreaseScore(int index)
    {
        switch(index)
        {
            case 0:
                ++_player1Score;
                break;
            case 1:
                ++_player2Score;
                break;
        }
        ResetGameState();
    }

    private void ResetGameState()
    {
        ResetBall();
        ResetPlayers();

        Score.text = string.Format("{0} : {1}", _player1Score, _player2Score);
    }

    private void ResetBall()
    {
        _ball.transform.localPosition = new Vector3(2.0f, 1.5f, 9.0f);
        _ball.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    private void ResetPlayers()
    {
        _players[0].transform.localPosition = new Vector3(-8.0f, 1.5f, 9.0f);
        _players[0].gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

        _players[1].transform.localPosition = new Vector3(12.0f, 1.5f, 9.0f);
        _players[1].gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

	// Use this for initialization
	void Start () {
        ResetGameState();
	}
}
