using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement   ;

public class GameManager : MonoBehaviour {

    private void Start()
    {
        
    }
    public static bool isHard;
    public void PlayVsComp()
    {
        GamePlayManager.Mode = false;
        SceneManager.LoadScene("OPTIONS");
    }
    public void Easy()
    {
        AI_Script.Depth = 2;
        isHard = false;
        SceneManager.LoadScene("GAMEPLAY");
    }
    public void Medium()
    {
        AI_Script.Depth = 3;
        isHard = false;
        SceneManager.LoadScene("GAMEPLAY");
    }
    public void Hard()
    {
        AI_Script.Depth = 3;
        isHard = true;
        SceneManager.LoadScene("GAMEPLAY");
    }
    public void PlayVsFriend()
    {
        GamePlayManager.Mode = true;
        SceneManager.LoadScene("GAMEPLAY");
    }
}
