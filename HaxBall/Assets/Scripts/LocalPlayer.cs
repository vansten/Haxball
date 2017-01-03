using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : Player
{
	protected override void Update ()
    {
        bool shoot = Input.GetButtonDown(_myInfo.ShootButtonName);
        if (shoot)
        {
            TryShoot();
        }

        Vector2 input = new Vector2(Input.GetAxis(_myInfo.HorizontalAxisName), Input.GetAxis(_myInfo.VerticalAxisName));

        AddMovement(input);

        base.Update();
    }
}
