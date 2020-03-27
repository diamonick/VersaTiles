using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EaseFunctions;
using TMPro;

public class Command_SCR : MonoBehaviour
{
    public enum Rarity
    {
        Rare = 5,
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
        SuperHeal,      // Average
        FullHeal,       // Very Uncommon
        SlowHeal,       // Average
        PowerUp,        // Average
        DefenseUp,      // Average
        Resist,         // Average
        Cure,           // Average
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
            cmd = GetRandomCommand(new List<Command>() { Command.Heal });
        }
        else if (RandomChance((int)Rarity.Average))
        {
            cmd = GetRandomCommand(new List<Command>() { Command.SuperHeal });
        }
        else if (RandomChance((int)Rarity.Uncommon))
        {
            cmd = GetRandomCommand(new List<Command>() { Command.None });
        }
        else if (RandomChance((int)Rarity.VeryUncommon))
        {
            cmd = GetRandomCommand(new List<Command>() { Command.FullHeal });
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
            case Command.SuperHeal: { AssignStats("Super Heal", 5, "Average", "You", new Color(1f, 81f / 255f, 184f / 255f), "Heals 30 HP."); break; }
            case Command.FullHeal: { AssignStats("Full Heal", 10, "Very Uncommon", "You", new Color(1f, 81f / 255f, 184f / 255f), "Fully heals your HP."); break; }
        }
        StartCoroutine(EasingFunctions.ColorChangeFromRGBA(Obj, new Color(SPR.color.r, SPR.color.g, SPR.color.b, 1f), 0.25f, Format.Scalar));
    }
    private void WriteMessage(string msg, bool typewriterMode) { BM.GetComponent<BattleManager_SCR>().WriteMessage(msg, typewriterMode); }

    public float ActivateCommand()
    {
        WriteMessage($"You used {cmdName}!", true);
        switch (command)
        {
            case Command.None: { StartCoroutine(Dummy()); return 2f; }
            case Command.Heal: { StartCoroutine(Heal()); return 2f; }
            case Command.SuperHeal: { StartCoroutine(SuperHeal()); return 2f; }
            case Command.FullHeal: { StartCoroutine(FullHeal()); return 2f; }
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
