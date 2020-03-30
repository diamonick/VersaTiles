﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusAilmentIcon_SCR : MonoBehaviour
{
    public enum StatusAilment
    {
        ATK_Up = 0,
        DEF_Up,
        Sleep,
        Lucky
    }
    private GameObject Obj;
    private GameObject turnsLeft_Text;
    private int turnsLeft = 0;
    private StatusAilment SA = StatusAilment.ATK_Up;

    private void Awake()
    {
        Obj = this.gameObject;
        turnsLeft_Text = Obj.transform.Find("Canvas/TurnsLeft Text").gameObject;
        turnsLeft_Text.transform.position = Obj.transform.position + new Vector3(23f, -1f, -5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (turnsLeft_Text != null) { turnsLeft_Text.transform.position = Obj.transform.position + new Vector3(23f, -1f, -5f); }
    }
    public bool isTurnZero() { return turnsLeft == 0; }
    public void SetTurns(int turnNum)
    {
        turnsLeft = turnNum;
        if (turnNum != -1) { turnsLeft_Text.GetComponent<TMP_Text>().text = $"{turnsLeft}"; }
    }
    public void SetStatusAilment(int ailmentNum) { SA = (StatusAilment)ailmentNum; }
    public int GetStatusAilment() { return (int)SA; }
    public void DecrementTurn()
    {
        if (turnsLeft != -1)
        {
            turnsLeft--;
            turnsLeft_Text.GetComponent<TMP_Text>().text = $"{turnsLeft}";
        }
    }
}
