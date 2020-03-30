using System;
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
        Froopa = 0,
        Nutbug,
        Reshroom,
        Apple,
        Snapple,
        EyeDye1,
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
    public enum StatusAilment
    {
        ATK_Up = 0,
        DEF_Up,
        Sleep,
        Lucky
    }
    public enum TimelineScript
    {
        None = 0,
        Flash,
        TakenDamage
    }
    private List<Color> EnemyColor = new List<Color>
    {
        new Color(0f, 128f/255f, 94f/255f, 1f),             //Froopa's primary color
        new Color(199f/255f, 170f/255f, 135f/255f, 1f),     //Nutbug's primary color
        new Color(133f/255f ,166f/255f, 1f, 1f),            //Reshroom's primary color
        new Color(196f/255f, 35f/255f, 35f/255f,1f),        //Snapple's primary color
        new Color(1f,1f,1f,1f),                             //Eye Dye's primary color
        new Color(1f, 82f/255f, 82f/255f, 1f),              //Pokerface's primary color
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
    private List<StatusAilment> AilmentList = new List<StatusAilment>();
    private List<GameObject> AilmentBoxes = new List<GameObject>();
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
    private const int maxNumOfStatusAilments = 3;
    private Enemy enemyType = Enemy.EyeDye1;
    private bool hasTakenDamage = false;
    private bool constantVibrate = false;
    private bool damagedInSleep = false;
    private bool isBadApple = false;
    private bool isFlinched = false;
    private bool isFlinchable = false;

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
        EnemyName_Text.transform.position = StatsBar.transform.position + new Vector3(0f, 48f, -5f);
        EnemyName_Text.transform.SetParent(StatsBar.transform.Find("Stats").gameObject.transform);
        EnemyHP_Text = StatsBar.transform.Find("Stats/Enemy HP").gameObject;
        EnemyHP_Text.transform.position = StatsBar.transform.position + new Vector3(105f, 14f, -5f);
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
            //Froopa
            case 0:
                {
                    EnemyStats("Froopa", Element.Wood, 10, 10, 2, 0, 5, UnityEngine.Random.Range(1, 3), Enemy.Froopa, EnemyColor[0]);
                    break;
                }
            //Nutbug
            case 1:
                {
                    EnemyStats("Nutbug", Element.Wood, 8, 8, 2, 1, 8, UnityEngine.Random.Range(1, 3), Enemy.Nutbug, EnemyColor[1]);
                    break;
                }
            //Reshroom
            case 2:
                {
                    EnemyStats("Reshroom", Element.Water, 14, 14, 4, 0, 10, 1, Enemy.Reshroom, EnemyColor[2]);
                    AddStatusAilment(StatusAilment.Sleep, -1);
                    break;
                }
            //Apple
            case 3:
                {
                    EnemyStats("Apple", Element.Wood, 1, 1, 1, 0, 1, 1, Enemy.Apple, EnemyColor[3]);
                    break;
                }
            //Snapple
            case 4:
                {
                    EnemyStats("Snapple", Element.Wood, 36, 36, 4, 1, 45, 2, Enemy.Snapple, EnemyColor[3]);
                    break;
                }
            //Eye Dye 1
            case 9:
                {
                    EnemyStats("Eye Dye", Element.Null, 10, 10, 1, 0, 10, UnityEngine.Random.Range(1, 3), Enemy.EyeDye1, EnemyColor[0]);
                    //enemyName = "Eye Dye";
                    //EnemyName_Text.GetComponent<TMP_Text>().text = enemyName;
                    //Obj.name = enemyName;
                    //mainElement = Element.Null;
                    //OtherFunctions.ChangeSprite(EnemyElement, "Sprites/GameplayUI/Elements", (int)mainElement);
                    //HP = 10;
                    //MaxHP = 10;
                    //EnemyHP_Text.GetComponent<TMP_Text>().text = $"{HP.ToString()}/{MaxHP.ToString()}";
                    //ATK = 1;
                    //DEF = 0;
                    //EXP = 10;
                    //turnsLeft = UnityEngine.Random.Range(1,3);
                    //enemyType = Enemy.EyeDye1;
                    //StatsBar.GetComponent<SpriteRenderer>().color = EnemyColor[0];
                    //TurnCounter.GetComponent<SpriteRenderer>().color = new Color(1f,126f/255f,0f);
                    break;
                }
            //Eye Dye 2
            case 10:
                {
                    EnemyStats("Eye Dye", Element.Null, 10, 10, 2, 0, 10, UnityEngine.Random.Range(1, 3), Enemy.EyeDye2, EnemyColor[0]);
                    break;
                }
            //Eye Dye 3
            case 11:
                {
                    EnemyStats("Eye Dye", Element.Null, 10, 10, 3, 0, 10, UnityEngine.Random.Range(1, 3), Enemy.EyeDye3, EnemyColor[0]);
                    break;
                }
            //Eye Dye 4
            case 12:
                {
                    EnemyStats("Eye Dye", Element.Null, 10, 10, 4, 0, 10, UnityEngine.Random.Range(1, 3), Enemy.EyeDye4, EnemyColor[0]);
                    break;
                }
            //Eye Dye 5
            case 13:
                {
                    EnemyStats("Eye Dye", Element.Null, 10, 10, 5, 0, 10, UnityEngine.Random.Range(1, 3), Enemy.EyeDye5, EnemyColor[0]);
                    break;
                }
            //Eye Dye 6
            case 14:
                {
                    EnemyStats("Eye Dye", Element.Null, 10, 10, 6, 0, 10, UnityEngine.Random.Range(1, 3), Enemy.EyeDye6, EnemyColor[0]);
                    break;
                }
        }
    }
    private void EnemyStats(string eName, Element element, int hp, int maxhp, int atk, int def, int exp, int turnNum, Enemy eType, Color barColor)
    {
        enemyName = eName;
        EnemyName_Text.GetComponent<TMP_Text>().text = eName;
        Obj.name = eName;
        mainElement = element;
        OtherFunctions.ChangeSprite(EnemyElement, "Sprites/GameplayUI/Elements", (int)mainElement);
        HP = hp;
        MaxHP = maxhp;
        EnemyHP_Text.GetComponent<TMP_Text>().text = $"{HP.ToString()}/{MaxHP.ToString()}";
        ATK = atk;
        DEF = def;
        EXP = exp;
        turnsLeft = turnNum;
        enemyType = eType;
        StatsBar.GetComponent<SpriteRenderer>().color = barColor;
        TurnCounter.GetComponent<SpriteRenderer>().color = new Color(1f, 126f / 255f, 0f);
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

    public void AddStatusAilment(StatusAilment SA, int turnNum)
    {
        bool repeatAilmentFound = false;
        int index = AilmentList.Count;
        if (turnNum != -1) { turnNum = Mathf.Clamp(turnNum, 1, 5); }

        for (int i = 0; i < AilmentList.Count; i++)
        {
            if (SA == AilmentList[i]) { repeatAilmentFound = true; index = i; break; }
        }

        if (repeatAilmentFound)
        {

        }
        else
        {
            AilmentList.Add(SA);
            AilmentBoxes.Add(OtherFunctions.CreateObjectFromResource("Prefabs/StatusAilmentIcon_PFB", Obj.transform.position + new Vector3(-96f, 128f + (index * 56f), -10f)));
            LookupAilment(index, SA);
            AilmentBoxes[index].GetComponent<StatusAilmentIcon_SCR>().SetTurns(turnNum);
            AilmentBoxes[index].transform.SetParent(Obj.transform);
        }
    }
    public void RemoveStatusAilment(int boxNum)
    {
        int index = AilmentList.Count - boxNum;
        AilmentList.RemoveAt(index);
        AilmentBoxes.RemoveAt(index);
    }
    private void LookupAilment(int index, StatusAilment SA)
    {
        switch (SA)
        {
            case StatusAilment.ATK_Up:
                {
                    OtherFunctions.ChangeSprite(AilmentBoxes[index], "Sprites/GameplayUI/StatusAilmentIcons", 0);
                    break;
                }
            case StatusAilment.DEF_Up:
                {
                    OtherFunctions.ChangeSprite(AilmentBoxes[index], "Sprites/GameplayUI/StatusAilmentIcons", 1);
                    break;
                }
            case StatusAilment.Sleep:
                {
                    OtherFunctions.ChangeSprite(AilmentBoxes[index], "Sprites/GameplayUI/StatusAilmentIcons", 2);
                    break;
                }
        }
    }
    public string GetName() { return enemyName; }
    public Enemy GetEnemyType() { return enemyType; }
    public int GetElement() { return (int)mainElement; }
    public int GetHP() { return HP; }
    public int GetDefense() { return DEF; }
    public void ReceiveDamage(int damage, bool ignoreDefense = false, bool flinchEnemy = false)
    {
        int fullDamage = 0;
        float elementMultiplier = DamageMultiplier[BM.GetComponent<BattleManager_SCR>().GetPlayerElement(), (int)mainElement];

        if (!ignoreDefense) { fullDamage = Mathf.Max(fullDamage, 0, (int)(damage * elementMultiplier) - DEF); }
        else { fullDamage = Mathf.Max(fullDamage, 0, (int)(damage * elementMultiplier)); }
        HP -= fullDamage;
        hasTakenDamage = true;
        if (flinchEnemy)
        {
            if (!isFlinchable) { isFlinched = true; }
            else { WriteMessage($"The {enemyName} cannot be flinched!", true); }
        }
        if (enemyType == Enemy.Apple) { isBadApple = true; }
        for (int i = 0; i < AilmentList.Count; i++)
        {
            if (AilmentList[i] == StatusAilment.Sleep) { damagedInSleep = true; break; }
        }
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
        if (!isFlinched || enemyType == Enemy.Apple)
        {
            switch (enemyType)
            {
                //Froopa
                case Enemy.Froopa: { StartCoroutine(Tackle()); return 2.5f; }
                //Nutbug
                case Enemy.Nutbug: { StartCoroutine(Ram()); return 2.5f; }
                //Reshroom
                case Enemy.Reshroom:
                    {
                        bool wakeUpRNG = RandomChance(50);
                        if (damagedInSleep && wakeUpRNG) { StartCoroutine(SleepAttack(true)); return 3.25f; }
                        else { StartCoroutine(SleepAttack(false)); return 1.5f; }
                    }
                //Apple
                case Enemy.Apple:
                    {
                        if (isBadApple) { StartCoroutine(Reveal()); return 6f; }
                        else { StartCoroutine(AppleWaiting()); return 4.25f; }
                    }
                //Apple
                case Enemy.Snapple:
                    {
                        if (RandomChance(50)) { StartCoroutine(Bite()); return 2f; }
                        else if (RandomChance(40))
                        {
                            int numOfHits = UnityEngine.Random.Range(2, 6);
                            StartCoroutine(SeedBarrage(numOfHits)); return 2f + (0.2f * numOfHits);
                        }
                        else { StartCoroutine(JuicyBite()); return 4f; }
                    }
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
        else
        {
            isFlinched = false;
            isFlinchable = true;
            WriteMessage($"The {enemyName} was flinched and cannot move!", true);
            turnsLeft = 1;
            return 1f;
        }
    }
    private void AddHPToEnemy(int hp)
    {
        HP += hp;
    }
    private void DealDamage(int baseDamage, int luckyRate)
    {
        int finalDamage = 0;
        float elementMultiplier = DamageMultiplier[(int)mainElement, BM.GetComponent<BattleManager_SCR>().GetPlayerElement()];

        float luckyMultiplier = 1f;
        bool randomChance = RandomChance(luckyRate);
        if (randomChance) { luckyMultiplier = 2f;}

        finalDamage = (int)(baseDamage * elementMultiplier * luckyMultiplier);
        StartCoroutine(BM.GetComponent<BattleManager_SCR>().ReceiveDamage(finalDamage));
    }

    private void WriteMessage(string msg, bool typewriterMode) { BM.GetComponent<BattleManager_SCR>().WriteMessage(msg, typewriterMode); }
    private void ClearMessage() { BM.GetComponent<BattleManager_SCR>().ClearMessage(); }


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

    private bool RandomChance(int percentage) { return (UnityEngine.Random.Range(0, 100) < percentage); }
    private IEnumerator Tackle()
    {
        bool tripRNG = RandomChance(20);
        WriteMessage($"The {enemyName} tried to tackle you!", true);
        yield return new WaitForSeconds(1f);
        if (tripRNG) { WriteMessage($"Unfortunately, the {enemyName} tripped on himself!", true); }
        else { DealDamage(ATK, 2); }
        turnsLeft = UnityEngine.Random.Range(1, 3);
    }
    private IEnumerator Ram()
    {
        WriteMessage($"The {enemyName} rammed its hard body at you!", true);
        yield return new WaitForSeconds(1f);
        DealDamage(ATK, 2);
        turnsLeft = 2;
    }
    private IEnumerator SleepAttack(bool isWoke)
    {
        if (isWoke)
        {
            ClearMessage();
            OtherFunctions.CreateObjectFromResource("Prefabs/BubbleSign_PFB", Obj.transform.position + new Vector3(0f, 16f, 0f));
            yield return new WaitForSeconds(1f);
            WriteMessage($"The {enemyName} woke up!", true);
            yield return new WaitForSeconds(0.75f);
            WriteMessage($"The {enemyName} fired a slimy bubble!", true);
            yield return new WaitForSeconds(1f);
            DealDamage(ATK, 0);
        }
        else
        {
            WriteMessage($"The {enemyName} continues to rest!", true);
            AddHPToEnemy(4);
            yield return new WaitForSeconds(1f);
        }
        damagedInSleep = false;
        turnsLeft = 1;
    }
    private IEnumerator AppleWaiting()
    {
        WriteMessage($"There's an {enemyName} on the ground!", true);
        yield return new WaitForSeconds(2f);
        WriteMessage($"It sure looks ripe and alluring!", true);
        yield return new WaitForSeconds(1.5f);
        turnsLeft = 1;
    }
    private IEnumerator Reveal()
    {
        hasTakenDamage = true;
        constantVibrate = true;
        vibration = 10f;
        yield return new WaitForSeconds(1f);
        WriteMessage($"The {enemyName} seems to be moving!", true);
        yield return new WaitForSeconds(1f);
        WriteMessage($"Wait a minute!?", true);
        yield return new WaitForSeconds(1f);
        WriteMessage($"This {enemyName} is a luring trap!", true);
        yield return new WaitForSeconds(1f);
        constantVibrate = false;
        OtherFunctions.ChangeSprite(Obj, "Sprites/Enemies/Snapple");
        AssignStats(4, 0);
        WriteMessage($"The Snapple makes a surprising appearance!", true);
        yield return new WaitForSeconds(1f);
    }
    private IEnumerator Bite()
    {
        WriteMessage($"The {enemyName} takes a soft bite on you!", true);
        yield return new WaitForSeconds(1f);
        DealDamage(ATK, 0);
        turnsLeft = UnityEngine.Random.Range(1, 3);
    }
    private IEnumerator JuicyBite()
    {
        int[] victimTile = new int[16] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        for (int i = victimTile.Length - 1; i > 0; i--)
        {
            // might have a range error based random number
            int j = Mathf.FloorToInt((UnityEngine.Random.value * (i + 1)) % victimTile.Length);
            int temp = victimTile[i];
            victimTile[i] = victimTile[j];
            victimTile[j] = temp;
        }

        WriteMessage($"The {enemyName} takes a nice, juicy bite on you!", true);
        yield return new WaitForSeconds(1f);
        ClearMessage();
        GameObject[] tempGrid = BM.GetComponent<BattleManager_SCR>().GetPlayerGrid();
        DealDamage(ATK-2, 10);
        for (int i = 0; i < 6; i++)
        {
            GameObject Fang1 = OtherFunctions.CreateObjectFromResource("Prefabs/EmptyObject_PFB", tempGrid[victimTile[i]].transform.position);
            Fang1.AddComponent<BiteAnimation_SCR>();
            Fang1.GetComponent<BiteAnimation_SCR>().SetFrameIndex(0);
            GameObject Fang2 = OtherFunctions.CreateObjectFromResource("Prefabs/EmptyObject_PFB", tempGrid[victimTile[i]].transform.position);
            Fang2.AddComponent<BiteAnimation_SCR>();
            Fang2.GetComponent<BiteAnimation_SCR>().SetFrameIndex(1);
            Fang1.transform.position = tempGrid[victimTile[i]].transform.position + new Vector3(0f, 56f, 0f);
            Fang1.GetComponent<BiteAnimation_SCR>().SetTile(tempGrid[victimTile[i]]);
            Fang2.transform.position = tempGrid[victimTile[i]].transform.position + new Vector3(0f, -56f, 0f);
            Fang2.GetComponent<BiteAnimation_SCR>().SetTile(tempGrid[victimTile[i]]);
        }
        HP += 6;
        yield return new WaitForSeconds(1f);
        WriteMessage($"The {enemyName} regained some of its health from your tiles", true);
        yield return new WaitForSeconds(1.5f);
        turnsLeft = UnityEngine.Random.Range(1, 3);
    }
    private IEnumerator SeedBarrage(int numOfHits)
    {
        WriteMessage($"The {enemyName} ate several apples and attempted to shoot them!", true);
        yield return new WaitForSeconds(1f);
        for (int tries = 0; tries < numOfHits; tries++)
        {
            DealDamage(1, 5*tries);
            yield return new WaitForSeconds(0.2f);
        }
        WriteMessage($"It landed {numOfHits} hits!", true);
        turnsLeft = 2;
    }
    private IEnumerator DieAttack()
    {
        WriteMessage($"The {enemyName} rolled itself at you!", true);
        yield return new WaitForSeconds(1f);
        DealDamage(ATK, 0);
        turnsLeft = UnityEngine.Random.Range(1, 4);
    }
}
