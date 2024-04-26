//using System.Collections;
//using System.Collections.Generic;
//using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using UnityEngine;

public struct UpgradeEventCurrentInfo
{
    public int numOfSignOptions;
    public float startAnswerX;
    public float spaceBetweenAnswers;

    public UpgradeEventCurrentInfo(int numOfSignOptions, float startAnswerX, float spaceBetweenAnswers)
    {
        this.numOfSignOptions = numOfSignOptions;
        this.startAnswerX = startAnswerX;
        this.spaceBetweenAnswers = spaceBetweenAnswers;
    }
}

public class UpgradeEventManager : MonoBehaviour, IPausable
{
    #region //IPausable implementation

    private bool paused = false;

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    #endregion

    private enum Stage 
    {
        Waiting,
        Question, //Shows the source sign
        Answer, //presents three target signs (where one is equivalent to the source sign presented earlier)
        Feedback, // Shows the correct answer and upgrades the player
        ChoosingUpgrade
    }

    //Debugging
    [SerializeField] private bool debug;
    private string debugTag = "UpgradeEventManager: ";

    //Stage
    private Stage stage = Stage.Waiting;

    [Header("References")]
    [SerializeField] private UpgradeQuestionSignController questionController;
    [SerializeField] private GameObject signObjectPrefab;
    [SerializeField] private ParticleManager particleManager;
    [SerializeField] private ConfettiLauncher confettiLauncher;
    [HideInInspector] public LevelManager levelManager;
    [HideInInspector] public PlayerController playerController;
    private AudioManager audioManager;
    private SignSelector signSelector;

    #region //Info used to instantiate stuff

    [Header("Instantiate parameters")]
    [SerializeField] private int numOfSignOptions;
    [SerializeField] private float upgradeAnswerSpawnBorder;

    //Filled by Initialize
    private Vector3 spawnPosition;
    private float zLimit;
    private float floorWidth;
    //Calculated on Initialize
    private float startAnswerX;
    private float spaceBetweenAnswers;

    #endregion

    //Speed of the sign objects
    private float speed; //Filled by Initialize

    //Answer objects
    private GameObject[] answerObjects;
    
    //Correct answer index
    private SignCode[] currentSigns;
    private int correctAnswerIndex;
    private int selectedAnswerIndex;

    [Header("Answer Target Objects")]
    [SerializeField] private Color questionColor;
    [SerializeField] private Color[] answerColors;
    [SerializeField] private float timeToDestroyAnswerObjects;
    [SerializeField] private float distanceFromPlayerToDestroyAnswerObjects;
    [SerializeField] private AudioClip destroyAnswersSound;

    //Player targets
    [Header("Player Targets")]
    [SerializeField] private GameObject playerTargetPrefab;
    private GameObject[] playerTargets;
    [SerializeField] private float playerTargetHideY, playerTargetShowY, playerTargetSmoothMoveRatio;
    [SerializeField] private Material playerTargetMaterial, playerTargetHighlitedMaterial;

    [Header("Audio")]
    [SerializeField] private AudioClip correctAnswerSound;
    [SerializeField] private AudioClip wrongAnswerSound;

    [Header("Player Upgrades")]
    [SerializeField] private PlayerUpgrade[] upgradeList = new PlayerUpgrade[(int)PlayerStatus.Count];
    private PlayerUpgrade[] currentUpgradeSelection;
    private PlayerStatus selectedUpgrade;
    [HideInInspector] public bool rewardUpgrade;
    [SerializeField] public int pointsRewardWhenNoUpgrade;

    [SerializeField] private int numOfUpgradeOptions;
    [SerializeField] private int initialUpgradeWeight;
    [SerializeField] private int upgradeWeightIncrease;
    [SerializeField] private int upgradeWeightAfterShowed;
    [SerializeField] private int upgradeWeightAfterSelected;

    [SerializeField] private UpgradeSelection upgradeSelection;
    [SerializeField] private Panel upgradeSelectionPanel;

    //Store possible upgrades and probability
    [Serializable]
    public class PlayerUpgrade
    {
        public PlayerStatus status;
        public int rarity;
        public Sprite icon;
        public int weight;

        public static PlayerUpgrade GetRandom(PlayerUpgrade[] list, PlayerUpgrade[] ignore = null)
        {
            //Creates a copy and remove the ignore
            List<PlayerUpgrade> listCopy = new List<PlayerUpgrade>(list);
            if (ignore != null)
            {
                foreach (PlayerUpgrade i in ignore)
                {
                    listCopy.Remove(i);
                }
            }
            
            int totalWeight = 0;
            foreach (PlayerUpgrade upgrade in listCopy)
            {
                totalWeight += upgrade.weight*upgrade.rarity;
            } 

            int random = UnityEngine.Random.Range(0, totalWeight);
            int count = 0;

            foreach (PlayerUpgrade upgrade in listCopy)
            {
                count += upgrade.weight*upgrade.rarity;

                if (random <= count) return upgrade;
            }

            return list[0];
        }
    }

    private void InitializeUpgrades()
    {
        for (int i = 0; i < upgradeList.Length; i++)
        {
            upgradeList[i].weight = initialUpgradeWeight;
        }
    }

    private PlayerUpgrade[] ChooseUpgrades(int numOfPossibleUpgrades)
    {
        PlayerUpgrade[] possibleUpgrades = new PlayerUpgrade[numOfPossibleUpgrades];

        for (int i = 0; i < numOfPossibleUpgrades; i++)
        {
            possibleUpgrades[i] = PlayerUpgrade.GetRandom(upgradeList, possibleUpgrades);
            possibleUpgrades[i].weight = upgradeWeightAfterShowed;
        }

        for (int i = 0; i < upgradeList.Length; i++)
        {
            bool shown = false;
            for (int j = 0; j < numOfPossibleUpgrades; j++)
            {
                if (upgradeList[i].status == possibleUpgrades[j].status) 
                {
                    shown = true; break;
                }
            }
            if (shown) continue;

            upgradeList[i].weight += upgradeWeightIncrease;
        }
        
        return possibleUpgrades;
    }

    //!!!Change later
    public void SelectUpgrade(PlayerStatus status)
    {
        selectedUpgrade = status;
    }

    #region //Operational methods

    public UpgradeEventCurrentInfo GetCurrentInfo()
    {
        return new UpgradeEventCurrentInfo(numOfSignOptions, startAnswerX, spaceBetweenAnswers);
    }

    public void Initialize(LevelManager levelManager, SignSelector selector, PlayerController playerController, float speed, Vector3 spawn_position, float z_limit, float floor_width)
    {
        this.levelManager = levelManager;
        this.signSelector = selector;
        this.playerController = playerController;
        this.speed = speed;
        this.spawnPosition = spawn_position;
        this.zLimit = z_limit;
        this.floorWidth = floor_width;

        startAnswerX = spawnPosition.x - floorWidth/2 + upgradeAnswerSpawnBorder;
        spaceBetweenAnswers = (floorWidth-(upgradeAnswerSpawnBorder*2f)) / (numOfSignOptions-1);

        InstantiateUpgradePlayerTargets();
        InitializeUpgrades();
    }

    public void StartUpgradeEvent(SignCode[] currentSigns, Texture question_texture, Texture answer_texture, int correctAnswerIndex, float speed, bool rewardUpgrade)
    {
        this.speed = speed;
        this.correctAnswerIndex = correctAnswerIndex;
        this.currentSigns = currentSigns;
        this.rewardUpgrade = rewardUpgrade;

        questionController.SetTextures(question_texture, answer_texture, questionColor, answerColors[correctAnswerIndex]);
        questionController.SetAnimation(UpgradeQuestionSignController.Animation.Entering);

        if (debug) Debug.Log(debugTag + "Question Stage");
        stage = Stage.Question;
    }

    #endregion

    #region //Player movement target methods

    private void InstantiateUpgradePlayerTargets()
    {
        if (playerTargets != null) 
        {
            //Destroy all
            foreach (GameObject target in playerTargets) Destroy(target);
        }

        //Instantiate new targets
        playerTargets = new GameObject[numOfSignOptions];

        float xx = startAnswerX;
        for (int i = 0; i < numOfSignOptions; i++)
        {
            GameObject target = Instantiate(playerTargetPrefab, new Vector3(xx, playerTargetHideY, 0), Quaternion.identity);
            
            playerTargets[i] = target;

            xx += spaceBetweenAnswers;
        }
    }

    public void SetHighlitedPlayerTarget(int index)
    {
        for (int i = 0; i < playerTargets.Length; i++)
        {
            Material mat = (i == index) ? playerTargetHighlitedMaterial : playerTargetMaterial;
            playerTargets[i].GetComponent<MeshRenderer>().material = mat;
        }
    }

    #endregion

    private System.Collections.IEnumerator destroyAnswerObjects(float time, int num, GameObject[] objs)
    {
        for (int i = 0; i < num; i++)
        {
            objs[i].GetComponent<SignObjectController>().destroy = true;
        }

        yield return new WaitForSeconds(time);

        audioManager.PlaySound(destroyAnswersSound);
        for (int i = 0; i < num; i++)
        {
            if (objs[i] != null)
            {
                if (debug) Debug.Log(debugTag + "Object [" + answerObjects[i].name + "] destroyed");

                objs[i].GetComponent<SignObjectController>().DestroyMe();
            }
        }
    }
    void Start()
    {
        if (debug)  { Debug.Log(debugTag + "Start"); }
        
        audioManager = Injector.GetAudioManager(gameObject);
    }

    void Update()
    {
        if (paused) return;

        switch (stage)
        {
            case Stage.Question:
            {
                //When there are no enemies left alive, spawn the answer objects
                if (levelManager.GetAliveEnemies().Count <= 0)
                {
                    if (debug) Debug.Log(debugTag + "Answer Stage");
                    
                    //Makes player stop shooting
                    playerController.SetState(playerController.upgradeState);

                    #region //Spawn the answer objects

                    float xx = startAnswerX;

                    //Instantiate the objects
                    answerObjects = new GameObject[numOfSignOptions];

                    for (int i = 0; i < numOfSignOptions; i++)
                    {
                        GameObject obj = Instantiate(signObjectPrefab, new Vector3(xx, spawnPosition.y, spawnPosition.z), Quaternion.identity);
                        
                        SignObjectController controller = obj.GetComponent<SignObjectController>();
                        controller.speed = speed;
                        controller.zLimit = zLimit;
                        controller.particleManager = particleManager;

                        controller.SetTextures(SignSetManager.GetSoureSign(currentSigns[i]).signTexture, SignSetManager.GetTargetSign(currentSigns[i]).signTexture, answerColors[i]);

                        answerObjects[i] = obj;

                        xx += spaceBetweenAnswers;
                    }

                    #endregion

                    stage = Stage.Answer;
                }
            }
            break;
            case Stage.Answer:
            {
                bool next_stage = false;
                int selected_index = (int) SignCode.AnswerTookToLong;
                float time_to_destroy = 0;

                //If object z is too close to the players
                if (Mathf.Abs(answerObjects[0].transform.position.z - playerController.transform.position.z) < distanceFromPlayerToDestroyAnswerObjects)
                {
                    if (debug) Debug.Log(debugTag + "Answer objects too close to the player, destroying them");
                    next_stage = true;
                }

                for (int i = 0; i < numOfSignOptions; i++)
                {
                    if (next_stage) break; //Jumps the loop if the next stage is already set because of proximity

                    if (answerObjects[i].GetComponent<SignObjectController>().chosen)
                    {
                        selected_index = i;
                        
                        if (debug) { Debug.Log(debugTag + "Answer selected - index [" + selected_index + "]"); Debug.Log(debugTag + "Object [" + answerObjects[selectedAnswerIndex].name + "] destroyed"); }

                        next_stage = true;
                        time_to_destroy = timeToDestroyAnswerObjects;
                        selected_index = i;

                        break;
                    }
                }

                if (next_stage)
                {
                    StartCoroutine(destroyAnswerObjects(time_to_destroy,numOfSignOptions,answerObjects)); //Starts the coroutine to destroy the answer objects

                    playerController.SetState(playerController.shootingState); //Allows player to shoot again
                    questionController.SetAnimation(UpgradeQuestionSignController.Animation.ShowAnswer); //Hides the question object

                    selectedAnswerIndex = selected_index;

                    if (debug) Debug.Log(debugTag + "Feedback Stage");
                    stage = Stage.Feedback;
                }
            }
            break;
            case Stage.Feedback:
            {
                //Resolve thisevent at the selector
                if (selectedAnswerIndex != (int) SignCode.AnswerTookToLong) signSelector.ResolveEvent(selectedAnswerIndex);

                if (debug) Debug.Log(debugTag + "Allows player to shoot again");

                //Allows player to shoot again
                playerController.SetShooting(true);

                if (selectedAnswerIndex == correctAnswerIndex)
                {
                    //positive feedback
                    confettiLauncher.LaunchConfetti();
                    audioManager.PlaySound(correctAnswerSound);
                    //playerController.Upgrade(); //!Change later
                    
                    if (rewardUpgrade)
                    {
                        //Rewards player with a upgrade
                        currentUpgradeSelection = ChooseUpgrades(numOfUpgradeOptions);
                        selectedUpgrade = PlayerStatus.Count;

                        upgradeSelectionPanel.SetActive(true);
                        upgradeSelection.SetButtons(currentUpgradeSelection);

                        if (debug) Debug.Log(debugTag + "Choosing upgrade stage");
                        stage = Stage.ChoosingUpgrade;
                    }  
                    else
                    {
                        //Rewards player with points
                        levelManager.currentScore += pointsRewardWhenNoUpgrade;
                        
                        if (debug) Debug.Log(debugTag + "Waiting stage");
                        stage = Stage.Waiting;
                    }
                }
                else 
                {
                    //negative feedback
                    audioManager.PlaySound(wrongAnswerSound);

                    if (debug) Debug.Log(debugTag + "Waiting stage");
                    stage = Stage.Waiting;
                }
            }
            break;
            case Stage.ChoosingUpgrade:
            {
                if (selectedUpgrade != PlayerStatus.Count)
                {
                    playerController.Upgrade(selectedUpgrade);
                    upgradeSelectionPanel.SetActive(false);

                    for (int i = 0; i < currentUpgradeSelection.Length; i++)
                    {
                        if (currentUpgradeSelection[i].status == selectedUpgrade) currentUpgradeSelection[i].weight = upgradeWeightAfterSelected;
                    }

                    if (debug) Debug.Log(debugTag + "Waiting stage");
                    stage = Stage.Waiting;
                }
            }
            break;
        }

        //Handles player targets
        float current_y = playerTargets[0].transform.position.y;
        float target_y = (playerController.GetState() is UpgradeState) ? playerTargetShowY : playerTargetHideY;

        current_y += (target_y - current_y) / (playerTargetSmoothMoveRatio / Time.deltaTime);

        foreach (GameObject target in playerTargets)
        {
            Vector3 target_pos = target.transform.position;
            target_pos.y = current_y;
            target.transform.position = target_pos;
        }
    }

    public bool Finished()
    {
        return stage == Stage.Waiting;
    }
}
