using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ePyCardState{
    drawpile,
    tableau,
    target,
    discard
}

public class CardPyramid : Card
{
    [Header("set Dynamically: CardPyramid")]
    public ePyCardState               state = ePyCardState.drawpile;
    public List<CardPyramid>       hiddenBy = new List<CardPyramid>();
    public int                      layoutID;
    public SlotDefPy                  slotDefPy;

    public override void OnMouseUpAsButton()
    {
        Pyramid.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}
