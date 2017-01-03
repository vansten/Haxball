using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPlayer
{
    Player1 = 0,
    Player2
}

public class Player : MonoBehaviour
{
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

    [HideInInspector]
    public EPlayer PlayerIndex;

    protected PlayersInfo _myInfo;
    protected GameObject _ball;
    protected Rigidbody _myRigidBody;
    protected const float _speed = 5.0f;

    protected void Start()
    {
        _myRigidBody = gameObject.GetComponent<Rigidbody>();
        _myInfo = GameController.Me.GetInfo(PlayerIndex);
    }

    protected virtual void Update()
    {
        _myRigidBody.velocity = Vector3.zero;
    }

    protected void OnTriggerEnter(Collider other)
    {
        string name = other.gameObject.tag;
        if (name == "Ball")
        {
            _ball = other.gameObject;
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        string name = other.gameObject.tag;
        if (name == "Ball")
        {
            _ball = null;
        }
    }

    public void AddMovement(Vector2 movement)
    {
        movement.Normalize();
        movement *= Time.deltaTime * _speed;
        transform.localPosition += new Vector3(movement.x, 0, movement.y);
    }

    public void TryShoot()
    {
        if (_ball != null)
        {
            Vector3 direction = _ball.transform.position - gameObject.transform.position;
            direction.y = 0.0f;
            direction.Normalize();
            _ball.GetComponent<Rigidbody>().AddForce(direction * 2.0f, ForceMode.Impulse);
        }
    }
}
