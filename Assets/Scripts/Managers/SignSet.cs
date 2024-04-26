using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SignCode
{
    AnswerTookToLong = -1,
    A = 0,
    B = 1,
    C = 2,
    D = 3,
    E = 4,
    F = 5,
    G = 6,
    H = 7,
    I = 8,
    J = 9,
    K = 10,
    L = 11,
    M = 12,
    N = 13,
    O = 14,
    P = 15,
    Q = 16,
    R = 17,
    S = 18,
    T = 19,
    U = 20,
    V = 21,
    W = 22,
    X = 23,
    Y = 24,
    Z = 25,
    Count
}

[Serializable]
public class Sign 
{
    public SignCode sign;
    public Texture signTexture;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SignSet", order = 1)]
public class SignSet : ScriptableObject
{
    public Sign[] signs;

    private Dictionary<SignCode, Sign> signDictionary;

    public void Initialize()
    {
        signDictionary = new Dictionary<SignCode, Sign>();
        foreach (Sign sign in signs)
        {
            signDictionary.Add(sign.sign, sign);
        }
    }

    public Sign GetSign(SignCode signCode)
    {
        return signDictionary[signCode];
    }

    public SignSet Copy()
    {
        SignSet newSignSet = CreateInstance<SignSet>();
        newSignSet.signs = new Sign[signs.Length];
        for (int i = 0; i < signs.Length; i++)
        {
            newSignSet.signs[i] = new Sign();
            newSignSet.signs[i].sign = signs[i].sign;
            newSignSet.signs[i].signTexture = signs[i].signTexture;
        }
        return newSignSet;
    }
}