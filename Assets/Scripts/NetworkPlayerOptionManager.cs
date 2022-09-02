using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NETWORK_ENGINE;

public class NetworkPlayerOptionManager : NetworkComponent
{
    public bool Switched = false;
    public bool Ready = false;
    public string Pname = "<Default>";
    public InputField PnameField;
    public int Difficulty = 1;
    public Material[] MyColors = new Material[3];

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "SWITCHED")
        {
            Switched = true;
            if (IsServer)
            {
                SendUpdate("SWITCHED", "1");
            }
        }

        if (flag == "LEVEL")
        {
            if (IsServer)
            {
                FindObjectOfType<GameManagingScript>().LevelSelected = int.Parse(value);
            }
            if (IsLocalPlayer)
            {
                SceneManager.LoadScene(int.Parse(value) + 1);
                this.gameObject.GetComponentInChildren<Canvas>().gameObject.SetActive(false);
                // GameObject.Find("Network Core").transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        if (flag == "DIFFICULTY")
        {
            Difficulty = int.Parse(value);
            if (IsServer)
            {
                SendUpdate("DIFFICULTY", value);
            }
        }

        if (flag == "PNAME")
        {
            Pname = value;
            if (IsServer)
            {
                SendUpdate("PNAME", value);
            }
        }

        if (flag == "READY")
        {
            Ready = bool.Parse(value);
            if (IsServer)
            {
                SendUpdate("READY", value);
            }
        }
    }

    public override IEnumerator SlowUpdate()
    {
        if (!IsLocalPlayer)
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(1);
    }

    public void SetDifficulty(int d)
    {
        if (IsLocalPlayer)
        {
            SendCommand("DIFFICULTY", d.ToString());
        }
    }

    public void SetReady(bool r)
    {
        if (IsLocalPlayer)
        {
            SendCommand("READY", r.ToString());
        }
    }

    public void SetPlayerName(string pn)
    {
        if (IsLocalPlayer)
        {
            SendCommand("PNAME", pn);
        }
    }

    public void SetLevel(int l)
    {
        if (IsLocalPlayer)
        {
            SendCommand("LEVEL", l.ToString());
        }
    }

    public void SceneChanger(Scene s, LoadSceneMode m)
    {
        if (IsLocalPlayer)
        {
            SendCommand("SWITCHED", "1");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += SceneChanger;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
