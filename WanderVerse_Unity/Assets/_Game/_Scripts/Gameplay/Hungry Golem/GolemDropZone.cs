// using UnityEngine;

// public class GolemDropZone : MonoBehaviour
// {
//     public HungryGolemController controller;

    
//     private void OnTriggerEnter2D(Collider2D collision)
//     {
//         FruitIdentity fruit = collision.GetComponent<FruitIdentity>();
//         DragDropItem dragItem = collision.GetComponent<DragDropItem>();

//         // If it's a draggable fruit...
//         if (fruit != null && dragItem != null)
//         {
//             // Open the mouth because food is nearby!
//             controller.golemRenderer.sprite = controller.openMouthSprite;
            
            
//             Destroy(collision.gameObject); // Chomp!
            
//         }
//     }

//     private void OnTriggerExit2D(Collider2D collision)
//     {
//         // Close the mouth if the player drags the fruit away without dropping it
//         if (collision.GetComponent<FruitIdentity>() != null)
//         {
//             controller.golemRenderer.sprite = controller.idleSprite; 
//         }
//     }
// }