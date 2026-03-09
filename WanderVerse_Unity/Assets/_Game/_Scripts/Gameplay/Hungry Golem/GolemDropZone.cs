// using UnityEngine;

// public class GolemDropZone : MonoBehaviour
// {
//     public HungryGolemController controller;

//     // OnTriggerStay continuously checks while an object is inside the zone
//     private void OnTriggerStay2D(Collider2D collision)
//     {
//         FruitIdentity fruit = collision.GetComponent<FruitIdentity>();
//         DragDropItem dragItem = collision.GetComponent<DragDropItem>();

//         // If it's a draggable fruit...
//         if (fruit != null && dragItem != null)
//         {
//             // Open the mouth because food is nearby!
//             controller.SetMouthState(true);

//             // ONLY eat it if it hasn't been counted AND the player let go!
//             if (!fruit.hasBeenCounted && dragItem.isReleased)
//             {
//                 fruit.hasBeenCounted = true; // Lock it so it can't double count
//                 controller.ConsumeFruit(fruit);
//                 Destroy(collision.gameObject); // Chomp!
//             }
//         }
//     }

//     private void OnTriggerExit2D(Collider2D collision)
//     {
//         // Close the mouth if the player drags the fruit away without dropping it
//         if (collision.GetComponent<FruitIdentity>() != null)
//         {
//             controller.SetMouthState(false); 
//         }
//     }
// }