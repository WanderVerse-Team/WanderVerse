using System;
using UnityEngine;

public class FruitIdentity : MonoBehaviour
{
    public String fruitType;
    public int fruitValue;

    public HungryGolemController controller;

    private void OnDestroy()
    {
        // Tell the controller "I am gone, you can spawn another one!"
        if (controller != null)
        {
            controller.RemoveFruit(this);
        }
    }

    void OnBecameInvisible()
{
    if (!Application.isPlaying) return;

    // Check if THIS object still exists before destroying it
    if (gameObject != null)
    {
        Destroy(gameObject);
    }
}
}
