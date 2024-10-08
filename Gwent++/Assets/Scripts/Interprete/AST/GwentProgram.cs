using System;
using System.Collections.Generic;
using UnityEngine;
public class GwentProgram : Stmt
{
    public List<Effect> Effects {get; set;}
    public List<CardComp> Cards {get; set;}
    public override Scope AssociatedScope {get; set;}
    public GwentProgram(CodeLocation location) : base (location)
    {
        Effects = new List<Effect>();
        Cards = new List<CardComp>();
    }
    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        this.AssociatedScope = scope;
        //Se chequean semanticamente los efectos
        bool checkEffects = true;
        foreach (Effect effect in Effects)
        {
            checkEffects = checkEffects && effect.CheckSemantic(context, AssociatedScope, errors);
        }

        //Se chequean semanticamente las cartas
        bool checkCards = true;
        foreach (CardComp card in Cards)
        {
            checkCards = checkCards && card.CheckSemantic(context, AssociatedScope, errors);
        }

        return checkCards && checkEffects;
    }
    public override void Interprete()
    {
        foreach (CardComp card in Cards)
        {
            card.CardBuilder();
        }
    }
    public override string ToString()
    {
        string s = "";
        foreach (Effect effect in Effects)
        {
            s = s + "\n" + effect.ToString();
        }
        foreach (CardComp card in Cards)
        {
            s += "\n" + card.ToString();
        }
        return s;
    }
}