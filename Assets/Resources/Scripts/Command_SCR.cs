using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EaseFunctions;
using TMPro;

public class Command_SCR : MonoBehaviour
{
    public enum Rarity
    {
        Rare = 100,
        VeryUncommon = 20,
        Uncommon = 40,
        Average = 50,
        Common = 60,
    }
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
    public enum Command
    {
        None = 0,
        Heal,           // Common
        SuperHeal,      // Uncommon
        FullHeal,       // Very Uncommon
        SlowHeal,       // Average
        PowerUp,        // Average
        DefenseUp,      // Average
        Resist,         // Average
        Cure,           // Average
        Shuffle,        // Common
        SpreadAttack,   // Common
        PiercingBlow,   // Uncommon
        Counter,        //
        Slap,           // Common
        Burn,           // Common
        BigBurst,       // Very Uncommon
        Freeze,         // Average
        RainDance,      // Average
        BubbleBombs,    //
        Paralyze,       // Common
        E_Strike,       // Average
        ThunderRage,    // Very Uncommon
        Absorb,         // Uncommon
        LightSpears,    // Average
        Reflect,        // Average
        Poison,         // Common
        LuckyBoost,     // Uncommon
        StatusShare,    // Uncommon
        InstantKO,      // Rare
        LuckyCharm,     // Very Uncommon
        LuckySevenStreak,// Rare
        Immunity,       // Very Uncommon
    }

    private GameObject Obj;
    private GameObject BM;
    private GameObject Name_Text;
    private GameObject Cost_Text;
    [SerializeField] private Command command = Command.None;
    private SpriteRenderer SPR;
    private string cmdName = "";
    [SerializeField] private int CP_Cost = 0;
    [SerializeField] private string cmdDescription;
    private string cmdAffectStr;
    private string cmdRarityStr;
    private bool isAssigned = false;
    private bool startVibrating = false;
    private float vibration = 0f;
    private Vector3 staticPos;

    private void Awake()
    {
        Obj = this.gameObject;
        staticPos = Obj.transform.position;
        SPR = Obj.GetComponent<SpriteRenderer>();
        BM = GameObject.Find("BattleManager");
        Name_Text = Obj.transform.Find("Canvas/Name Text").gameObject;
        Cost_Text = Obj.transform.Find("Canvas/CP_Cost Text").gameObject;
        Name_Text.transform.position = Obj.transform.position + new Vector3(16f, 0f, -1f);
        Cost_Text.transform.position = Obj.transform.position + new Vector3(260f, 0f, -1f);
        SPR.color = new Color(1f, 1f, 1f, 0.25f);
        Obj.transform.Find("Canvas").GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (startVibrating)
        {
            float randX = Random.Range(-vibration, vibration);
            float randY = Random.Range(-vibration, vibration);
            Obj.transform.position = staticPos + new Vector3(randX, randY, 0f);

            if (vibration > 0f) { vibration -= 0.25f; }
            else { vibration = 0; startVibrating = false; }
        }
        Name_Text.transform.position = Obj.transform.position + new Vector3(16f, 0f, -1f);
        Cost_Text.transform.position = Obj.transform.position + new Vector3(260f, 0f, -1f);
    }

    public string GetName() { return Name_Text.GetComponent<TMP_Text>().text; }
    public int GetCost() { return CP_Cost; }
    public string GetDescription() { return cmdDescription; }
    public string GetAffect() { return cmdAffectStr; }
    public string GetRarity() { return cmdRarityStr; }
    public Color GetColor() { return SPR.color; }
    public void Vibrate() { startVibrating = true; vibration = 15f; }

    private Command GetCommand(int elem)
    {
        Command cmd = Command.None;
        Element playerElement = (Element)elem;

        if (RandomChance((int)Rarity.Common))
        {
            cmd = GetRandomCommand(new List<Command>() { Command.Heal, Command.Shuffle, Command.Slap });
        }
        else if (RandomChance((int)Rarity.Average))
        {
            cmd = GetRandomCommand(new List<Command>() { Command.PowerUp, Command.DefenseUp });
        }
        else if (RandomChance((int)Rarity.Uncommon))
        {
            cmd = GetRandomCommand(new List<Command>() { Command.SuperHeal, Command.LuckyBoost });
        }
        else if (RandomChance((int)Rarity.VeryUncommon))
        {
            cmd = GetRandomCommand(new List<Command>() { Command.FullHeal, Command.LuckyCharm });
        }
        else if (RandomChance((int)Rarity.Rare))
        {
            cmd = GetRandomCommand(new List<Command>() { Command.None });
        }

        return cmd;
    }

    private bool RandomChance(int percentage) { return (Random.Range(0, 100) < percentage); }
    private Command GetRandomCommand(List<Command> commands) { return commands[Random.Range(0, commands.Count)]; }
    private bool MatchElement(Element firstElement, Element secondElement) { return (firstElement == secondElement); }

    public bool CommandIsAssigned() { return isAssigned; }
    public void AssignCommand()
    {
        isAssigned = true;
        command = GetCommand(BM.GetComponent<BattleManager_SCR>().GetPlayerElement());

        switch (command)
        {
            case Command.None: { AssignStats("Dummy", 5, "N/A", "N/A", new Color(0.5f, 0.5f, 0.5f), "Does nothing."); break; }
            case Command.Heal: { AssignStats("Heal", 2, "Common", "You", new Color(1f, 81f/255f, 184f/255f), "Heals 10 HP."); break; }
            case Command.SuperHeal: { AssignStats("Super Heal", 5, "Uncommon", "You", new Color(1f, 81f / 255f, 184f / 255f), "Heals 30 HP."); break; }
            case Command.FullHeal: { AssignStats("Full Heal", 10, "Very Uncommon", "You", new Color(1f, 81f / 255f, 184f / 255f), "Fully heals your HP."); break; }
            case Command.Shuffle: { AssignStats("Shuffle", 1, "Common", "You", new Color(0f, 214f / 255f, 32f / 255f), "Shuffles all tiles in your grid."); break; }
            case Command.Slap: { AssignStats("Slap", 1, "Common", "Enemy", new Color(223f / 255f, 164f / 255f, 121f / 255f), "Flinches the enemy. Can only be used once per enemy."); break; }
            case Command.PowerUp: { AssignStats("Power Up", 2, "Average", "You", new Color(1f, 0f, 0f), "Boosts your Attack by 1 stage."); break; }
            case Command.DefenseUp: { AssignStats("Defense Up", 2, "Average", "You", new Color(0f, 84f / 255f, 1f), "Boosts your Defense by 1 stage."); break; }
            case Command.LuckyBoost: { AssignStats("Lucky Boost", 3, "Uncommon", "You", new Color(1f, 184f / 255f, 83f / 255f), "Boosts the chance of landing Lucky hits by 1 stage."); break; }
            case Command.LuckyCharm: { AssignStats("Lucky Charm", 6, "Very Uncommon", "You", new Color(1f, 213f / 255f, 44f / 255f), "Boosts the chance of landing Lucky hits by 2 stages."); break; }
        }
        StartCoroutine(EasingFunctions.ColorChangeFromRGBA(Obj, new Color(SPR.color.r, SPR.color.g, SPR.color.b, 1f), 0.25f, Format.Scalar));
    }
    private void WriteMessage(string msg, bool typewriterMode) { BM.GetComponent<BattleManager_SCR>().WriteMessage(msg, typewriterMode); }
    private void ClearMessage() { BM.GetComponent<BattleManager_SCR>().ClearMessage(); }

    public float ActivateCommand()
    {
        WriteMessage($"You used {cmdName}!", true);
        switch (command)
        {
            case Command.None: { StartCoroutine(Dummy()); return 2f; }
            case Command.Heal: { StartCoroutine(Heal()); return 2f; }
            case Command.SuperHeal: { StartCoroutine(SuperHeal()); return 2f; }
            case Command.FullHeal: { StartCoroutine(FullHeal()); return 2f; }
            case Command.Shuffle: { StartCoroutine(Shuffle()); return 2.5f; }
            case Command.Slap: { StartCoroutine(Slap()); return 2.5f; }
            case Command.PowerUp: { StartCoroutine(PowerUp()); return 2.5f; }
            case Command.DefenseUp: { StartCoroutine(DefenseUp()); return 2.5f; }
            case Command.LuckyBoost: { StartCoroutine(LuckyBoost()); return 2.5f; }
            case Command.LuckyCharm: { StartCoroutine(LuckyCharm()); return 2.5f; }
        }
        return 2f;
    }

    private IEnumerator Dummy()
    {
        yield return new WaitForSeconds(1f);
        WriteMessage("It does nothing!", true);
    }

    private IEnumerator Heal()
    {
        yield return new WaitForSeconds(1f);
        BM.GetComponent<BattleManager_SCR>().AddHPToPlayer(10);
    }
    private IEnumerator SuperHeal()
    {
        yield return new WaitForSeconds(1f);
        BM.GetComponent<BattleManager_SCR>().AddHPToPlayer(30);
    }
    private IEnumerator FullHeal()
    {
        int currentHP = BM.GetComponent<BattleManager_SCR>().GetPlayer().HP;
        int maxHP = BM.GetComponent<BattleManager_SCR>().GetPlayer().MaxHP;
        yield return new WaitForSeconds(1f);
        BM.GetComponent<BattleManager_SCR>().AddHPToPlayer(maxHP - currentHP);
    }
    private IEnumerator Shuffle()
    {
        GameObject[] tempGrid = BM.GetComponent<BattleManager_SCR>().GetPlayerGrid();
        Playtile_SCR.Tile[] tileTypes = new Playtile_SCR.Tile[16];
        Vector3[] tilePositions = new Vector3[16];
        for (int i = 0; i < tilePositions.Length; i++)
        {
            tilePositions[i] = tempGrid[i].transform.position;
            tileTypes[i] = tempGrid[i].GetComponent<Playtile_SCR>().GetTileType();
        }
        int frameIndex = 0;
        for (int i = 0; i < tempGrid.Length; i++)
        {
            if (tempGrid[i].GetComponent<Playtile_SCR>().GetTileType() == Playtile_SCR.Tile.ElementSwap)
            {
                frameIndex = tempGrid[i].GetComponent<Playtile_SCR>().GetFrameIndex();
                Debug.Log($"frameIndex: {frameIndex}");
            }
        }
        for (int i = tileTypes.Length - 1; i > 0; i--)
        {
            // might have a range error based random number
            int j = Mathf.FloorToInt((Random.value * (i + 1)) % tileTypes.Length);
            Playtile_SCR.Tile temp = tileTypes[i];
            tileTypes[i] = tileTypes[j];
            tileTypes[j] = temp;
        }

        yield return new WaitForSeconds(1f);
        for (int i = 0; i < tempGrid.Length; i++) { StartCoroutine(EasingFunctions.TranslateTo(tempGrid[i], new Vector3(454f, 467f, 0f), 0.5f, 3, Easing.EaseOut)); }
        yield return new WaitForSeconds(0.75f);
        for (int i = 0; i < tempGrid.Length; i++)
        {
            BM.GetComponent<BattleManager_SCR>().SetPlaytile(i, (int)tileTypes[i], frameIndex);
            StartCoroutine(EasingFunctions.TranslateTo(tempGrid[i], tilePositions[i], 0.5f, 3, Easing.EaseOut));
        }
    }
    private IEnumerator Slap()
    {
        yield return new WaitForSeconds(1f);
        ClearMessage();
        GameObject EnemyTarget = BM.GetComponent<BattleManager_SCR>().GetEnemyTarget();
        GameObject HandSlap = OtherFunctions.CreateObjectFromResource("Prefabs/EmptyObject_PFB", EnemyTarget.transform.position + new Vector3(-128f, 64f, -1f));
        HandSlap.AddComponent<SlapAnimation_SCR>();
    }
    private IEnumerator PowerUp()
    {
        yield return new WaitForSeconds(1f);
        BM.GetComponent<BattleManager_SCR>().AddATKToPlayer();
        int playerATKLvl = BM.GetComponent<BattleManager_SCR>().GetPlayer().ATK_LevelNum;
        WriteMessage($"Your ATK stat was increased! Current ATK level: {playerATKLvl}", true);
    }
    private IEnumerator DefenseUp()
    {
        yield return new WaitForSeconds(1f);
        BM.GetComponent<BattleManager_SCR>().AddDEFToPlayer();
        int playerDEFLvl = BM.GetComponent<BattleManager_SCR>().GetPlayer().DEF_LevelNum;
        WriteMessage($"Your DEF stat was increased! Current DEF level: {playerDEFLvl}", true);
    }
    private IEnumerator LuckyBoost()
    {
        yield return new WaitForSeconds(1f);
        BM.GetComponent<BattleManager_SCR>().AddLUCKToPlayer();
        int playerLUCKLvl = BM.GetComponent<BattleManager_SCR>().GetPlayer().LUCK_LevelNum;
        WriteMessage($"Your LUCK stat was increased! Current LUCK level: {playerLUCKLvl}", true);
    }
    private IEnumerator LuckyCharm()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 2; i++) { BM.GetComponent<BattleManager_SCR>().AddLUCKToPlayer(); }
        int playerLUCKLvl = BM.GetComponent<BattleManager_SCR>().GetPlayer().LUCK_LevelNum;
        WriteMessage($"Your LUCK stat was increased! Current LUCK level: {playerLUCKLvl}", true);
    }

    private void AssignStats(string name, int cp_cost, string rarity, string affectsWho, Color col, string desc)
    {
        cmdName = name;
        Name_Text.GetComponent<TMP_Text>().text = name;
        CP_Cost = cp_cost;
        Cost_Text.GetComponent<TMP_Text>().text = $"{CP_Cost} CP";
        SPR.color = col;
        cmdDescription = desc;
        cmdAffectStr = affectsWho;
        cmdRarityStr = rarity;
    }

    public void EraseStats()
    {
        isAssigned = false;
        command = Command.None;
        cmdName = "";
        Name_Text.GetComponent<TMP_Text>().text = "";
        CP_Cost = 0;
        Cost_Text.GetComponent<TMP_Text>().text = "";
        SPR.color = new Color(1f, 1f, 1f, 0.25f);
        cmdDescription = "";
        cmdAffectStr = "";
        cmdRarityStr = "";
    }
}
