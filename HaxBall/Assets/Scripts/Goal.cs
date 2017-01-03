using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField]
    private EPlayer _playerIndex;

    private void OnTriggerEnter(Collider other)
    {
        if(GameController.Me.Role == NetworkRole.Host)
        {
            GameController.Me.IncreaseScore(_playerIndex);
        }
    }
}
