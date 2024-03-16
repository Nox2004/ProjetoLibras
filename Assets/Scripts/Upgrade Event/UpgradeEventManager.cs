using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public struct UpgradeEventCurrentInfo
{
    public int numOfUpgrades;
    public float startAnswerX;
    public float spaceBetweenAnswers;
}

public class UpgradeEventManager : MonoBehaviour
{
    private enum Stage 
    {
        Question, //Shows the source sign
        Answer, //presents three target signs (where one is equivalent to the source sign presented earlier)
        Feedback, // Shows the correct answer and upgrades the player
        Destroy
    }

    //Debugging
    public bool debug;
    private string debugTag = "UpgradeEventManager: ";

    //All info that is external to the logic of other classes
    public UpgradeEventCurrentInfo currentInfo;

    //Stage
    private Stage stage = Stage.Question;
    
    //References
    public PlayerController playerController;

    //Speed of the sign objects
    public float speed; 

    //Spawn position and border
    public Vector3 spawnPosition;
    public float zLimit;

    //Question object
    public GameObject questionObject;
    private Image questionImage;
    public Sprite questionSprite;
    private Transform questionTransform;

    private Vector3 questionInitialPosition;
    private Quaternion questionInitialRotation;

    //Question object procedural animation
    public CurveValueInterpolator questionEnterInterpolator;
    private float questionXAngle = 0f;
    public CurveValueInterpolator questionExitInterpolator;
    private float questionYPosition = 0f;
    private WaveValueInterpolator questionYAngleInterpolator, questionXAngleInterpolator, questionZAngleInterpolator;
    
    //Answer objects
    private GameObject[] answerObjects;
    public GameObject[] answerPrefabs;
    
    //Correct answer index
    public int correctAnswerIndex;
    private int selectedAnswerIndex;
    
    void Start()
    {
        if (debug)  { Debug.Log(debugTag + "Start"); Debug.Log(debugTag + "Question Stage"); }

        //Change the question object sprite to the sign matching the correct answer
        questionTransform = questionObject.GetComponent<Transform>();
        questionImage = questionObject.GetComponentInChildren<Image>();
        questionImage.sprite = questionSprite;

        //Set the initial position and rotation of the question object
        questionInitialPosition = questionTransform.localPosition;
        questionInitialRotation = questionTransform.rotation;

        //Set the wavey interpolators
        questionXAngleInterpolator = new WaveValueInterpolator(-2f, 2f, 2f);
        questionYAngleInterpolator = new WaveValueInterpolator(-3f, 3f, 1.7f);
        questionZAngleInterpolator = new WaveValueInterpolator(-4f, 4f, 3f);
    }

    void Update()
    {
        switch (stage)
        {
            case Stage.Question:
            {
                //Rotates the question object to show the sign to the player
                questionEnterInterpolator.Update(Time.deltaTime);

                questionXAngle = questionEnterInterpolator.GetValue();
                
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

                //When the question enter animation finished, spawn the answer objects
                if (questionEnterInterpolator.Finished() && (enemy_count <= 0))
                {
                    if (debug) Debug.Log(debugTag + "Answer Stage");
                    
                    //Makes player stop shooting
                    playerController.SetState(playerController.upgradeState);

                    #region //Spawn the answer objects

                    float xx = currentInfo.startAnswerX;

                    //Instantiate the objects
                    answerObjects = new GameObject[currentInfo.numOfUpgrades];

                    for (int i = 0; i < currentInfo.numOfUpgrades; i++)
                    {
                        GameObject obj = Instantiate(answerPrefabs[i], new Vector3(xx, spawnPosition.y, spawnPosition.z), Quaternion.identity);
                        
                        obj.GetComponent<SignObjectController>().speed = speed;
                        obj.GetComponent<SignObjectController>().zLimit = zLimit;
                        answerObjects[i] = obj;

                        xx += currentInfo.spaceBetweenAnswers;
                    }

                    #endregion

                    stage = Stage.Answer;
                }
            }
            break;
            case Stage.Answer:
            {
                //!!Select condition - change later
                
                float answerZ = answerObjects[0].transform.position.z;
                float playerZ = playerController.transform.position.z;

                if (answerZ <= playerZ)
                {
                    float playerX = playerController.transform.position.x;
                    
                    //Selects the answer closest to the player
                    selectedAnswerIndex = -1;
                    float minDistance = float.MaxValue;

                    for (int i = 0; i < currentInfo.numOfUpgrades; i++)
                    {
                        float distance = Mathf.Abs(answerObjects[i].transform.position.x - playerX);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            selectedAnswerIndex = i;
                        }
                    }

                    if (debug) { Debug.Log(debugTag + "Answer selected - index [" + selectedAnswerIndex + "]"); Debug.Log(debugTag + "Object [" + answerObjects[selectedAnswerIndex].name + "] destroyed"); }
                    answerObjects[selectedAnswerIndex].GetComponent<SignObjectController>().StartDestruction(); //Destroy selected answer object

                    if (debug) Debug.Log(debugTag + "Feedback Stage");
                    stage = Stage.Feedback;

                    playerController.SetState(playerController.shootingState); //Allows player to shoot again
                }

                // if (Input.touches.Length > 0)
                // {
                //     Touch t = Input.touches[Input.touches.Length-1];
                //     if (t.phase == TouchPhase.Began)
                //     {
                //         selectedAnswerIndex = (int) Mathf.Floor((t.position.x / Screen.width) * answerObjects.Length);
                //         int i = selectedAnswerIndex;

                //         if (debug) { Debug.Log(debugTag + "Answer selected - index [" + i + "]"); Debug.Log(debugTag + "Object [" + answerObjects[i].name + "] destroyed"); }
                //         Destroy(answerObjects[i]);

                //         if (debug) Debug.Log(debugTag + "Feedback Stage");
                //         stage = Stage.Feedback;
                //     }
                // }
            }
            break;
            case Stage.Feedback:
            {
                //Question exit animation
                questionExitInterpolator.Update(Time.deltaTime);
                questionYPosition = questionExitInterpolator.GetValue();

                if (questionExitInterpolator.Finished())
                {
                    if (debug) Debug.Log(debugTag + "Allows player to shoot again");

                    //Allows player to shoot again
                    playerController.SetShooting(true);

                    if (debug) Debug.Log(debugTag + "Destroy stage");

                    stage = Stage.Destroy;
                }

                if (selectedAnswerIndex == correctAnswerIndex)
                {
                    //positive feedback
                }
                else 
                {
                    //negative feedback
                }
            }
            break;
        }
        
        //Updates sign rotation
        Vector3 tmp = questionInitialRotation.eulerAngles;
        tmp += new Vector3(questionXAngle + questionXAngleInterpolator.Update(Time.deltaTime),
        questionYAngleInterpolator.Update(Time.deltaTime),
        questionZAngleInterpolator.Update(Time.deltaTime));
        
        questionTransform.rotation = Quaternion.Euler(tmp);
        
        //Updates sign position
        tmp = questionInitialPosition; tmp.y += questionYPosition;
        questionTransform.localPosition = tmp;
    }

    void OnDestroy()
    {
        //May run in editor mode when game is stopped (???)
        if (debug) Debug.Log(debugTag + "OnDestroy - Resetting the question object position and rotation and interpolators");

        //Resets the question object position and rotation
        questionTransform.localPosition = questionInitialPosition;
        questionTransform.rotation = questionInitialRotation;

        //Reset the interpolators so next time the question object is shown, the animation starts from the beginning
        questionEnterInterpolator.Reset();
        questionExitInterpolator.Reset();
    }
    

    public bool Finished()
    {
        return stage == Stage.Destroy;
    }
}
