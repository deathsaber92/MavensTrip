using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelectorController : MonoBehaviour
{
    public TextMeshProUGUI debugReporter;
    public TextMeshProUGUI skinName;

    public RectTransform viewportPanel; // Used to hold the scrollview panel    
    public RectTransform center; // Center to compare the distance for each button
    public float lerpToItemSpeed = 10f;
   
    public GameObject[] skinItems; // The array of skin elements
    public GameObject frameOfHighlight; //This will be the frame of the highlighted skin

    public int startButton = 1;
    public static string currentSkin;
    
    private float[] distance; // All buttons distance to the center
    private float[] distanceReposition; //Used for infinite scrolling
    private bool dragging = false; // Will be true while panel is being dragged 
    private bool currentButtonMessageSend = false; // A message with the current skim item was sent
    public static int closestItemToCenter; // Will hold the number of the button with smallest distance to center 

    private void Start()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsStrings.setStartSkin))
        {
            debugReporter.text = debugReporter.text + "\n" + "SkinSelectorController.Start(): start skin player pref exists. Setting first start button to: " + PlayerPrefs.GetInt(PlayerPrefsStrings.setStartSkin);
            SendMessageFromSkinItem(PlayerPrefs.GetInt(PlayerPrefsStrings.setStartSkin));
            startButton = PlayerPrefs.GetInt(PlayerPrefsStrings.setStartSkin);
        }
        else
        {
            debugReporter.text = debugReporter.text + "\n" + "SkinSelectorController.Start(): start skin player pref does not exist. Setting first start button to default 0";
            SendMessageFromSkinItem(0);
            PlayerPrefs.SetInt(PlayerPrefsStrings.setStartSkin, 0);
            startButton = 0;
        }

        int skinItemsLength = skinItems.Length;
        distance = new float[skinItemsLength];     

        viewportPanel.anchoredPosition = new Vector2((startButton - 1) * -230, 0f);

        foreach (GameObject skin in skinItems)
        {
            skin.SetActive(true);
        }
      //  FindObjectOfType<SkinLockStatusManager>().UpdateSkinsAfterAction();
    }

    private void Update()
    {
        if (MainMenuHandler.menuLoaded.Value == true & Auth.isAuthenticated)
        {
            EnableDisableHighlight();        
            CreateDistanceRepositionFloatArray();
            UpdateDistancesOfSkinElements();
            //ScaleWithMovement();
            AssignClosestItemToCenter();
            PlaceInCenteredPositionWhenNotDragging();
        }
    }

    /// <summary>
    /// Assigns an array for repositioned distances of length equal to the skinItems array length
    /// </summary>
    private void CreateDistanceRepositionFloatArray()
    {
        if (distanceReposition == null)
        {
            distanceReposition = new float[skinItems.Length];
        }
    }   

    /// <summary>
    /// This centers the closest element to the middle when the drag ended
    /// </summary>
    private void PlaceInCenteredPositionWhenNotDragging()
    {
        if (!dragging)
        {
            LerpToItem(-skinItems[closestItemToCenter].GetComponent<RectTransform>().anchoredPosition.x);
        }
    }

    /// <summary>
    /// Gets the lowest distance from distances array (distance from center).
    /// For each element in skinItems if the min distance to center equals the distance of one of the skin items.
    /// Assign the index of the lowest distance button to closestItemToCenter
    /// </summary>
    private void AssignClosestItemToCenter()
    {
        float minDistanceToCenter = Mathf.Min(distance);
        
        for (int i = 0; i < skinItems.Length; i++)
        {
            if (minDistanceToCenter == distance[i])
            {
                closestItemToCenter = i;
                skinItems[i].GetComponent<RectTransform>().localScale = new Vector2(1, 1);
            }
        }
    }

    /// <summary>
    /// Scaling the movement of the skin item with distance so it can get bigger in the moddle
    /// </summary>
    private void ScaleWithMovement()
    {
        for (int i = 0; i < skinItems.Length; i++)
        {
            float scaleFactor = 100 / (distance[i] + 1);         
            skinItems[i].GetComponent<RectTransform>().localScale = new Vector2(scaleFactor, scaleFactor);
        }
    }

    /// <summary>
    /// Calculates all distances realtime for all skin items related to the center of the viewport and index number.
    /// Distances between elements are equal so element index can be used)
    /// </summary>
    private void UpdateDistancesOfSkinElements()
    {
        for (int i = 0; i < skinItems.Length; i++)
        {
            distanceReposition[i] = center.GetComponent<RectTransform>().position.x - skinItems[i].GetComponent<RectTransform>().position.x;
            distance[i] = Mathf.Abs(distanceReposition[i]);
        }
    }

    /// <summary>
    /// Lerps to item when is not dragging
    /// </summary>
    /// <param name="position"></param>
    private void LerpToItem(float position)
    {
        float newX = Mathf.Lerp(viewportPanel.anchoredPosition.x, position, TimeVariables.timeDeltaTime * lerpToItemSpeed);            

        //Send a message when a button is centered
        if (Mathf.Abs(newX) >= Mathf.Abs(position) - 1f && Mathf.Abs(newX) <= Mathf.Abs(position) + 1f && !currentButtonMessageSend)
        {
            currentButtonMessageSend = true;
            SendMessageFromSkinItem(closestItemToCenter);            
        }

        Vector2 newPosition = new Vector2(newX, viewportPanel.anchoredPosition.y);
        viewportPanel.anchoredPosition = newPosition;        
    }   

    public void LerpToNextItem()
    {
        viewportPanel.anchoredPosition = new Vector2((closestItemToCenter + 1) * -230, 0f);
        SendMessageFromSkinItem(closestItemToCenter+1);
    }

    public void LerpToPreviousItem()
    {
        viewportPanel.anchoredPosition = new Vector2((closestItemToCenter - 1) * -230, 0f);
        SendMessageFromSkinItem(closestItemToCenter-1);
    }

    //This is the live skin display controller based on skin index
    //Skinloader will hold the value for skinName and assign the new skins while swapping
    public void SendMessageFromSkinItem(int skinItemIndex)
    {
        if (skinItemIndex == 0)
        {
            currentSkin = "mavenDefault";
            if (PlayerPrefs.GetInt(PlayerPrefsStrings.skinDefaultLock) == 0)
            {
                SkinLockStatusManager.currentSkinOwned = true;
            }
            else
            {
                SkinLockStatusManager.currentSkinOwned = false;
            }

            skinName.text = "MAVEN";
            debugReporter.text = debugReporter.text + "\n" + "SendMessageFromSkinItem(): current skin is: " + currentSkin;
        }
        else if (skinItemIndex == 1)
        {
            currentSkin = "mavenFounders";
            if (PlayerPrefs.GetInt(PlayerPrefsStrings.skinFoundersLock) == 0)
            {
                SkinLockStatusManager.currentSkinOwned = true;
            }
            else
            {
                SkinLockStatusManager.currentSkinOwned = false;
            }

            skinName.text = "FOUNDERS MAVEN";
            debugReporter.text = debugReporter.text + "\n" + "SendMessageFromSkinItem(): current skin is: " + currentSkin;
        }
        else if (skinItemIndex == 2)
        {
            currentSkin = "mavenExplorer";
            if (PlayerPrefs.GetInt(PlayerPrefsStrings.skinExplorerLock) == 0)
            {
                SkinLockStatusManager.currentSkinOwned = true;
            }
            else
            {
                SkinLockStatusManager.currentSkinOwned = false;
            }

            skinName.text = "EXPLORER MAVEN";
            debugReporter.text = debugReporter.text + "\n" + "SendMessageFromSkinItem(): current skin is: " + currentSkin;
        }
        else if (skinItemIndex == 3)
        {
            currentSkin = "mavenRhino";
            if (PlayerPrefs.GetInt(PlayerPrefsStrings.skinRhinoLock) == 0)
            {
                SkinLockStatusManager.currentSkinOwned = true;
            }
            else
            {
                SkinLockStatusManager.currentSkinOwned = false;
            }

            skinName.text = "RHINO MAVEN";
            debugReporter.text = debugReporter.text + "\n" + "SendMessageFromSkinItem(): current skin is: " + currentSkin;
        }
        else if (skinItemIndex == 4)
        {
            currentSkin = "mavenPharaon";
            if (PlayerPrefs.GetInt(PlayerPrefsStrings.skinPharaonLock) == 0)
            {
                SkinLockStatusManager.currentSkinOwned = true;
            }
            else
            {
                SkinLockStatusManager.currentSkinOwned = false;
            }

            skinName.text = "PHARAON MAVEN";
            debugReporter.text = debugReporter.text + "\n" + "SendMessageFromSkinItem(): current skin is: " + currentSkin;
        }
    }

    /// <summary>
    /// Drag started triggered on event from UI
    /// </summary>
    public void StartDrag()
    {
        currentButtonMessageSend = false;
        dragging = true;        
    }

    /// <summary>
    /// Drag ended triggered on event from UI 
    /// </summary>
    public void EndDrag()
    {
        dragging = false;
    }

    /// <summary>
    /// This handles the highlight display of the selected skin
    /// </summary>
    public void EnableDisableHighlight()
    {
        if (dragging)
        {
            if (frameOfHighlight.activeSelf)
                frameOfHighlight.SetActive(false);
        }
        else if (!dragging && currentButtonMessageSend)
        {
            if (!frameOfHighlight.activeSelf)
                frameOfHighlight.SetActive(true);
            FindObjectOfType<SkinLoader>().LoadSkin();
        }
    }
}
