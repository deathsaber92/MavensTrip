using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinsStrings : MonoBehaviour
{
    public string[] skinsStringsAssignments;
    public string[] defaultLockValuesByIdexOrderAssignments; 
    public static string[] skinsStrings;
    public static string[] defaultLockValuesByIdexOrder;

    public void Awake()
    {
        CreateInstanceOfSkinStrings();
    }

    /// <summary>
    /// Instantiates the skinStringsAssignments dictionary to be used everywhere
    /// </summary>
    public void CreateInstanceOfSkinStrings()
    {
        skinsStrings = skinsStringsAssignments;
        defaultLockValuesByIdexOrder = defaultLockValuesByIdexOrderAssignments;
    }
} 
