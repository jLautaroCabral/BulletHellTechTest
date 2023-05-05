using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RotateForeverNetworked : NetworkBehaviour
{
    private enum Axis { X, Y, Z };

    [SerializeField] private Axis axis = Axis.Y;
    [SerializeField] bool reverse = false;
    [SerializeField] public float _rotationsPerSecond = 1f;
    
    public bool ableToRotate { get; set; }
    
    [Networked]
    public float rotation { get; set; }

    public override void Spawned()
    {
        Rotate();
    }
    public override void Render()
    {
        if (ableToRotate)
        {
            Rotate();    
        }
    }

    void Rotate()
    {
        float direction = reverse == true ? -1 : 1;
        rotation = _rotationsPerSecond * 360f * Time.deltaTime * direction;

        switch (axis)
        {
            case Axis.X:
                transform.Rotate(rotation, 0, 0);
                break;
            case Axis.Y:
                transform.Rotate(0, rotation, 0);
                break;
            case Axis.Z:
                transform.Rotate(0, 0, rotation);
                break;
        }
    }
}
