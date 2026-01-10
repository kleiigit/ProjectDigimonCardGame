using ProjectScript.Enums;
using SinuousProductions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace ProjectScript.EffectManager
{
    public class EffectPrompt
    {
        public Card UserEffect { get; }
        public Keyword EffectType { get; private set; }
        public int Quantity { get; private set; }

        public CardType TypeTarget { get; private set; } = CardType.Card;
        public int PowerTarget { get; private set; }
        public DigimonField DigimonField { get; private set; } = DigimonField.NoField;
        public bool IsLesser { get; private set; }

        public bool OpponentSide { get; private set; } = false;
        public FieldPlace PlaceTarget { get; private set; } = FieldPlace.BattleZone;

        public Keyword Effect { get; private set; } = Keyword.None;
        public int EffectQuantity { get; private set; }

        public List<GameObject> SelectedTarget { get; private set; } = new List<GameObject>();
        private readonly string PromptedEffect;

        public EffectPrompt(Card user, string prompt)
        {
            UserEffect = user;
            PromptedEffect = prompt;
            SplitPrompt();
        }
        private void SplitPrompt()
        {
            string[] commands = PromptedEffect.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (commands.Length == 0)
                return;

            int index = 0;

            // 1 - Tipo de efeito (obrigatório)
            ParseEffectType(commands[index++]);

            // 2 - Tipo de carta alvo (opcional)
            if (index < commands.Length)
            {
                string key = commands[index].Split(',')[0].ToLower();
                if (IsTargetCommand(key))
                {
                    TargetFiltered(commands[index]);
                    index++;
                }
            }

            // 3 - Localização / lado (opcional)
            if (index < commands.Length)
            {
                string key = commands[index].Split(',')[0].ToLower();

                bool isLocation =
                    key == "play" ||
                    key == "opo" ||
                    Enum.TryParse<FieldPlace>(key, true, out _);

                if (isLocation)
                {
                    ParseTargetLocation(commands[index]);
                    index++;
                }
            }

            // 4 - Efeito aplicado (opcional)
            if (index < commands.Length)
            {
                string key = commands[index].Split(',')[0];
                if (KeywordFromString(key) != Keyword.None)
                {
                    ParseEffectKeyword(commands[index]);
                }
            }
        }


        #region Parsing
        private bool IsTargetCommand(string command)
        {
            // Comandos válidos de alvo: digimon, card, etc.
            return command switch
            {
                "digi" => true,
                "card" => true,
                _ => false
            };
        }
        private void ParseEffectType(string cmd)
        {
            var parts = cmd.Split(',');
            EffectType = KeywordFromString(parts[0]);
            Quantity = parts.Length > 1 && int.TryParse(parts[1], out var q) ? q : 0;
        }
        private void TargetFiltered(string cmd)
        {
            var parts = cmd.Split(',');

            TypeTarget = CardTypeFromString(parts[0]);

            if (parts.Length > 1)
            {
                if (!FieldFromString(parts[1], out var field))
                {
                    PowerTarget = int.TryParse(parts[1], out var n) ? n : 0;
                    IsLesser = parts.Length > 2 && parts[2].Equals("less", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    DigimonField = field;
                }
            }
        }
        private void ParseTargetLocation(string cmd)
        {
            var parts = cmd.Split(',');

            // Define lado
            if (parts[0].Equals("opo", StringComparison.OrdinalIgnoreCase))
            {
                OpponentSide = true;
            }
            else if (parts[0].Equals("play", StringComparison.OrdinalIgnoreCase))
            {
                OpponentSide = false;
            }
            else if (Enum.TryParse(parts[0], true, out FieldPlace placeOnly))
            {
                // Apenas local, assume lado do jogador
                OpponentSide = false;
                PlaceTarget = placeOnly;
                return;
            }

            // Define local (se existir)
            if (parts.Length > 1 && Enum.TryParse(parts[1], true, out FieldPlace place))
            {
                PlaceTarget = place;
            }
        }
        private void ParseEffectKeyword(string cmd)
        {
            var parts = cmd.Split(',');
            Effect = KeywordFromString(parts[0]);
            if (Effect == Keyword.None)
                Effect = Keyword.Draw;
            EffectQuantity = parts.Length > 1 && int.TryParse(parts[1], out var q) ? q : 0;
        }
        #endregion

        #region Conversions
        private Keyword KeywordFromString(string key) => key switch
        {
            "TE" => Keyword.Target,
            "CE" => Keyword.Condition,
            "Draw" => Keyword.Draw,
            "Cache" => Keyword.Cache,
            "Down" => Keyword.Down,
            "discard" => Keyword.Discard,
            "destroy" => Keyword.Destroy,
            _ => Keyword.None
        };
        private bool FieldFromString(string str, out DigimonField field)
        {
            field = str switch
            {
                "dr" => DigimonField.DragonsRoar,
                _ => default
            };
            return field != default;
        }
        private CardType CardTypeFromString(string str) => str switch
        {
            "digi" => CardType.Digimon,
            "card" => CardType.Card,
            _ => CardType.Card
        };
        #endregion

        #region Smart Targeting

        public bool TryAddTarget(List<GameObject> possibleTargets)
        {
            foreach (var target in possibleTargets)
            {
                if (IsValidTarget(target))
                {
                    SelectedTarget.Add(target);
                    if (SelectedTarget.Count >= Quantity)
                        break;
                }
            }
            return SelectedTarget.Count > 0;
        }

        private bool IsValidTarget(GameObject target)
        {
            if (target == null) return false;
            var cardDisplay = target.GetComponent<CardDisplay>();
            if (cardDisplay?.cardData == null) return false;

            var card = cardDisplay.cardData;
            if (card.cardType != TypeTarget) return false;

            if (card is DigimonCard digimon)
            {
                if (PowerTarget > 0)
                {
                    if (IsLesser && digimon.Power >= PowerTarget) return false;
                    if (!IsLesser && digimon.Power != PowerTarget) return false;
                }
                if (DigimonField != DigimonField.NoField && digimon.Field != DigimonField) return false;
            }

            return true;
        }

        #endregion

        public override string ToString()
        {
            string sidefield = OpponentSide == false ? "player" : "oponent";

            string targetCard = TypeTarget.ToString();
            if (PowerTarget > 0)
            { 
                targetCard += " " + PowerTarget + " power or "; 
                targetCard += IsLesser ? "less" : "more";
            }
            if (DigimonField != DigimonField.NoField) targetCard += " " + DigimonField;

            return $"{EffectType} {Quantity} - " +
                $"({targetCard}) - " +
                $"({sidefield} {PlaceTarget}) - {Effect} {EffectQuantity}";
        }
    }
}
