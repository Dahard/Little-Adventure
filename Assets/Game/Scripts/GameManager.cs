using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private Character playerCharacter;
    private bool gameIsOVer;

    private void Awake()
    {
        playerCharacter = GameObject.FindWithTag("Player").GetComponent<Character>();
    }

    public void GameOver()
    {
        Debug.Log("GAME OVER");
    }

    public void GameIsFinished()
    {
        Debug.Log("GAME IS FINISHED");
    }

    void Update()
    {
        if (gameIsOVer)
        {
            return;
        }

        if (playerCharacter.CurrentState == Character.CharacterState.Dead)
        {
            gameIsOVer = true;
            GameOver();
        }
    }
}
