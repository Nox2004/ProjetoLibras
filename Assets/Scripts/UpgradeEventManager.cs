using System.Collections;
using System.Collections.Generic;
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

    private Stage stage = Stage.Question;
    public float speed;

    public GameObject questionObject;
    private Image questionImage;
    public Sprite questionSprite;

    private GameObject[] answerObject;
    public GameObject[] answerPrefabs;
    public int correctAnswerIndex;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool Finished()
    {
        return stage == Stage.Feedback;
    }
}
