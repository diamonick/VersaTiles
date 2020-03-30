﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EaseFunctions;

public class AttackArrow_SCR : MonoBehaviour
{
    private List<Color> ElementColor = new List<Color>
    {
        new Color(218f/255f, 218f/255f, 218f/255f, 1f),     //Null color
        new Color(1f, 144f/255f, 0f, 1f),                   //Fire color
        new Color(0f, 156f/255f, 1f, 1f),                   //Water color
        new Color(1f, 234f/255f, 0f, 1f),                   //Electric color
        new Color(21f/255f, 222f/255f, 0f, 1f),             //Wood color
        new Color(1f, 238f/255f, 170f/255f, 1f),            //Light color
        new Color(106f/255f, 0f, 196f/255f, 1f),            //Dark color
    };
    private GameObject Obj;
    private GameObject EnemyTarget;
    private int fullDamage = 0;
    private bool showLuckyText = false;
    private float timeVal = 0.5f;
    private readonly float timeValMax = 60f;

    private void Awake()
    {
        Obj = this.gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(EasingFunctions.ScaleTo(Obj, new Vector2(4f, 4f), 0.25f, 2, Easing.EaseOut));
    }

    // Update is called once per frame
    void Update()
    {
        if (EnemyTarget != null)
        {
            Vector3 dir = EnemyTarget.transform.position - Obj.transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Obj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            if (timeVal > 0f) { timeVal -= 1f * Time.deltaTime; }
            else
            {
                if (showLuckyText) { OtherFunctions.CreateObjectFromResource("Prefabs/LuckyText_PFB", Obj.transform.position); }
                EnemyTarget.GetComponent<Enemy_SCR>().ReceiveDamage(fullDamage);
                Destroy(Obj);
            }
        }
    }

    public void AcquireTarget(GameObject Enemy, int elementNum)
    {
        EnemyTarget = Enemy;
        Obj.GetComponent<SpriteRenderer>().color = ElementColor[elementNum];
    }

    public void TransferDamage(int damage, int LUCK)
    {
        int luckyMultiplier = 1;
        if (RandomChance(LUCK)) { luckyMultiplier = 2; showLuckyText = true; }

        fullDamage = damage * luckyMultiplier;
    }
    private bool RandomChance(int percentage) { return (Random.Range(0, 100) < percentage); }
}
