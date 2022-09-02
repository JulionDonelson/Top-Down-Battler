using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class NetworkTransform : NetworkComponent
{
    public Vector3 LastPosition = Vector3.zero;
    public Vector3 LastRotation = Vector3.zero;

    public override void HandleMessage(string flag, string value)
    {
        char[] remove = { '(', ')' };

        if (flag == "POS")
        {
            string[] data = value.Trim(remove).Split(',');

            // If you have Rigidbody
            // Find the distance between client position and server update position
            // if distance < .1 -- ignore
            // else if distance < .5 -- lerp
            // else -- teleport

            Vector3 target = new Vector3(
                float.Parse(data[0]),
                float.Parse(data[1]),
                0
                );
            if ((target - this.transform.position).magnitude < .5f)
            {
                // lerp
                this.transform.position = Vector3.Lerp(this.transform.position, target, .25f);
            }
            else
            {
                this.transform.position = target;
            }
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsServer)
        {
            // Is the position different?
            if (LastPosition != this.transform.position)
            {
                SendUpdate("POS", this.transform.position.ToString());
                LastPosition = this.transform.position;
            }

            if (IsDirty)
            {
                SendUpdate("POS", this.transform.position.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(MyCore.MasterTimer);
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
