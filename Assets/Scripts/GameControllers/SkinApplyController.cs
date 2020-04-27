using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinApplyController : MonoBehaviour
{
    public GameObject[] skinIndex;
    public Transform mavenParent;
    private string selectedSkin;
    public static bool protectMultipleSpawns = false;
    
    void Awake()
    {
        selectedSkin = SkinSelectorController.currentSkin;
        protectMultipleSpawns = false;

        int counter = -1;

        //Spawns skin and moves to parent position
        foreach (GameObject skin in skinIndex)
        {
            counter += 1;
            if (selectedSkin == skin.name.ToString())
            {
                Instantiate(skinIndex[counter], mavenParent);
                skinIndex[counter].transform.position = new Vector3(0, 0, 0);
                protectMultipleSpawns = true;
            }
        }

        if (!protectMultipleSpawns)
        {
            Instantiate(skinIndex[0], mavenParent);
            skinIndex[0].transform.position = new Vector3(0, 0, 0);
            protectMultipleSpawns = true;
        }
    }
}
