using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialManager : LevelManager
{ 
    enum TutorialState
    {
        TeachingToMove,
        TeachingToShoot,
        TeachingToQuiz,
        TeachingToUpgrade,
        End
    }

    private TutorialState tutorialState;

    public override void GameOver()
    {
        //just revives player (player should not die in tutorial)
        if (tutorialState == TutorialState.TeachingToShoot) currentStarScore = 0;

        playerController.Revive();
    }

    [SerializeField] GameObject transitionPrefab;

    public void GoToNormalGame()
    {
        GameManager.UnlockGameMode(GameModeID.Normal);
        GameManager.SetHighScore(GameModeID.Tutorial, 1);
        GameObject trans_obj = Instantiate(transitionPrefab);
        Transition transition = trans_obj.GetComponent<Transition>();
        transition.targetSceneName = "Game";
    }
    
    private float playerLastPosition = 0f, playerMovementCount = 0f;
    [SerializeField] private float playerMoveTutorial = 0f;

    private float enemySpawnTimer;
    [SerializeField] private float enemySpawnCooldown;

    private EnemyPool currentEnemyPool;

    [SerializeField] private SignSet tutorialSignSet;

    private int quizzCorrectAnswersInARow = 0;
    [SerializeField] private int quizzCorrectAnswersToFinish = 3;

    [SerializeField] private float tutorialPanelTimeToReappear = 5f;
    [SerializeField] private TutorialPanel howToMove; //turns off after player moves a little
    [SerializeField] private TutorialPanel howToShoot; //turns off after player kills a enemy

    [SerializeField] private float howToShootAppearDelay = 5f;
    private bool howToShootAppeared = false;
    [SerializeField] private float howToQuizAppearDelay = 6f;
    private float howToQuizAppearDelayTimer = 0f;
    private bool howToQuizAppeared = false;
    [SerializeField] private TutorialPanel howToQuizz; //turns off after player answers a quizz right

    [SerializeField] private float speedDiminishMultiplier = 0.3f;
    private float initialObjectsSpeed;

    [SerializeField] private TutorialPanel finalPanel; //turnsoff when player touches anywhere
    [SerializeField] private float finishTutorialDelay;

    [SerializeField] private RectTransform starProgressionUITransform;
    [SerializeField] private float starProgressionUIOffet;
    [SerializeField] private float starProgressionUIOffetDiminishMultiplier;
    private float starProgressionUIInitialBottom, starProgressionUIInitialTop;

    [SerializeField] private RectTransform signQuizEventsCompletedTransform;
    [SerializeField] private float initialsignQuizEventsCompletedOffet;
    private float initialsignQuizEventsCompletedBottom, initialsignQuizEventsCompletedTop;

    [SerializeField] private TMPro.TextMeshProUGUI signQuizEventsCountText;

    new void Start()
    {
        base.Start();

        tutorialState = TutorialState.TeachingToMove;

        currentEnemyPool = currentSubstar.enemyPool;

        playerLastPosition = playerController.transform.position.x;

        playerController.SetShooting(false);

        howToMove.SetActive(tutorialPanelTimeToReappear);

        initialObjectsSpeed = objectsSpeed;

        //Sets up star progression UI offset
        starProgressionUIInitialBottom = starProgressionUITransform.offsetMin.y;
        starProgressionUIInitialTop = starProgressionUITransform.offsetMax.y;

        starProgressionUITransform.offsetMin = new Vector2(starProgressionUITransform.offsetMin.x, starProgressionUIInitialBottom + starProgressionUIOffet);
        starProgressionUITransform.offsetMax = new Vector2(starProgressionUITransform.offsetMax.x, starProgressionUIInitialTop + starProgressionUIOffet);

        //Sets up sign quiz events completed UI offset
        initialsignQuizEventsCompletedBottom = signQuizEventsCompletedTransform.offsetMin.y;
        initialsignQuizEventsCompletedTop = signQuizEventsCompletedTransform.offsetMax.y;

        signQuizEventsCompletedTransform.offsetMin = new Vector2(signQuizEventsCompletedTransform.offsetMin.x, initialsignQuizEventsCompletedBottom + initialsignQuizEventsCompletedOffet);
        signQuizEventsCompletedTransform.offsetMax = new Vector2(signQuizEventsCompletedTransform.offsetMax.x, initialsignQuizEventsCompletedTop + initialsignQuizEventsCompletedOffet);
    }

    public override void AddStarScore(int score)
    {
        if (tutorialState == TutorialState.TeachingToShoot)
        {
            howToShoot.SetUnactive();
        }

        base.AddStarScore(score);
    }

    protected override void Update()
    {
        if (paused) return;

        switch (tutorialState)
        {
            case TutorialState.TeachingToMove:
            {
                //UI moving tutorial shows up

                //if player moves more than X units, change state to TeachingToShoot and makes player start shooting
                if (playerController.transform.position.x != playerLastPosition)
                {
                    playerMovementCount += Mathf.Abs(playerController.transform.position.x - playerLastPosition);
                    playerLastPosition = playerController.transform.position.x;
                }

                if (playerMovementCount >= playerMoveTutorial)
                {
                    howToMove.SetUnactive();

                    tutorialState = TutorialState.TeachingToShoot;

                    playerController.SetShooting(true);
                }
            }
            break;
            case TutorialState.TeachingToShoot:
            {
                //Star progression UI offset diminishes
                starProgressionUIOffet *= Mathf.Pow(starProgressionUIOffetDiminishMultiplier, Time.deltaTime);

                //UI shooting tutorial shows up
                howToShootAppearDelay-= Time.deltaTime;
                if (howToShootAppearDelay <= 0 && howToShootAppeared == false)
                {
                    howToShootAppeared = true;
                    howToShoot.SetActive(tutorialPanelTimeToReappear);
                }
                
                //spawn enemies and count enemies slain by player (count should reset if player dies)
                enemySpawnTimer -= Time.deltaTime;
                if (enemySpawnTimer <= 0)
                {
                    enemySpawnTimer = enemySpawnCooldown;
                    SpawnEnemy(currentEnemyPool.GetRandomEnemyPrefab(), floorWidth);
                }

                //When player reaches a substar
                if (StarReadyForReward())
                {
                    //if (debug) Debug.Log(debugTag + "Player reached a substar");

                    if (debug) Debug.Log(debugTag + "Starting upgrade event");

                    int num = signQuizEventManager.GetCurrentInfo().numOfSignOptions;
                    SignSelection signSelection = signSelector.SelectSigns(num);

                    signQuizEventManager.StartSignQuizEvent(signSelection.signs,
                                            SignSetManager.GetTargetSign(signSelection.signs[signSelection.correctSignIndex]).signTexture,
                                            SignSetManager.GetTargetSign(signSelection.signs[signSelection.correctSignIndex]).signTexture,
                                            signSelection.correctSignIndex);

                    if (debug) Debug.Log(debugTag + "Creating upgrade event manager");

                    tutorialState = TutorialState.TeachingToQuiz;

                    howToQuizAppearDelayTimer = howToQuizAppearDelay;
                }
            }
            break;
            case TutorialState.TeachingToQuiz:
            {
                //hides star progression UI
                starProgressionUIOffet -= 20f * Time.deltaTime;

                //shows sign quiz events completed UI
                initialsignQuizEventsCompletedOffet *= Mathf.Pow(starProgressionUIOffetDiminishMultiplier, Time.deltaTime);

                if (!howToQuizAppeared)
                {
                    if (signQuizEventManager.stage == SignQuizEventManager.Stage.Answer)
                    {
                        howToQuizAppearDelayTimer -= Time.deltaTime;
                        if (howToQuizAppearDelayTimer <= 0)
                        {
                            howToQuizAppeared = true;
                            howToQuizz.SetActive(tutorialPanelTimeToReappear);
                        }
                    }
                }

                if (howToQuizz.on)
                {
                    objectsSpeed *= Mathf.Pow(speedDiminishMultiplier, Time.deltaTime);
                }
                else
                {
                    objectsSpeed = initialObjectsSpeed;
                }
                
                if (signQuizEventManager.Finished())
                {
                    howToQuizz.SetUnactive();
                    if (signQuizEventManager.AnsweredRight())
                    {
                        howToQuizAppeared = true;
                        quizzCorrectAnswersInARow++;
                    }
                    else
                    {
                        howToQuizAppeared = false;
                        howToQuizAppearDelayTimer = howToQuizAppearDelay;
                        quizzCorrectAnswersInARow = 0;
                    }

                    if (quizzCorrectAnswersInARow >= quizzCorrectAnswersToFinish)
                    {
                        StartUpgradeSelection(currentReward as UpgradeReward);
                        
                        //goes to upgrade selection state
                        tutorialState = TutorialState.TeachingToUpgrade;
                    }
                    else
                    {
                        int num = signQuizEventManager.GetCurrentInfo().numOfSignOptions;
                        SignSelection signSelection = signSelector.SelectSigns(num);

                        signQuizEventManager.StartSignQuizEvent(signSelection.signs,
                                                SignSetManager.GetTargetSign(signSelection.signs[signSelection.correctSignIndex]).signTexture,
                                                SignSetManager.GetTargetSign(signSelection.signs[signSelection.correctSignIndex]).signTexture,
                                                signSelection.correctSignIndex);
                    }
                }
            }
            break;
            case TutorialState.TeachingToUpgrade:
            {
                if (upgraded == true)
                {
                    upgraded = false;

                    //finish tutorial and go to main game
                    tutorialState = TutorialState.End;
                }
            }
            break;
            case TutorialState.End:
            {
                //shows sign quiz events completed UI
                initialsignQuizEventsCompletedOffet -= 20f * Time.deltaTime;

                //shows final panel
                finishTutorialDelay -= Time.deltaTime;
                if (finishTutorialDelay <= 0 && !finalPanel.on)
                {
                    finalPanel.SetActive(0f);
                }
            }
            break;
        }        

        starProgressionUITransform.offsetMin = new Vector2(starProgressionUITransform.offsetMin.x, starProgressionUIInitialBottom + starProgressionUIOffet);
        starProgressionUITransform.offsetMax = new Vector2(starProgressionUITransform.offsetMax.x, starProgressionUIInitialTop + starProgressionUIOffet);
        
        signQuizEventsCompletedTransform.offsetMin = new Vector2(signQuizEventsCompletedTransform.offsetMin.x, initialsignQuizEventsCompletedBottom + initialsignQuizEventsCompletedOffet);
        signQuizEventsCompletedTransform.offsetMax = new Vector2(signQuizEventsCompletedTransform.offsetMax.x, initialsignQuizEventsCompletedTop + initialsignQuizEventsCompletedOffet);
        
        signQuizEventsCountText.text = quizzCorrectAnswersInARow.ToString() + "/" + quizzCorrectAnswersToFinish.ToString();

        base.Update();
    }
}
