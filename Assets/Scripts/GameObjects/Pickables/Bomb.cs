using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour, IPickable
{

    [SerializeField] private SpriteRenderer bombSprite;
    void IPickable.Init()
    {
        throw new System.NotImplementedException();
    }

    void IPickable.PickUp()
    {
        throw new System.NotImplementedException();
    }

    void IPickable.Activate()
    {
        throw new System.NotImplementedException();
    }
}
