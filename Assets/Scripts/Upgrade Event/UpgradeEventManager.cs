//using System.Collections;
//using System.Collections.Generic;
//using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;

public struct UpgradeEventCurrentInfo
{
    public int numOfOptions;
    public float startAnswerX;
    public float spaceBetweenAnswers;

    public UpgradeEventCurrentInfo(int numOfOptions, float startAnswerX, float spaceBetweenAnswers)
    {
        this.numOfOptions = numOfOptions;
        this.startAnswerX = startAnswerX;
        this.spaceBetweenAnswers = spaceBetweenAnswers;
    }
}

public struct UpgradeEventInstanceData
{ 
    public SignCode selectedSign;
    public SignCode correctSign;
    public SignCode[] options;

    public UpgradeEventInstanceData(SignCode[] options, SignCode selectedSign, SignCode correctSign)
    {
        this.selectedSign = selectedSign;
        this.correctSign = correctSign;
        this.options = options;
    }

    public bool isCorrect() => selectedSign == correctSign;
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
        Feedback // Shows the correct answer and upgrades the player
    }

    //Debugging
    [SerializeField] private bool debug;
    private string debugTag = "UpgradeEventManager: ";

    //Stage
    private Stage stage = Stage.Waiting;
    //History
    private List<UpgradeEventInstanceData> upgradeEventHistory;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private UpgradeQuestionSignController questionController;
    [SerializeField] private GameObject signObjectPrefab;
    [SerializeField] private ParticleManager particleManager;
    [SerializeField] private AudioManager audioManager;

    #region //Info used to instantiate stuff

    [Header("Instantiate parameters")]
    [SerializeField] private int numOfOptions;
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
    [SerializeField] private float timeToDestroyAnswerObjects;
    [SerializeField] private float distanceFromPlayerToDestroyAnswerObjects;
    [SerializeField] private AudioClip destroyAnswersSound;

    //Player targets
    [Header("Player Targets")]
    [SerializeField] private GameObject playerTargetPrefab;
    private GameObject[] playerTargets;
    [SerializeField] private float playerTargetHideY, playerTargetShowY, playerTargetSmoothMoveRatio;
    [SerializeField] private Material playerTargetMaterial, playerTargetHighlitedMaterial;

    #region //Operational methods

    public UpgradeEventCurrentInfo GetCurrentInfo()
    {
        return new UpgradeEventCurrentInfo(numOfOptions, startAnswerX, spaceBetweenAnswers);
    }

    public void Initialize(float speed, Vector3 spawn_position, float z_limit, float floor_width)
    {
        this.speed = speed;
        this.spawnPosition = spawn_position;
        this.zLimit = z_limit;
        this.floorWidth = floor_width;

        startAnswerX = spawnPosition.x - floorWidth/2 + upgradeAnswerSpawnBorder;
        spaceBetweenAnswers = (floorWidth-(upgradeAnswerSpawnBorder*2f)) / (numOfOptions-1);

        InstantiateUpgradePlayerTargets();
    }

    public void StartUpgradeEvent(SignCode[] currentSigns, Texture question_texture, Texture answer_texture, GameObject[] answer_prefabs, int correct_answer_index, float speed)
    {
        this.speed = speed;
        this.correctAnswerIndex = correct_answer_index;
        this.currentSigns = currentSigns;

        questionController.SetTextures(question_texture, answer_texture);
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
        playerTargets = new GameObject[numOfOptions];

        float xx = startAnswerX;
        for (int i = 0; i < numOfOptions; i++)
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
        
        upgradeEventHistory = new List<UpgradeEventInstanceData>();
        audioManager = Injector.GetAudioManager(gameObject);
    }

    void Update()
    {
        if (paused) return;

        switch (stage)
        {
            case Stage.Question:
            {
                //Count enemies in scene
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

                int enemy_count = 0;
                foreach (GameObject enemy in enemies)
                {
                    if (enemy.GetComponent<ITakesDamage>().alive)
                    {
                        enemy_count++;
                    }
                }

                //When there are no enemies left alive, spawn the answer objects
                if (enemy_count <= 0)
                {
                    if (debug) Debug.Log(debugTag + "Answer Stage");
                    
                    //Makes player stop shooting
                    playerController.SetState(playerController.upgradeState);

                    #region //Spawn the answer objects

                    float xx = startAnswerX;

                    //Instantiate the objects
                    answerObjects = new GameObject[numOfOptions];

                    for (int i = 0; i < numOfOptions; i++)
                    {
                        GameObject obj = Instantiate(signObjectPrefab, new Vector3(xx, spawnPosition.y, spawnPosition.z), Quaternion.identity);
                        
                        SignObjectController controller = obj.GetComponent<SignObjectController>();
                        controller.speed = speed;
                        controller.zLimit = zLimit;
                        controller.particleManager = particleManager;

                        controller.SetTextures(SignSetManager.GetSoureSign(currentSigns[i]).signTexture, SignSetManager.GetTargetSign(currentSigns[i]).signTexture);

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

                for (int i = 0; i < numOfOptions; i++)
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
                    StartCoroutine(destroyAnswerObjects(time_to_destroy,numOfOptions,answerObjects)); //Starts the coroutine to destroy the answer objects

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
                //Adds this event instance to the history
                upgradeEventHistory.Add(new UpgradeEventInstanceData(   currentSigns, 
                                                                        (selectedAnswerIndex == (int) SignCode.AnswerTookToLong) ? SignCode.AnswerTookToLong : currentSigns[selectedAnswerIndex],
                                                                        currentSigns[correctAnswerIndex]));

                if (selectedAnswerIndex == correctAnswerIndex)
                {
                    //positive feedback
                    playerController.Upgrade(); //!Change later
                }
                else 
                {
                    //negative feedback
                }

                if (debug) Debug.Log(debugTag + "Allows player to shoot again");

                //Allows player to shoot again
                playerController.SetShooting(true);

                if (debug) Debug.Log(debugTag + "Waiting stage");

                stage = Stage.Waiting;
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
