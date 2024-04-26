using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

//Data collected from the player's choices
public class SignData
{
    public SignCode sign; //The sign this instance of data is about
    public int showWeight; //The chance for this sign to appear
    public Dictionary<SignCode, int> similarSigns; //List of signs that are similar to this one

    public SignData(SignCode sign, int showWeight)
    {
        //Dicionario
        this.sign = sign;
        this.showWeight = showWeight;
        this.similarSigns = new Dictionary<SignCode, int>();
    }

    public static SignData GetDataByCode(SignCode sign, List<SignData> signsList)
    {
        for (int i = 0; i < signsList.Count; i++)
        {
            if (signsList[i].sign == sign)
            {
                return signsList[i];
            }
        }
        return null;
    }

    public static SignData GetRandom(List<SignData> list, SignCode[] ignore = null)
    {
        //Creates a copy and remove the ignore
        List<SignData> listCopy = new List<SignData>(list);
        if (ignore != null)
        {
            foreach (SignCode i in ignore)
            {
                listCopy.Remove(GetDataByCode(i,list));
            }
        }
        
        int totalWeight = 0;
        foreach (SignData sign in listCopy)
        {
            totalWeight += sign.showWeight;
        } 

        int random = UnityEngine.Random.Range(0, totalWeight);
        int count = 0;

        foreach (SignData sign in listCopy)
        {
            count += sign.showWeight;

            if (random <= count) return sign;
        }

        return list[0];
    }

    public static SignData GetRandomSimilar(SignData similarTo, List<SignData> list, SignCode[] ignore = null)
    {
        //Creates a copy and remove the ignore
        List<SignData> listCopy = new List<SignData>(list);
        if (ignore != null)
        {
            foreach (SignCode i in ignore)
            {
                listCopy.Remove(GetDataByCode(i,list));
            }
        }
        
        int totalWeight = 0;
        foreach (SignData sign in listCopy)
        {
            int mult = 1;
            if (similarTo.similarSigns.ContainsKey(sign.sign))
            {
                mult = similarTo.similarSigns[sign.sign];
            }
            totalWeight += sign.showWeight*mult;
        } 

        int random = UnityEngine.Random.Range(0, totalWeight);
        int count = 0;

        foreach (SignData sign in listCopy)
        {
            int mult = 1;
            if (similarTo.similarSigns.ContainsKey(sign.sign))
            {
                mult = similarTo.similarSigns[sign.sign];
            }
            count += sign.showWeight*mult;

            if (random <= count) return sign;
        }

        return list[0];
    }
}

public struct SignSelection
{
    public SignCode[] signs;
    public int correctSignIndex;

    public SignSelection(int numOfSigns)
    {
        signs = new SignCode[numOfSigns];
        correctSignIndex = 0;
    }

    public void SetCorrectIndex(int index)
    {
        correctSignIndex = index;
    }
}

public struct PlayerSignChoiceData
{
    public enum ResultType
    {
        RightAnswer,
        CloseAnswer, //Selected a similar
        WrongAnswer
    }

    public SignCode selectedSign;
    public SignCode correctSign;
    public SignCode[] options;
    public ResultType resultType;

    public PlayerSignChoiceData(SignCode[] options, SignCode selectedSign, SignCode correctSign)
    {
        this.selectedSign = selectedSign;
        this.correctSign = correctSign;
        this.options = options;
        this.resultType = (selectedSign == correctSign ? ResultType.RightAnswer : ResultType.WrongAnswer); //!!!!ADD Close answer later
    }
}

[Serializable]
public class SignSelector
{
    //List of player choices
    private List<PlayerSignChoiceData> playerChoiceHistory = new List<PlayerSignChoiceData>();

    //List of all posible signals
    private List<SignData> _signsList;
    private List<SignData> signsList 
    {
        get
        {
            if (_signsList == null)
            {
                _signsList = new List<SignData>();
                //Create a list of all sign data
                for (int i = 0; i < SignSetManager.signCodes.Count; i++)
                {
                    signsList.Add(new SignData(SignSetManager.signCodes[i], inicialSignShowWeight));
                }
            }
            return _signsList;
        }
        set
        {
            _signsList = value;
        }
    }

    [HideInInspector] public SignSelection currentSelection;

    [SerializeField] private int inicialSignShowWeight; //initial chance for a sign to appear
    [SerializeField] private int showWeightIncrement; //how much the chance increases when a sign is not chosen
    [SerializeField] private int showWeightAfterSelected; //value to which the chance is set when a sign is chosen

    [SerializeField] private int inicialSimiliarWeight; //initial chance for a similar sign to appear
    [SerializeField] private int weightSimiliarIncrement; //how much the chance increases when a similar sign is chosen

    public SignSelector()
    {
        
    }

    public SignSelection SelectSigns(int numOfOptions)
    {
        SignSelection signSelection = new SignSelection(numOfOptions);
        SignData correctSign;

        //Select the correct sign
        SignData selected = SignData.GetRandom(signsList);
        selected.showWeight = showWeightAfterSelected;

        correctSign = selected;
        int correctSignIndex = UnityEngine.Random.Range(0, numOfOptions);

        signSelection.signs[correctSignIndex] = selected.sign;
        signSelection.correctSignIndex = correctSignIndex;

        for (int i = 0; i < numOfOptions - 1; i++)
        {
            selected = SignData.GetRandomSimilar(correctSign,signsList,signSelection.signs);
            selected.showWeight = showWeightAfterSelected;

            int index = i;

            //jumps correct index
            if (i >= correctSignIndex) index++;

            signSelection.signs[index] = selected.sign;
        }

        currentSelection = signSelection;

        return signSelection;
    }

    public void ResolveEvent(int selectedSignIndex)
    {
        playerChoiceHistory.Add(new PlayerSignChoiceData(currentSelection.signs,
                                                        currentSelection.signs[selectedSignIndex],
                                                        currentSelection.signs[currentSelection.correctSignIndex]));

        PlayerSignChoiceData currentChoice = playerChoiceHistory.Last();

        switch (currentChoice.resultType)
        {
            case PlayerSignChoiceData.ResultType.RightAnswer:
            {
                
            }
            break;
            case PlayerSignChoiceData.ResultType.WrongAnswer:
            {
                SignData signChoosenData = SignData.GetDataByCode(currentChoice.selectedSign,signsList);

                if (signChoosenData.similarSigns.ContainsKey(currentChoice.correctSign))
                {
                    //increase the similarity value
                    signChoosenData.similarSigns[currentChoice.selectedSign] += weightSimiliarIncrement;
                }
                else
                {
                    //Add chosen sign to similar
                    signChoosenData.similarSigns[currentChoice.correctSign] = inicialSimiliarWeight;
                }
            }
            break;
        }
    }
}