using System;
using UnityEngine;

public class FruitIdentity : MonoBehaviour
{
    public String fruitType;
    public int fruitValue;

    public HungryGolemController controller;

    private void OnDestroy()
    {
        // To notify the controller when this fruit is destroyed.
        if (controller != null)
        {
            controller.RemoveFruit(this);
        }
    }

    void OnBecameInvisible()
{
    if (!Application.isPlaying) return;

    // Check if this object still exists before destroying it
    if (gameObject != null)
    {
        Destroy(gameObject);
    }
}
}
