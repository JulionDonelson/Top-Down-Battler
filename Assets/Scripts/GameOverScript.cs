using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NETWORK_ENGINE;

public class GameOverScript : NetworkComponent
{
    public Text Results;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "TEXT")
        {
            Results.text = "The winner is " + value;
        }
    }

    public override IEnumerator SlowUpdate()
    {
        if (!IsLocalPlayer)
        {
            GetComponentInChildren<Canvas>().gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(1);
    }

    public void WinnerName(string pn)
    {
        SendUpdate("TEXT", pn);
    }

    public void BackToMainMenu()
    {
        if (IsLocalPlayer)
        {
            SceneManager.LoadScene(0);
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
