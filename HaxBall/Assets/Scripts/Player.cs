using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum EPlayer
{
    Player1 = 0,
    Player2
}

public class Player : MonoBehaviour {

    [SerializeField]
    private EPlayer _playerIndex;
    private Rigidbody _myRigidBody;

    void Start()
    {
        _myRigidBody = gameObject.GetComponent<Rigidbody>();
    }

    //physics way 
    //private void FixedUpdate()
    //{
    //    Vector3 input;
    //    if(_playerIndex == EPlayer.Player1)
    //        input = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")) * Time.deltaTime * 100.0f;
    //    else
    //        input = new Vector3(Input.GetAxis("Horizontal2"), 0.0f, Input.GetAxis("Vertical2")) * Time.deltaTime * 100.0f;

    //    if (Vector3.Dot(_myRigidBody.velocity, input) < 0)
    //        _myRigidBody.velocity = Vector3.zero;

    //    _myRigidBody.AddForce(input, ForceMode.Impulse);
    //}

    //private void OnCollisionEnter(Collision collision)
    //{
    //    string name = collision.gameObject.tag;
    //    if (name == "Ball")
    //    {
    //        collision.rigidbody.AddForceAtPosition(_myRigidBody.velocity * 0.1f, collision.contacts[0].point, ForceMode.Impulse);
    //    }
    //    else if (name == "Player")
    //    {
    //        _myRigidBody.AddForceAtPosition(collision.relativeVelocity * 10.0f, collision.contacts[0].point, ForceMode.Impulse);
    //    }
    //}



    private GameObject _ball;

    private void Update()
    {
        bool shoot;
        Vector3 input;
        if (_playerIndex == EPlayer.Player1)
        {
            input = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
            shoot = Input.GetButtonDown("Jump");
        }
        else
        {
            input = new Vector3(Input.GetAxis("Horizontal2"), 0.0f, Input.GetAxis("Vertical2"));
            shoot = Input.GetButtonDown("Jump2");
        }

        if (shoot && _ball != null)
        {
            Vector3 direction = _ball.transform.position - gameObject.transform.position;
            direction.y = 0.0f;
            direction.Normalize();
            _ball.GetComponent<Rigidbody>().AddForce(direction * 2.0f, ForceMode.Impulse);
        }
        input.Normalize();
        input *= Time.deltaTime * 3.0f;
        transform.localPosition += input;
        _myRigidBody.velocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        string name = other.gameObject.tag;
        if (name == "Ball")
        {
            _ball = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        string name = other.gameObject.tag;
        if (name == "Ball")
        {
            _ball = null;
        }
    }
}
