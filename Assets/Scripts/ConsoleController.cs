using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleController : MonoBehaviour
{
    public GameObject debugConsole;
    public static bool isOpen = false;

    /// <summary>
    /// Opens the console
    /// </summary>
    public void OpenCloseConsoleButton()  
    {
        if (isOpen)
        {
            debugConsole.SetActive(false);
            isOpen = false;
        }
        else
        {
            debugConsole.SetActive(true);
            isOpen = true;
        }
    }

    /// <summary>
    /// Resets all the prefabs to test new game entry
    /// </summary>
    public void ResetPrefabsConsoleButton()
    {
        PlayerPrefs.DeleteAll();
    }
}
