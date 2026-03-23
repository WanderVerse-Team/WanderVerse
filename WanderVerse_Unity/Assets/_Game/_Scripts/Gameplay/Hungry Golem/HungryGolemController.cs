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
    public Transform spawnLineLeft;  
    public Transform spawnLineRight; 
    public GameObject mouthZone;     

    public int maxFruitsOnScreen = 5; // Maximum fruits we can have on screen at once
    private int currentActiveFruits = 0; // Keeps track of how many fruits are currently active

    

    [Header("--- Audio Setup ---")]
    public AudioClip eatSound;      
    public AudioClip spawnSound;

    [Header("--- UI Signs ---")]
    public RectTransform wrongValuePanel; 
    public RectTransform overfeedPanel;
    public RectTransform underfeedPanel;

    [Header("--- Settings ---")]
    public float hiddenY = 800f; // Sign position when hidden above screen
    public float visibleY = 0f;  // Sign position when centered on screen
    public float waitTime = 2.0f; // How long warning signs stay on screen
    public float targetX = 0f;

    [Header("--- UI References ---")]
    public TMPro.TextMeshProUGUI instructionText;

    private float nextSpawnTime;
    private int fruitsNearMouth = 0;
    private bool isProcessingMistake = false;
    private int correctFruitsOnScreen = 0;

    [Header("--- Victory Screen ---")]
    public GameObject victoryPanel; 
    public ParticleSystem confetti;
    public ParticleSystem fireworks;

    
    protected override GameType SupportedGameType => GameType.Counting;

    protected override void Start()
{
     
    base.Start(); 
    Screen.orientation = ScreenOrientation.LandscapeLeft;
    confetti.Stop();
    fireworks.Stop();
    victoryPanel.SetActive(false);
    
    

    
    if (golemRenderer != null && idleSprite != null)
    {
        golemRenderer.sprite = idleSprite;
    }
}

    
    protected override void InitializeLevel()
    {
        golemRenderer.sprite = idleSprite;
        nextSpawnTime = Time.time + levelData.spawnRate;
        Debug.Log($"Golem is hungry for {targetScore} {levelData.levelTitle}s");
    }

    //Spawn fruits at regular intervals
    public override void Update()
    {
        base.Update(); 

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

    GameObject prefabToSpawn;

    if (correctFruitsOnScreen == 0)
    {
        int index = Random.Range(0, levelData.spawnItems.Count);
        prefabToSpawn = levelData.spawnItems[index];
        correctFruitsOnScreen++; 
    }
    else{
    //Chance to spawn a distractor
    bool spawnCorrect = Random.value < 0.3f;
        prefabToSpawn = spawnCorrect ? 
        levelData.spawnItems[Random.Range(0, levelData.spawnItems.Count)] : 
        levelData.distractors[Random.Range(0, levelData.distractors.Count)];
    }
    
    // Pick a random spot between the left and right boundaries to spawn
    float randomX = Random.Range(spawnLineLeft.position.x, spawnLineRight.position.x);

    // Create the fruit at this random position
    Vector3 spawnPos = new Vector3(randomX, spawnLineLeft.position.y, 0);
    GameObject fruit = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

    
    Rigidbody2D rb = fruit.GetComponent<Rigidbody2D>();
    if(rb != null) rb.linearVelocity = Vector2.down * levelData.itemFallSpeed;

    if(AudioManager.Instance != null) AudioManager.Instance.PlaySFX(spawnSound);

    FruitIdentity fruitScript = fruit.GetComponent<FruitIdentity>();
    if (fruitScript != null)
    {
        fruitScript.controller = this; 
    }

    
    currentActiveFruits++;
}
    // Show the golem opening its mouth when a fruit enters
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fruit"))
        {
            fruitsNearMouth++;
            golemRenderer.sprite = openMouthSprite;
            
        }
    }

    
public void RemoveFruit(FruitIdentity fruit)
{
    currentActiveFruits--;
    
    
    if (currentActiveFruits < 0) currentActiveFruits = 0;

    // Only count the right type of fruit 
    if (fruit.fruitValue == levelData.validValue)
    {
        correctFruitsOnScreen--;
        if (correctFruitsOnScreen < 0) correctFruitsOnScreen = 0;
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

    
    
public  void ValidateDrop(GameObject item, GameObject zone)
{
    if (zone == null) return; 

    FruitIdentity fruit = item.GetComponent<FruitIdentity>();

    if (fruit != null)
    {
        
        if (fruit.fruitValue == levelData.validValue)
        {
            HandleCorrectAnswer(); 
            Debug.Log($"<color=green>SUCCESS!</color> Ate {item.name}. Total Score: {currentScore}");

            
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(eatSound);
            StartCoroutine(GulpRoutine());
        }
        else
        {
            
            HandleWrongAnswer();
            StartCoroutine(AutoSignSequence(wrongValuePanel));
            
            
            
            
            currentScore = 0; 
            Debug.Log($"<color=red>TOO MUCH!</color> The Golem can't eat {fruit.fruitValue} fruits at once!");


                
                if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
        }
    }


}




    private IEnumerator GulpRoutine()
{
    // Change to the eating sprite briefly
    golemRenderer.sprite = openMouthSprite;

    
    yield return new WaitForSeconds(0.4f);

    
    if (fruitsNearMouth <= 0)
    {
        golemRenderer.sprite = idleSprite;
    }
}

public void CheckIfFinished()
{
    if (isProcessingMistake) return;
    if (currentScore == targetScore)
    {   
        CheckWinCondition();
        DoorLogic doorLogic = FindObjectOfType<DoorLogic>();
        doorLogic.OpenDoor();
        //VictoryScreen();
        Debug.Log("The door creaks open as the Golem is perfectly fed!");
    }
    else if (currentScore < targetScore)
    {
        HandleWrongAnswer();
        
        StartCoroutine(AutoSignSequence(underfeedPanel));
        
        currentScore = 0;
        Debug.Log("The door is still locked! Did the Golem eat enough?");
    }
    else
    {
        HandleWrongAnswer();
        StartCoroutine(AutoSignSequence(overfeedPanel));
        currentScore = 0;
    }
}

    private IEnumerator AutoSignSequence(RectTransform sign)
{
    if (sign == null) yield break;
    isProcessingMistake = true;

    Vector2 hiddenPos = new Vector2(targetX, hiddenY);
    Vector2 visiblePos = new Vector2(targetX, visibleY);

    currentScore = 0;
    sign.anchoredPosition = hiddenPos;
    sign.gameObject.SetActive(true);
    
    if (AudioManager.Instance != null) AudioManager.Instance.PlayError();

    // Animate the sign sliding down from above
    yield return StartCoroutine(MoveSign(sign, hiddenPos, visiblePos));

    yield return new WaitForSecondsRealtime(waitTime);

    // Slide it back up out of view
    yield return StartCoroutine(MoveSign(sign, visiblePos, hiddenPos));

    sign.gameObject.SetActive(false);
    isProcessingMistake = false;
}
private IEnumerator MoveSign(RectTransform rect, Vector2 startPos, Vector2 endPos)
{
    float duration = 0.5f;
    float elapsed = 0;

    while (elapsed < duration)
    {
        elapsed += Time.unscaledDeltaTime;
        float t = elapsed / duration;
        
        
        float curve = t * t * (3f - 2f * t); 
        
        rect.anchoredPosition = Vector2.Lerp(startPos, endPos, curve);
        
        yield return null;
    }
    rect.anchoredPosition = endPos;
}
    public void VictoryScreen()
{
    //Trigger confetti and fireworks
    if (confetti != null)
    {
        confetti.Play();
    }   
    if (fireworks != null)
    {
        fireworks.Play();
    }

    //Display the victory UI
    //victoryPanel.SetActive(true); 


    
}
}
