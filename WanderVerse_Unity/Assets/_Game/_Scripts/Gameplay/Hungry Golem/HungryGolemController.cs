using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;

public class HungryGolemController : BaseLevelController
{
    [Header("--- Golem Visuals ---")]
    public SpriteRenderer golemRenderer;
    public Sprite idleSprite;
    public Sprite openMouthSprite;

    [Header("--- Spawning Setup ---")]
    public Transform spawnLineLeft;  // Empty GameObject at top-left
    public Transform spawnLineRight; // Empty GameObject at top-right
    public GameObject mouthZone;     // The "Stomach" trigger area

    public int maxFruitsOnScreen = 5; // The Limit
    private int currentActiveFruits = 0; // The Counter

    

    [Header("--- Audio Setup ---")]
    public AudioSource audioSource; 
    public AudioClip eatSound;      
    public AudioClip spawnSound;    
    public AudioClip wrongSound;

    [Header("--- UI Signs ---")]
    public RectTransform wrongValuePanel; 
    public RectTransform overfeedPanel;
    public RectTransform underfeedPanel;

    [Header("--- Settings ---")]
    public float hiddenY = 800f; // Height above screen
    public float visibleY = 0f;  // Center of screen
    public float waitTime = 2.0f; // How long the sign stays visible

    private float nextSpawnTime;
    private int fruitsNearMouth = 0;
    private bool isProcessingMistake = false;

    // 1. Tell the framework we are a Spawner game
    protected override GameType SupportedGameType => GameType.Spawner;

    protected override void Start()
{
    // 1. Run Randiv's BaseLevelController logic 
    // This turns on isGameActive, sets the timer, and loads the score!
    base.Start(); 
    
    

    // 2. Now do your Golem-specific setup
    if (golemRenderer != null && idleSprite != null)
    {
        golemRenderer.sprite = idleSprite;
    }
}

    // 2. Setup the Golem based on the ScriptableObject
    protected override void InitializeLevel()
    {
        golemRenderer.sprite = idleSprite;
        nextSpawnTime = Time.time + levelData.spawnRate;
        Debug.Log($"Golem is hungry for {levelData.targetScore} {levelData.levelTitle}s");
    }

    // 3. Handle the spawning loop
    public override void Update()
    {
        base.Update(); // IMPORTANT: Keeps Randiv's timer running

        if (isGameActive)
        {
            if (Time.time >= nextSpawnTime)
            {
                SpawnFruit();
                nextSpawnTime = Time.time + levelData.spawnRate;
            }
        }
    }

    private void SpawnFruit()
{
    if (currentActiveFruits >= maxFruitsOnScreen) 
    {
        return; 
    }
    // 1. Pick the prefab 
    bool spawnCorrect = Random.value > 0.4f;
    GameObject prefabToSpawn = spawnCorrect ? 
        levelData.spawnItems[Random.Range(0, levelData.spawnItems.Count)] : 
        levelData.distractors[Random.Range(0, levelData.distractors.Count)];

    // 2. THE AREA LOGIC
    // We pick a random number between the Left point and the Right point
    float randomX = Random.Range(spawnLineLeft.position.x, spawnLineRight.position.x);

    // 3. Spawn the fruit at that random X, but keep the Height (Y) from your line
    Vector3 spawnPos = new Vector3(randomX, spawnLineLeft.position.y, 0);
    GameObject fruit = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

    // 4. Set the speed
    Rigidbody2D rb = fruit.GetComponent<Rigidbody2D>();
    if(rb != null) rb.linearVelocity = Vector2.down * levelData.itemFallSpeed;

    if(audioSource != null && spawnSound != null)
    {
        audioSource.PlayOneShot(spawnSound);
    }

    FruitIdentity fruitScript = fruit.GetComponent<FruitIdentity>();
    if (fruitScript != null)
    {
        fruitScript.controller = this; 
    }

    // Increase the count
    currentActiveFruits++;
}
    // 4. The "State Swap" Logic
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fruit"))
        {
            fruitsNearMouth++;
            golemRenderer.sprite = openMouthSprite;
            
        }
    }

    // 2. Add a function to lower the count
public void RemoveFruit()
{
    currentActiveFruits--;
    
    // Safety check: prevent negative numbers if something glitches
    if (currentActiveFruits < 0) currentActiveFruits = 0;
}

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Fruit"))
        {
            fruitsNearMouth--;
            if (fruitsNearMouth <= 0)
            {
                fruitsNearMouth = 0;
                golemRenderer.sprite = idleSprite;
            }
        }
    }

    // 5. The Core Validation (Called by DragDropItem)
    
public  void ValidateDrop(GameObject item, GameObject zone)
{
    if (zone == null) return; 

    FruitIdentity fruit = item.GetComponent<FruitIdentity>();

    if (fruit != null)
    {
        // RULE: Golem only eats things worth EXACTLY 1
        if (fruit.fruitValue == 1)
        {
            HandleCorrectAnswer(); 
            Debug.Log($"<color=green>SUCCESS!</color> Ate {item.name}. Total Score: {currentScore}");
            
            // Play the gulp!
            audioSource.PlayOneShot(eatSound);
            StartCoroutine(GulpRoutine());
        }
        else
        {
            // If value is 2, 5, etc., it's WRONG
            HandleWrongAnswer();
            StartCoroutine(AutoSignSequence(wrongValuePanel));
            
            
            
            // Optional: Penalty (Resets score)
            currentScore = 0; 
            Debug.Log($"<color=red>TOO MUCH!</color> The Golem can't eat {fruit.fruitValue} fruits at once!");
            

            // Play the mistake sound!
            audioSource.PlayOneShot(wrongSound);
        }
    }


}




    private IEnumerator GulpRoutine()
{
    // 1. Change to the eating/open mouth image
    golemRenderer.sprite = openMouthSprite;

    // 2. Wait for a brief moment (0.4 seconds)
    yield return new WaitForSeconds(0.4f);

    // 3. If no more fruits are touching the Golem, go back to Idle
    if (fruitsNearMouth <= 0)
    {
        golemRenderer.sprite = idleSprite;
    }
}

public void CheckIfFinished()
{
    if (isProcessingMistake) return;
    if (currentScore == levelData.targetScore)
    {   
        CheckWinCondition();
        DoorLogic doorLogic = FindObjectOfType<DoorLogic>();
        doorLogic.OpenDoor();
        // SUCCESS: Play a 'Door Opening' sound or animation here!
        //ShowWinScreen();
        Debug.Log("The door creaks open as the Golem is perfectly fed!");
    }
    else if (currentScore < levelData.targetScore)
    {
        HandleCorrectAnswer();
        // Too few fruits
        StartCoroutine(AutoSignSequence(underfeedPanel));
        // Tip: Change the text to "The door is still locked! Did the Golem eat enough?"
        currentScore = 0;
        Debug.Log("The door is still locked! Did the Golem eat enough?");
    }
    else
    {
        HandleWrongAnswer();
        // Too many fruits (Overfed)
        StartCoroutine(AutoSignSequence(overfeedPanel));
        currentScore = 0;
    }
}

    private IEnumerator AutoSignSequence(RectTransform sign)
{
    if (sign == null) yield break;
    isProcessingMistake = true;

    // 1. Reset and Show
    currentScore = 0;
    
    sign.gameObject.SetActive(true);
    audioSource.PlayOneShot(wrongSound);

    // 2. Slide Down (From 800 to 0)
    yield return StartCoroutine(MoveSign(sign, hiddenY, visibleY));

    // 3. Wait
    yield return new WaitForSecondsRealtime(waitTime);

    // 4. Slide Up (From 0 back to 800)
    yield return StartCoroutine(MoveSign(sign, visibleY, hiddenY));

    // 5. Hide
    sign.gameObject.SetActive(false);
    
    isProcessingMistake = false;
}
    private IEnumerator MoveSign(RectTransform rect, float startY, float endY)
    {
        float duration = 0.5f;
        float elapsed = 0;
        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Smooth bounce/ease effect
            float curve = t * t * (3f - 2f * t); 
            rect.localPosition = new Vector2(0, Mathf.Lerp(startY, endY, curve));
            yield return null;
        }
    }
}
