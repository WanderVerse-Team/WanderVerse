using UnityEngine;
using System.Collections;
using TMPro;

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

    [Header("--- UI Setup ---")]
    public TextMeshProUGUI scoreText;

    [Header("--- Audio Setup ---")]
public AudioSource audioSource; 
public AudioClip eatSound;      
public AudioClip spawnSound;    
public AudioClip wrongSound;

    private float nextSpawnTime;
    private int fruitsNearMouth = 0;

    // 1. Tell the framework we are a Spawner game
    protected override GameType SupportedGameType => GameType.Spawner;

    protected override void Start()
{
    // 1. Run Randiv's BaseLevelController logic 
    // This turns on isGameActive, sets the timer, and loads the score!
    base.Start(); 
    UpdateScoreUI();
    

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
    // 1. Pick the prefab (same logic as before)
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
    
public override void ValidateDrop(GameObject item, GameObject zone)
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
            UpdateScoreUI();
            // Play the gulp!
            audioSource.PlayOneShot(eatSound);
            StartCoroutine(GulpRoutine());
        }
        else
        {
            // If value is 2, 5, etc., it's WRONG
            HandleWrongAnswer();
            
            // Optional: Penalty (Resets score)
            currentScore = 0; 
            Debug.Log($"<color=red>TOO MUCH!</color> The Golem can't eat {fruit.fruitValue} fruits at once!");
            UpdateScoreUI();

            // Play the mistake sound!
            audioSource.PlayOneShot(wrongSound);
        }
    }


}

private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            
            scoreText.text = $"Fruits: {currentScore} / {levelData.targetScore}";
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
}