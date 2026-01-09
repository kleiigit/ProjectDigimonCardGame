using ProjectScript.Enums;
using SinuousProductions;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Processor.EffectManager
{
    public class EffectPrompt
    {
        public Card UserEffect {  get; }
        //
        public Keyword EffectType { get; set; }
        public int Quantity { get; set; }
        //
        public CardType TypeTarget { get; set; } = CardType.Card;
        public int PowerTarget { get; set; }
        public DigimonField DigimonField { get; set; } = DigimonField.NoField;
        public bool IsLesser { get; set; }
        //
        public bool OpponentSide { get; set; } = false;
        public FieldPlace PlaceTarget { get; set; } = FieldPlace.BattleZone;
        //
        public Keyword Effect { get; set; }
        //
        public List<GameObject> SelectedTarget { get; set; }
        private string PromptedEffect { get; set; }

        public EffectPrompt(Card user, string prompt)
        {
            UserEffect = user;
            PromptedEffect = prompt;
            SplitPrompt();
        }

        void SplitPrompt()
        {
            string[] commands = PromptedEffect.Split(';'); //Separa os comandos
            if (commands.Length == 0)
            {
                Debug.LogWarning($"[ExecuteCardEffect] Malformed: \"{PromptedEffect}\"");
                return;
            }

            // Comando de alvo e quantidade de alvos
            string[] commandSplit = commands[0].Split(',');
            EffectType = KeywordConvert(commandSplit[0]);
                if (commandSplit.Length > 1) 
                    Quantity = int.TryParse(commandSplit[1], out var q) ? q : 0;   
            if (commands.Length > 1)
            {
                // Comando de filtro de alvo
                TargetFiltered(commands[1]);
            }
            if(commands.Length > 2)
            {
                // Comando de local do alvo
                commandSplit = commands[2].Split(',');
                OpponentSide = commandSplit[0] == "opo" ? true : false;
                if (commandSplit.Length > 1)
                    PlaceTarget = Enum.TryParse(commandSplit[1], true, out FieldPlace v) ? v : FieldPlace.BattleZone;
            }
            if (commands.Length > 3)
            {
                // Comando de Efeito realizado
                commandSplit = commands[3].Split(",");
                Effect = Enum.TryParse(commandSplit[0], true, out Keyword v) ? v : Keyword.Destroy;
            }
        }

        void TargetFiltered(string prompt)
        {
            string[] commandSplit = prompt.Split(',');
            TypeTarget = CardTypeConvert(commandSplit[0], out CardType v) ? v : CardType.Card;
            
            if (commandSplit.Length > 1)
                // digimon Field
                if (FieldConvert(commandSplit[1], out DigimonField field))
                {
                    DigimonField = field;
                    return;
                }
                // digimon Power
                else if (int.TryParse(commandSplit[1], out var n))
                { 
                    PowerTarget = n;
                    IsLesser = commandSplit[2] == "less" ? true : false;
                    return; 
                }
                else
                {
                    Debug.Log("Error Convert: " + commandSplit[1]);
                }
        }
        Keyword KeywordConvert(string keyString)
        {
            switch(keyString)
            {
                case "TE": return Keyword.Target;
                case "CE": return Keyword.Condition;
                case "Draw": return Keyword.Draw;
                case "Cache": return Keyword.Cache;
                case "Down": return Keyword.Down;
                default:
                    break;
            }
            Debug.LogWarning("[EffectPrompt] Error ao Converter Keyword: "+ keyString);
            return 0;
        }
        bool FieldConvert(string fieldString, out DigimonField field)
        {
            switch (fieldString)
            {
                case "dr":
                    field = DigimonField.DragonsRoar;
                    return true;
                default:
                    break;
            }
            field = default;
            return false;
        }
        bool CardTypeConvert(string cardTypeString, out CardType card)
        {
            switch (cardTypeString)
            {
                case "digi":
                    card = CardType.Digimon;
                    return true;
                default:
                    break;
            }
            Debug.LogWarning("[EffectPrompt] Error ao Converter CardType: " + cardTypeString);
            card = default;
            return false;
        }

        public override string ToString()
        {
            return EffectType.ToString() + " " + Quantity +
                " - (" + TypeTarget.ToString() + " " + 
                PowerTarget + " " + DigimonField + " " + IsLesser + 
                ") - (" + OpponentSide + " " + PlaceTarget + " " +
                ") - " + EffectType.ToString();
        }
    }
}
