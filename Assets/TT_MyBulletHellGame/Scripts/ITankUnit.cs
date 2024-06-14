using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITankUnit
{
    public Transform GetTankTurret();
    public Transform GetTankHull();
    public Color GetTankColor();
}
