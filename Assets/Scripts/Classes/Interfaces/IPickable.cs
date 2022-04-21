using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickable
{
    void Init(IPickableArgs args);
    void GenerateInScene(Vector3 position);

    void PickUp();

    void Activate();

    void Destroy();

    void CheckBounds();

}