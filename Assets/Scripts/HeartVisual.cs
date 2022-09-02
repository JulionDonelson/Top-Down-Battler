using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartVisual : MonoBehaviour
{
    public int health;
    public SpriteRenderer SprRen;
    public Sprite[] heartState = new Sprite[5];

    public void ChangeOfHeart(int change)
    {
        health = change;
        if (health >= 0 && health <= 4)
        {
            SprRen.sprite = heartState[health];
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
