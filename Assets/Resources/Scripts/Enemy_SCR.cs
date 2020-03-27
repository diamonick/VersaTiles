﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EaseFunctions;
using TMPro;

public class Enemy_SCR : MonoBehaviour
{
    public enum Enemy
    {
        EyeDye1 = 0,
        EyeDye2,
        EyeDye3,
        EyeDye4,
        EyeDye5,
        EyeDye6,
        Pokerface,
        Devol,
        Luckitty,
        Eigther,
        Bugbyte,
        Blootooth,
        Spiritune,
        Deletus,
        Coldbase,
        AgileTeam,
        AnonymousGuy
    }
    public enum TimelineScript
    {
        None = 0,
        Flash,
        TakenDamage
    }
    private List<Color> EnemyColor = new List<Color>
    {
        new Color(1f,1f,1f,1f),                     //Eye Dye's primary color
        new Color(1f, 82f/255f, 82f/255f, 1f),      //Pokerface's primary color
    };
    private readonly float[,] DamageMultiplier = new float[7, 7]
    {
        {1f,1f,1f,1f,1f,1f,1f},     //Null
        {1f,1f,0.5f,1f,2f,1f,1f},   //Fire
        {1f,2f,1f,0.5f,1f,1f,1f},   //Water
        {1f,1f,2f,1f,0.5f,1f,1f},   //Electric
        {1f,0.5f,1f,2f,1f,1f,1f},   //Wood
        {1f,1f,1f,1f,1f,1f,2f},     //Light
        {1f,1f,1f,1f,1f,2f,1f},     //Dark
    };
    private GameObject Obj;
    private GameObject StatsBar;
    private GameObject EnemyName_Text;
    private GameObject EnemyHP_Text;
    private GameObject EnemyHPBar;
    private GameObject EnemyHP_HideBar;
    private GameObject EnemyElement;
    private Image EnemyHP_HideBarMask;
    private GameObject Selection;
    private GameObject[] SelectTick = new GameObject[4];
    private GameObject TurnCounter;
    private GameObject TurnCounter_Text;
    private bool isDefeated = false;
    private GameObject BM;
    private SpriteRenderer SPR;
    private Material flashMat;
    private Material defaultMat;
    public enum Element
    {
        Null,
        Fire,
        Water,
        Electric,
        Wood,
        Light,
        Dark
    }
    private int ID = 0;
    [SerializeField] private string enemyName;
    [SerializeField] private Element mainElement;
    [SerializeField] private int HP;
    [SerializeField] private int MaxHP;
    [SerializeField] private int ATK;
    [SerializeField] private int DEF;
    [SerializeField] private int EXP;
    [SerializeField] private int turnsLeft;
    private Enemy enemyType = Enemy.EyeDye1;
    private bool hasTakenDamage = false;

    //Timeline Variables
    private int SS_Animation_index = 0;
    private float timeVal = 2f;
    private readonly float timeValMax = 60f;
    private bool timeline_running = true;
    private TimelineScript TLS = TimelineScript.None;
    private bool isCoroutineRunning = false;
    private int flashCycle = 0;
    private const int flashCycleLimit = 4;
    private float vibration = 0f;
    private Vector3 staticPos;

    private void Awake()
    {
        Obj = this.gameObject;
        BM = GameObject.Find("BattleManager");
        SPR = Obj.GetComponent<SpriteRenderer>();
        flashMat = Resources.Load<Material>("Materials/Flash_MAT");
        defaultMat = SPR.material;
        staticPos = Obj.transform.position;
        StatsBar = OtherFunctions.CreateObjectFromResource("Prefabs/Enemy_StatsBar_PFB", Obj.transform.position + new Vector3(-132f, -144f, -105f));
        TurnCounter = OtherFunctions.CreateObjectFromResource("Prefabs/TurnCounter_PFB", Obj.transform.position + new Vector3(0f, 0f, -105f));
    }

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        HP = Mathf.Clamp(HP, 0, MaxHP);
        bool isDanger = (((float)HP / (float)MaxHP) <= 0.25f);

        EnemyHP_HideBarMask.fillAmount = 1f - ((float)HP / (float)MaxHP);

        if (isDanger)
        {
            float damageTint = ((Mathf.Sin(2f * DateTime.Now.Millisecond / 150f) + 1));
            damageTint = Mathf.Clamp(damageTint, 0.1f, 1f);
            EnemyHPBar.GetComponent<SpriteRenderer>().color = new Color(1f, damageTint, damageTint, 1f);
        }

        if (isDefeated)
        {
            vibration += 1f;
            SPR.material.color -= new Color(2f * Time.deltaTime, 2f * Time.deltaTime, 2f * Time.deltaTime, 0f);
            if (SPR.material.color.r <= 0f)
            {
                for (int starCount = 0; starCount < 16; starCount++)
                {
                    float speed = (starCount < 8 ? 12f : 24f);
                    GameObject Star = OtherFunctions.CreateObjectFromResource("Prefabs/StarFX_PFB", staticPos - new Vector3(0f, 0f, -10f));
                    Star.GetComponent<StarFX_SCR>().AssignIndex(starCount, speed);

                }
                BM.GetComponent<BattleManager_SCR>().AddEXP(EXP);
                Destroy(TurnCounter);
                Destroy(StatsBar);
                Destroy(Obj);
            }
        }

        timeVal = Mathf.Clamp(timeVal, 0f, timeValMax);
        if (timeline_running)
        {
            if (timeVal > 0f) { timeVal -= 1f * Time.deltaTime; }
            else { StartCoroutine(ExecuteTimeline(TLS)); }
        }

        if (hasTakenDamage)
        {
            float randX = UnityEngine.Random.Range(-vibration, vibration);
            float randY = UnityEngine.Random.Range(-vibration, vibration);
            Obj.transform.position = staticPos + new Vector3(randX, randY, 0f);

            if (HP > 0)
            {
                if (vibration > 0f) { vibration -= 0.25f; }
                else { vibration = 0; hasTakenDamage = false; }
            }
        }
        else { Obj.transform.position = staticPos; }
        FormatText();
    }
    IEnumerator ExecuteTimeline(TimelineScript timelinescript)
    {
        if (isCoroutineRunning) { yield break; }
        isCoroutineRunning = true;

        if (timelinescript == TimelineScript.Flash)
        {
            SS_Animation_index++;
            switch (SS_Animation_index)
            {
                case 1:
                    {
                        SPR.material = flashMat;
                        timeVal = 0.2f;
                        break;
                    }
                case 2:
                    {
                        SPR.material = defaultMat;
                        SS_Animation_index = 0;
                        timeVal = 0.2f;
                        flashCycle++;
                        if (flashCycle >= flashCycleLimit)
                        {
                            flashCycle = 0;
                            TLS = TimelineScript.None;
                            BM.GetComponent<BattleManager_SCR>().ClearMessage();
                            Destroy(Selection);
                        }
                        break;
                    }
            }
        }

        isCoroutineRunning = false;
    }

    public void AssignStats(int enemyID, int spawnNum)
    {
        StatsBar.name = $"StatsBar {spawnNum}";
        EnemyHPBar = StatsBar.transform.Find("EnemyHPBar").gameObject;
        EnemyName_Text = StatsBar.transform.Find("Stats/Enemy Name").gameObject;
        EnemyName_Text.transform.position = StatsBar.transform.position + new Vector3(0f, 46f, -5f);
        EnemyName_Text.transform.SetParent(StatsBar.transform.Find("Stats").gameObject.transform);
        EnemyHP_Text = StatsBar.transform.Find("Stats/Enemy HP").gameObject;
        EnemyHP_Text.transform.position = StatsBar.transform.position + new Vector3(105f, 17f, -5f);
        EnemyHP_Text.transform.SetParent(StatsBar.transform.Find("Stats").gameObject.transform);
        EnemyHP_HideBar = StatsBar.transform.Find("EnemyHPBar/EnemyHP FullBar/EnemyHP DamageBar").gameObject;
        EnemyHP_HideBar.transform.position = StatsBar.transform.position + new Vector3(59f, -12f, -5f);
        EnemyHP_HideBar.transform.SetParent(StatsBar.transform.Find("EnemyHPBar/EnemyHP FullBar/EnemyHP DamageBar").gameObject.transform);
        EnemyHP_HideBarMask = EnemyHP_HideBar.GetComponent<Image>();
        EnemyElement = StatsBar.transform.Find("Element_PFB").gameObject;

        float spriteHalfWidth = SPR.bounds.extents.x;
        float spriteHalfHeight = SPR.bounds.extents.y;
        TurnCounter.transform.position += new Vector3(spriteHalfWidth, spriteHalfHeight, 0f);
        TurnCounter.name = $"TurnCounter {spawnNum}";
        TurnCounter_Text = TurnCounter.transform.Find("Canvas/TurnCounterText").gameObject;
        TurnCounter_Text.transform.position = TurnCounter.transform.position + new Vector3(spriteHalfWidth, spriteHalfHeight - 5f, -5f);
        TurnCounter_Text.transform.SetParent(TurnCounter.transform.Find("Canvas").gameObject.transform);


        switch (enemyID)
        {
            //Eye Dye 1
            case 0:
                {
                    enemyName = "Eye Dye";
                    EnemyName_Text.GetComponent<TMP_Text>().text = enemyName;
                    Obj.name = enemyName;
                    mainElement = Element.Null;
                    OtherFunctions.ChangeSprite(EnemyElement, "Sprites/GameplayUI/Elements", (int)mainElement);
                    HP = 10;
                    MaxHP = 10;
                    EnemyHP_Text.GetComponent<TMP_Text>().text = $"{HP.ToString()}/{MaxHP.ToString()}";
                    ATK = 1;
                    DEF = 0;
                    EXP = 10;
                    turnsLeft = UnityEngine.Random.Range(1,3);
                    enemyType = Enemy.EyeDye1;
                    StatsBar.GetComponent<SpriteRenderer>().color = EnemyColor[0];
                    TurnCounter.GetComponent<SpriteRenderer>().color = new Color(1f,126f/255f,0f);
                    break;
                }
            //Eye Dye 2
            case 1:
                {
                    enemyName = "Eye Dye";
                    EnemyName_Text.GetComponent<TMP_Text>().text = enemyName;
                    Obj.name = enemyName;
                    mainElement = Element.Null;
                    OtherFunctions.ChangeSprite(EnemyElement, "Sprites/GameplayUI/Elements", (int)mainElement);
                    HP = 10;
                    MaxHP = 10;
                    EnemyHP_Text.GetComponent<TMP_Text>().text = $"{HP.ToString()}/{MaxHP.ToString()}";
                    ATK = 2;
                    DEF = 0;
                    EXP = 10;
                    turnsLeft = UnityEngine.Random.Range(1, 3);
                    enemyType = Enemy.EyeDye2;
                    StatsBar.GetComponent<SpriteRenderer>().color = EnemyColor[0];
                    TurnCounter.GetComponent<SpriteRenderer>().color = new Color(1f, 126f / 255f, 0f);
                    break;
                }
            //Eye Dye 3
            case 2:
                {
                    enemyName = "Eye Dye";
                    EnemyName_Text.GetComponent<TMP_Text>().text = enemyName;
                    Obj.name = enemyName;
                    mainElement = Element.Null;
                    OtherFunctions.ChangeSprite(EnemyElement, "Sprites/GameplayUI/Elements", (int)mainElement);
                    HP = 10;
                    MaxHP = 10;
                    EnemyHP_Text.GetComponent<TMP_Text>().text = $"{HP.ToString()}/{MaxHP.ToString()}";
                    ATK = 3;
                    DEF = 0;
                    EXP = 10;
                    turnsLeft = UnityEngine.Random.Range(1, 3);
                    enemyType = Enemy.EyeDye3;
                    StatsBar.GetComponent<SpriteRenderer>().color = EnemyColor[0];
                    TurnCounter.GetComponent<SpriteRenderer>().color = new Color(1f, 126f / 255f, 0f);
                    break;
                }
            //Eye Dye 4
            case 3:
                {
                    enemyName = "Eye Dye";
                    EnemyName_Text.GetComponent<TMP_Text>().text = enemyName;
                    Obj.name = enemyName;
                    mainElement = Element.Null;
                    OtherFunctions.ChangeSprite(EnemyElement, "Sprites/GameplayUI/Elements", (int)mainElement);
                    HP = 10;
                    MaxHP = 10;
                    EnemyHP_Text.GetComponent<TMP_Text>().text = $"{HP.ToString()}/{MaxHP.ToString()}";
                    ATK = 4;
                    DEF = 0;
                    EXP = 10;
                    turnsLeft = UnityEngine.Random.Range(1, 3);
                    enemyType = Enemy.EyeDye4;
                    StatsBar.GetComponent<SpriteRenderer>().color = EnemyColor[0];
                    TurnCounter.GetComponent<SpriteRenderer>().color = new Color(1f, 126f / 255f, 0f);
                    break;
                }
            //Eye Dye 5
            case 4:
                {
                    enemyName = "Eye Dye";
                    EnemyName_Text.GetComponent<TMP_Text>().text = enemyName;
                    Obj.name = enemyName;
                    mainElement = Element.Null;
                    OtherFunctions.ChangeSprite(EnemyElement, "Sprites/GameplayUI/Elements", (int)mainElement);
                    HP = 10;
                    MaxHP = 10;
                    EnemyHP_Text.GetComponent<TMP_Text>().text = $"{HP.ToString()}/{MaxHP.ToString()}";
                    ATK = 5;
                    DEF = 0;
                    EXP = 10;
                    turnsLeft = UnityEngine.Random.Range(1, 3);
                    enemyType = Enemy.EyeDye5;
                    StatsBar.GetComponent<SpriteRenderer>().color = EnemyColor[0];
                    TurnCounter.GetComponent<SpriteRenderer>().color = new Color(1f, 126f / 255f, 0f);
                    break;
                }
            //Eye Dye 6
            case 5:
                {
                    enemyName = "Eye Dye";
                    EnemyName_Text.GetComponent<TMP_Text>().text = enemyName;
                    Obj.name = enemyName;
                    mainElement = Element.Null;
                    OtherFunctions.ChangeSprite(EnemyElement, "Sprites/GameplayUI/Elements", (int)mainElement);
                    HP = 10;
                    MaxHP = 10;
                    EnemyHP_Text.GetComponent<TMP_Text>().text = $"{HP.ToString()}/{MaxHP.ToString()}";
                    ATK = 6;
                    DEF = 0;
                    EXP = 10;
                    turnsLeft = UnityEngine.Random.Range(1, 3);
                    enemyType = Enemy.EyeDye6;
                    StatsBar.GetComponent<SpriteRenderer>().color = EnemyColor[0];
                    TurnCounter.GetComponent<SpriteRenderer>().color = new Color(1f, 126f / 255f, 0f);
                    break;
                }
        }
    }
    private void FormatText()
    {
        EnemyHP_Text.GetComponent<TMP_Text>().text = $"{HP.ToString()}/{MaxHP.ToString()}";
        TurnCounter_Text.GetComponent<TMP_Text>().text = $"{turnsLeft}";
    }

    public void Flash()
    {
        float spriteWidth = Obj.GetComponent<SpriteRenderer>().bounds.size.x;
        float spriteHeight = Obj.GetComponent<SpriteRenderer>().bounds.size.y;
        TLS = TimelineScript.Flash;
        timeVal = 0f;
        CreateSelection(spriteWidth, spriteHeight, new Color(1f, 1f, 1f, 1f), 2f);
        WriteMessage($"You're now targeting {enemyName}.", false);
    }

    public bool isFlashing() { return TLS == TimelineScript.Flash; }

    public void StopFlashing()
    {
        SPR.material = defaultMat;
        SS_Animation_index = 0;
        timeVal = 0.2f;
        flashCycle = 0;
        TLS = TimelineScript.None;
        if (Selection != null) { Destroy(Selection); }
    }

    public string GetName() { return enemyName; }
    public Enemy GetEnemyType() { return enemyType; }
    public int GetElement() { return (int)mainElement; }
    public int GetHP() { return HP; }
    public int GetDefense() { return DEF; }
    public void ReceiveDamage(int damage)
    {
        int fullDamage = 0;
        float elementMultiplier = DamageMultiplier[BM.GetComponent<BattleManager_SCR>().GetPlayerElement(), (int)mainElement];

        fullDamage = Mathf.Max(fullDamage, 0, (int)(damage * elementMultiplier) - DEF);
        HP -= fullDamage;
        hasTakenDamage = true;
        vibration = fullDamage * 2f;
        timeVal = 0f;
    }

    public void EnemyIsDefeated()
    {
        isDefeated = true;
        SPR.material = flashMat;
    }

    public bool DecrementTurn()
    {
        turnsLeft--;
        if (turnsLeft == 0) { return true; }
        return false;
    }

    public float ChooseMove()
    {
        switch (enemyType)
        {
            //Eye Dye 1
            case Enemy.EyeDye1: { StartCoroutine(DieAttack()); return 2f; }
            //Eye Dye 2
            case Enemy.EyeDye2: { StartCoroutine(DieAttack()); return 2f; }
            //Eye Dye 3
            case Enemy.EyeDye3: { StartCoroutine(DieAttack()); return 2f; }
            //Eye Dye 4
            case Enemy.EyeDye4: { StartCoroutine(DieAttack()); return 2f; }
            //Eye Dye 5
            case Enemy.EyeDye5: { StartCoroutine(DieAttack()); return 2f; }
            //Eye Dye 6
            case Enemy.EyeDye6: { StartCoroutine(DieAttack()); return 2f; }
            default: { return 1f; }
        }
    }

    private void DealDamage(int baseDamage, float luckyRate)
    {
        int finalDamage = 0;
        float elementMultiplier = DamageMultiplier[(int)mainElement, BM.GetComponent<BattleManager_SCR>().GetPlayerElement()];

        float luckyMultiplier = 1f;
        int randomChance = UnityEngine.Random.Range(0, 101);
        if (randomChance < luckyRate) { luckyMultiplier = 2f;}

        finalDamage = (int)(baseDamage * elementMultiplier * luckyMultiplier);
        StartCoroutine(BM.GetComponent<BattleManager_SCR>().ReceiveDamage(finalDamage));
    }

    private void WriteMessage(string msg, bool typewriterMode) { BM.GetComponent<BattleManager_SCR>().WriteMessage(msg, typewriterMode); }

    private IEnumerator DieAttack()
    {
        WriteMessage($"The {enemyName} rolled itself at the player!", true);
        yield return new WaitForSeconds(1f);
        DealDamage(ATK, 0);
        turnsLeft = UnityEngine.Random.Range(1, 4);
    }

    private void CreateSelection(float width, float height, Color col, float scale = 1f, float padding = 0f)
    {
        Vector3 pos = Obj.transform.position;
        Selection = OtherFunctions.CreateObjectFromResource("Prefabs/EmptyObject_PFB", pos);

        for (int i = 0; i < SelectTick.Length; i++)
        {
            if (SelectTick[i] == null)
            {
                Vector3 relPos = new Vector3(0f, 0f, 0f);
                if (i == 0) { relPos = new Vector3((-width / 2f) - padding, (height / 2f) + padding, -2f); }        //Top-left corner
                else if (i == 1) { relPos = new Vector3((width / 2f) + padding, (height / 2f) + padding, -2f); }    //Top-right corner
                else if (i == 2) { relPos = new Vector3((-width / 2f) - padding, (-height / 2f) - padding, -2f); }  //Bottom-left corner
                else if (i == 3) { relPos = new Vector3((width / 2f) + padding, (-height / 2f) - padding, -2f); }   //Bottom-right corner

                SelectTick[i] = OtherFunctions.CreateObjectFromResource("Prefabs/SelectTick_PFB", pos + relPos);
                SelectTick[i].GetComponent<SelectTick_SCR>().SetTickBorder(i);
                SelectTick[i].transform.localScale = new Vector3(scale, scale, 1f);
                SelectTick[i].transform.SetParent(Selection.transform);
                SelectTick[i].GetComponent<SpriteRenderer>().color = col;
            }
        }

        Selection.transform.SetParent(Obj.transform);
    }
}