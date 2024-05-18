using System.Collections.Generic;
using UnityEngine;

static class Utilities
{
    public static void GetAllChildren(Transform parent, ref List <Transform> listOfChildren)
    {  
        foreach (Transform t in parent) 
        {
 
            listOfChildren.Add(t);
 
            GetAllChildren(t, ref listOfChildren);
 
        } 
    }

    public static void GetComponentsInAllChildren<T>(Transform parent, ref List<T> listOfComponents)
    {  
        foreach (Transform t in parent) 
        {
            if (t.GetComponent<T>() != null)
            {
                listOfComponents.Add(t.GetComponent<T>());
            }
 
            GetComponentsInAllChildren(t, ref listOfComponents);
 
        } 
    }
}