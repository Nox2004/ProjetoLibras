using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Transition : MonoBehaviour
{
    protected enum Stage
    {
        StartScene,
        TargetScene
    }
    protected Stage stage;

    public string targetSceneName;
    protected ChangeSceneManager changeSceneManager;

    protected float animationCompletion = 0;	

    [SerializeField] protected CurveValueInterpolator transitionAnimationEnter;
    [SerializeField] protected CurveValueInterpolator transitionAnimationExit;
    private bool startedEnter = false, startedExit = false;

    virtual protected void Start()
    {
        DontDestroyOnLoad(gameObject);

        changeSceneManager = Injector.GetSceneManager();

        transitionAnimationEnter.startVal = 0;
        transitionAnimationEnter.endVal = 1;
        transitionAnimationExit.startVal = 1;
        transitionAnimationExit.endVal = 0;
    }

    virtual protected void Update()
    {
        switch (stage)
        {
            case Stage.StartScene:
            {
                if (!startedEnter)
                {
                    AtEnter();
                    startedEnter = true;
                }

                animationCompletion = transitionAnimationEnter.Update();

                if (transitionAnimationEnter.Finished())
                {
                    stage = Stage.TargetScene;
                    changeSceneManager.LoadScene(targetSceneName);
                }
            }
            break;
            case Stage.TargetScene:
            {
                if (!startedExit)
                {
                    AtExit();
                    startedExit = true;
                }

                animationCompletion = transitionAnimationExit.Update();

                if (transitionAnimationExit.Finished())
                {
                    Destroy(gameObject);
                }
            }
            break;
        }
    }

    virtual protected void AtEnter() {}

    virtual protected void AtExit() {}
}
