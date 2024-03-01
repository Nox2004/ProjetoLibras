using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable] public class SignDictionary : SerializableDictionary<char, GameObject> { }

public class SignSetManager : MonoBehaviour
{
    public char[] signSet;
    public SignDictionary alphabetObjects;
    public Dictionary<char, GameObject> SignObjects;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
