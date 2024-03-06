using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeEventManager : MonoBehaviour
{
    private enum Stage 
    {
        Question, //Shows the source sign
        Answer, //presents three target signs (where one is equivalent to the source sign presented earlier)
        Feedback // Shows the correct answer and upgrades the player
    }

    public bool debug;
    private string debugTag = "UpgradeEventManager: ";

    private Stage stage = Stage.Question;
    public float speed; 

    public Vector3 spawnPosition;
    public GameObject questionObject;
    private Image questionImage;
    public Sprite questionSprite;

    private GameObject[] answerObjects;
    public GameObject[] answerPrefabs;
    
    public int correctAnswerIndex;
    private int selectedAnswerIndex;
    
    void Start()
    {
        if (debug)  { Debug.Log(debugTag + "Start"); Debug.Log(debugTag + "Question Stage"); }

        //Change the question object sprite to the sign matching the correct answer
        questionImage = questionObject.GetComponent<Image>();
        questionImage.sprite = questionSprite;
    }

    void Update()
    {
        Transform question_transform = questionObject.transform;
        switch (stage)
        {
            case Stage.Question:
            {
                //Move the question object down so the player can see it
                question_transform.position += new Vector3(0, -200 * Time.deltaTime, 0);

                if (question_transform.position.y <= Screen.height - 200)
                {
                    if (debug) Debug.Log(debugTag + "Answer Stage");
                    
                    float size = 4; //!!!Change later
                    float space = size / (answerPrefabs.Length-1);
                    float xx = spawnPosition.x - size/2;

                    answerObjects = new GameObject[answerPrefabs.Length];
                    for (int i = 0; i < answerPrefabs.Length; i++)
                    {
                        GameObject obj = Instantiate(answerPrefabs[i], new Vector3(xx, spawnPosition.y, spawnPosition.z), Quaternion.identity);
                        
                        obj.GetComponent<SignObjectController>().speed = speed;
                        answerObjects[i] = obj;

                        xx += space;
                    }

                    stage = Stage.Answer;
                }
            }
            break;
            case Stage.Answer:
            {
                //Move the question object up so the player cant see it anymore
                if (question_transform.position.y <= Screen.height + 200) question_transform.position += new Vector3(0, 200 * Time.deltaTime, 0);

                //!!Select condition - change later
                if (Input.touches.Length > 0)
                {
                    Touch t = Input.touches[Input.touches.Length-1];
                    if (t.phase == TouchPhase.Began)
                    {
                        selectedAnswerIndex = (int) Mathf.Floor((t.position.x / Screen.width) * 3f);
                        int i = selectedAnswerIndex;

                        if (debug) { Debug.Log(debugTag + "Answer selected - index [" + i + "]"); Debug.Log(debugTag + "Object [" + answerObjects[i].name + "] destroyed"); }
                        Destroy(answerObjects[i]);

                        if (debug) Debug.Log(debugTag + "Feedback Stage");
                        stage = Stage.Feedback;
                    }
                }
            }
            break;
            case Stage.Feedback:
            {
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
    }

    public bool Finished()
    {
        return stage == Stage.Feedback;
    }
}
