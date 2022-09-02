using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

public enum PlayerState
{
    walk,
    attack,
    stagger
}

public enum HealthState
{
    vulnerable,
    stagger,
    invincible
}

public class PlayerMovement : NetworkComponent
{
    public float speed;
    public float knockTime;
    public Rigidbody2D myRigidBody;
    public Animator myAnim;
    public GameObject heart;
    public Text PnameText;
    public PlayerState currentState = PlayerState.walk;
    public HealthState currentHealth = HealthState.vulnerable;
    public Vector3 LastPosition = Vector3.zero;
    public Vector3 LastRotation = Vector3.zero;

    public string Pname;
    public int maxHealth;
    public int health;

    public override void HandleMessage(string flag, string value)
    {
        char[] remove = { '(', ')' };

        if (flag == "PNAME")
        {
            Pname = value;
            PnameText.text = Pname;
        }

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
            Vector3 diff = target - this.transform.position;
            if (diff.magnitude < .5f)
            {
                // lerp
                this.transform.position = Vector3.Lerp(this.transform.position, target, .25f);
            }
            else
            {
                this.transform.position = target;
            }
            myAnim.SetFloat("moveX", diff.x);
            myAnim.SetFloat("moveY", diff.y);
            myAnim.SetBool("moving", true);
        }

        if (flag == "MOVE")
        {
            if (IsServer)
            {
                string[] args = value.Split(',');
                Vector3 movement = new Vector3(float.Parse(args[0]), float.Parse(args[1]), 0);
                myRigidBody.MovePosition(transform.position + movement.normalized * speed);

                myAnim.SetFloat("moveX", movement.x);
                myAnim.SetFloat("moveY", movement.y);
                myAnim.SetBool("moving", true);
            }
        }

        if (flag == "ATTACK")
        {
            if (currentState != PlayerState.attack)
            {
                StartCoroutine(AttackCo());
                if (IsServer)
                {
                    SendUpdate("ATTACK", value);
                }
            }
        }

        if (flag == "IDLE")
        {
            myAnim.SetBool("moving", false);
        }

        if (flag == "VELOCITY")
        {
            myRigidBody.velocity = new Vector2(0, 0);
            if (IsServer)
            {
                SendUpdate("VELOCITY", value);
            }
        }

        if (flag == "KNOCKBACK")
        {
            string[] data = value.Trim(remove).Split(',');

            Vector3 diff = new Vector3(
                float.Parse(data[0]),
                float.Parse(data[1]),
                0
                );

            currentState = PlayerState.stagger;
            // myRigidBody.isKinematic = false;
            myRigidBody.AddForce(diff, ForceMode2D.Impulse);
            StartCoroutine(KnockCo());
        }

        if (flag == "DAMAGE")
        {
            if (currentHealth == HealthState.vulnerable)
            {
                int damage = int.Parse(value);
                health -= damage;
                heart.GetComponent<HeartVisual>().ChangeOfHeart(health);
                currentHealth = HealthState.stagger;
                StartCoroutine(DamageCo());
                if (IsServer)
                {
                    SendUpdate("DAMAGE", value);
                }
                if (health <= 0)
                {
                    MyCore.NetCreateObject(2, Owner, Vector3.zero);
                    MyCore.NetDestroyObject(this.NetId);
                }
            }
        }

        if (flag == "HEAL")
        {
            health = maxHealth;
            heart.GetComponent<HeartVisual>().ChangeOfHeart(health);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            Debug.Log("Listening for Player " + Owner);
        }
        if (!IsLocalPlayer)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }
        while (IsLocalPlayer)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            float a = Input.GetAxisRaw("Fire1");
            if (a != 0 && currentState != PlayerState.attack)
            {
                SendCommand("ATTACK", a.ToString());
            }
            else if ((h != 0 || v != 0) && currentState == PlayerState.walk)
            {
                SendCommand("MOVE", h + "," + v);
            }
            else if (currentState == PlayerState.walk)
            {
                SendCommand("IDLE", "false");
            }

            if (currentState != PlayerState.stagger)
            {
                SendCommand("VELOCITY", "0");
            }
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
        while (IsServer)
        {
            // Is the position different?
            if (LastPosition != this.transform.position)
            {
                SendUpdate("POS", this.transform.position.ToString());
                LastPosition = this.transform.position;
            }
            else
            {
                SendUpdate("IDLE", "false");
            }

            if (IsDirty)
            {
                SendUpdate("POS", this.transform.position.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    private IEnumerator AttackCo()
    {
        myAnim.SetBool("attacking", true);
        currentState = PlayerState.attack;
        yield return null;
        myAnim.SetBool("attacking", false);
        yield return new WaitForSeconds(.2f);
        currentState = PlayerState.walk;
    }

    private IEnumerator KnockCo()
    {
        yield return new WaitForSeconds(knockTime);
        myRigidBody.velocity = Vector2.zero;
        // myRigidBody.isKinematic = true;
        currentState = PlayerState.walk;
    }

    private IEnumerator DamageCo()
    {
        yield return new WaitForSeconds(knockTime);
        if (currentHealth == HealthState.stagger)
        {
            currentHealth = HealthState.vulnerable;
        }
    }

    public void HealUp()
    {
        health = maxHealth;
        heart.GetComponent<HeartVisual>().ChangeOfHeart(health);
        SendUpdate("HEAL", "4");
    }

    public void StartingParameters(string pn)
    {
        Pname = pn;
        PnameText.text = Pname;
        SendUpdate("PNAME", Pname);
    }

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();
        myAnim.SetFloat("moveY", -1);
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
