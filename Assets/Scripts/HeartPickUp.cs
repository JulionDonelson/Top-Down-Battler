using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class HeartPickUp : NetworkComponent
{
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "HEAL")
        {
            
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                other.GetComponent<PlayerMovement>().HealUp();
                MyCore.NetDestroyObject(NetId);
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
