using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NETWORK_ENGINE;

public class GameManagingScript : NetworkComponent
{
    public bool GameStarted = false;
    public bool GameOver = false;
    public int LevelSelected = 0;
    public bool AllClientsSwitched = false;
    public string Winner = "<Default>";

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "GameStart")
        {
            GameStarted = bool.Parse("value");
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(1);

        if (IsServer)
        {
            NetworkPlayerOptionManager[] temp = GameObject.FindObjectsOfType<NetworkPlayerOptionManager>();
            // To wait for all players to be ready
            while (!GameStarted || MyCore.Connections.Count == 0)
            {
                // Look at all the player managers
                // If any Ready are false - then GameStarted = false
                temp = GameObject.FindObjectsOfType<NetworkPlayerOptionManager>();
                if (temp.Length != 0)
                {
                    GameStarted = true;
                }
                
                for (int i = 0; i < temp.Length; i++)
                {
                    if (!temp[i].Ready)
                    {
                        GameStarted = false;
                    }
                }

                yield return new WaitForSeconds(MyCore.MasterTimer);
            }
            // Game is now ready to start...
            Debug.Log("All players are ready");

            // Switch scenes
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i].SendUpdate("LEVEL", LevelSelected.ToString());
            }

            // Wait until all have switched
            while (!AllClientsSwitched)
            {
                AllClientsSwitched = true;
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i].Switched == false)
                    {
                        AllClientsSwitched = false;
                        break;
                    }
                    Debug.Log("Player " + i + " has switched");
                }

                yield return new WaitForSeconds(.1f);
            }
            Debug.Log("All players have switched");

            // Then I want to switch the scene
            SceneManager.LoadScene(LevelSelected + 1);

            // Notify all players that game is about to start
            SendUpdate("GameStart", "True");
            Debug.Log("The Game has started");

            // Spawn all player characters
            temp = GameObject.FindObjectsOfType<NetworkPlayerOptionManager>();
            for (int i = 0; i < temp.Length; i++)
            {
                GameObject tempGO = new GameObject();
                if (i == 0)
                {
                    tempGO = MyCore.NetCreateObject(temp[i].Difficulty, temp[i].Owner, new Vector3(8.5f, 8.5f, 0f));
                }
                if (i == 1)
                {
                    tempGO = MyCore.NetCreateObject(temp[i].Difficulty, temp[i].Owner, new Vector3(-8.5f, 8.5f, 0f));
                }
                if (i == 2)
                {
                    tempGO = MyCore.NetCreateObject(temp[i].Difficulty, temp[i].Owner, new Vector3(8.5f, -8.5f, 0f));
                }
                if (i >= 3)
                {
                    tempGO = MyCore.NetCreateObject(temp[i].Difficulty, temp[i].Owner, new Vector3(-8.5f, -8.5f, 0f));
                }
                tempGO.GetComponent<PlayerMovement>().StartingParameters(temp[i].Pname);
            }
            Debug.Log("Objects have been created");

            // Spawning Pickups
            yield return new WaitForSeconds(MyCore.MasterTimer);
            if (LevelSelected == 0 || LevelSelected == 2)
            {
                MyCore.NetCreateObject(3, Owner, Vector3.zero);
            }
            else
            {
                MyCore.NetCreateObject(3, Owner, new Vector3(0, -8, 0));
            }

            // Wait for the end game condition
            while (!GameOver)
            {
                PlayerMovement[] tempPM = GameObject.FindObjectsOfType<PlayerMovement>();
                if (tempPM.Length <= 1)
                {
                    GameOver = true;
                    Winner = tempPM[0].Pname;
                    MyCore.NetCreateObject(2, tempPM[0].Owner, Vector3.zero);
                    MyCore.NetDestroyObject(tempPM[0].NetId);
                }
                yield return new WaitForSeconds(MyCore.MasterTimer);
            }

            // Present the results
            GameOverScript[] tempGG = GameObject.FindObjectsOfType<GameOverScript>();
            for (int i = 0; i < tempGG.Length; i++)
            {
                tempGG[i].WinnerName(Winner);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
