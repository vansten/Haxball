using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField]
    private EPlayer _playerIndex;

    private void OnTriggerEnter(Collider other)
    {
        GameController.Me.IncreaseScore((int)_playerIndex);
    }
}
