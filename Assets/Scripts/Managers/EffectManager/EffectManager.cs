using SinuousProductions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectScript.Enums;
using System;
public class EffectManager : MonoBehaviour
{
    public TargetSelector targetSelector; // Referência ao TargetSelector na cena

    public void ExecuteCardEffect(CardEffects effect, Card card)
    {
        string[] effectRecipe = CardEffects.EffectTypePrompt(effect.promptEffect);

        if (effectRecipe == null || effectRecipe.Length == 0)
        {
            Debug.LogWarning($"[ExecuteCardEffect] Empty or invalid effect on: {card.cardName}");
            return;
        }

        var teCommands = effectRecipe.Where(s => s.StartsWith("TE")).ToList();

        if (teCommands.Count == 0)
        {
            Debug.LogWarning($"[ExecuteCardEffect] No 'TE' command found for: {card.cardName}");
            return;
        }

        foreach (string teCommand in teCommands) // Se o efeito é TE
        {
            Debug.Log($"Processing: {teCommand}");

            string[] commands = teCommand.Split(';');
            if (commands.Length == 0)
            {
                Debug.LogWarning($"[ExecuteCardEffect] Malformed 'TE' command: \"{teCommand}\"");
                continue;
            }

            string commandType = commands[0]; // Ex: "TE,1"
            string[] commandTypeSplit = commandType.Split(',');

            if (commandTypeSplit.Length == 0 || commandTypeSplit[0] != "TE")
            {
                Debug.LogWarning($"[ExecuteCardEffect] Unknown or invalid command type: {commandType}");
                continue;
            }

            int quantity = 1;
            if (commandTypeSplit.Length > 1)
                int.TryParse(commandTypeSplit[1], out quantity);  // Pega a quantidade, se especificada

            // Construir critérios para filtragem
            TargetCriteria criteria = new TargetCriteria
            {
                count = quantity,
                requiredCardCondition = new RequiredCard()
            };

            // Armazena o efeito final a aplicar (ex: "destroy")
            string finalEffect = null;

            // Processa os comandos após "TE,1"
            for (int i = 1; i < commands.Length; i++)
            {
                string targetCommand = commands[i].Trim();
                string[] parts = targetCommand.Split(',');

                if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
                {
                    Debug.LogWarning($"[ExecuteCardEffect] Empty or invalid command: \"{targetCommand}\"");
                    continue;
                }

                string targetType = parts[0].Trim().ToLower();

                // Caso o comando final seja o efeito (ex: destroy)
                if (targetType == "destroy" || targetType == "discard" || targetType == "down" || targetType == "freeze")
                {
                    finalEffect = targetType;
                    continue;
                }

                List<string> specifications = parts.Skip(1).ToList();

                if (targetType == "digi")
                {
                    criteria.requiredCardCondition.typeCard = CardType.Digimon;

                    foreach (string spec in specifications)
                    {
                        string s = spec.Trim().ToLower();

                        if (Enum.TryParse(s, true, out CardColor color))
                        {
                            criteria.requiredCardCondition.colorCard = color;
                            continue;
                        }

                        if (Enum.TryParse(s, true, out DigimonAttribute attribute))
                        {
                            criteria.requiredCardCondition.attriDigimon = attribute;
                            continue;
                        }

                        if (Enum.TryParse(s, true, out DigimonField field))
                        {
                            criteria.requiredCardCondition.fieldDigimon = field;
                            continue;
                        }

                        if (s.StartsWith("name="))
                        {
                            criteria.requiredCardCondition.nameCard = s.Substring(5);
                            continue;
                        }

                        if (s.StartsWith("id="))
                        {
                            criteria.requiredCardCondition.IDOfCard = s.Substring(3);
                            continue;
                        }

                        if (Enum.TryParse(s, true, out DigimonType type))
                        {
                            criteria.requiredCardCondition.typeDigimon = type;
                            continue;
                        }

                        string[] tokens = s.Split(',');
                        if (tokens.Length == 3 && tokens[0] == "digi")
                        {
                            if (int.TryParse(tokens[1], out int value))
                            {
                                string comparison = tokens[2];

                                if (value <= 10) // Considerado como level
                                {
                                    criteria.requiredCardCondition.levelDigimon = value;

                                    criteria.requiredCardCondition.compareLevel = true;
                                    criteria.requiredCardCondition.levelLessThanOrEqual = comparison == "less";
                                    criteria.requiredCardCondition.levelGreaterThanOrEqual = comparison == "more";
                                }
                                else // Considerado como Power
                                {
                                    criteria.requiredCardCondition.PowerDigimon = value;

                                    criteria.requiredCardCondition.comparePower = true;
                                    criteria.requiredCardCondition.powerLessThanOrEqual = comparison == "less";
                                    criteria.requiredCardCondition.powerGreaterThanOrEqual = comparison == "more";
                                }

                                continue;
                            }
                        }

                        if (s.StartsWith("lv"))
                        {
                            if (int.TryParse(s.Replace("lv", ""), out int lvl))
                            {
                                criteria.requiredCardCondition.levelDigimon = lvl;
                                continue;
                            }
                        }

                        if (int.TryParse(s, out int power))
                        {
                            criteria.requiredCardCondition.PowerDigimon = power;
                            continue;
                        }

                        Debug.LogWarning($"[ExecuteCardEffect] Especificação não reconhecida: {s}");
                    }
                }
                else if (targetType == "opo") // Exemplo para opponent field
                {
                    if (specifications.Count > 0)
                    {
                        if (System.Enum.TryParse(specifications[0], true, out DigimonField field))
                        {
                            criteria.requiredCardCondition.fieldDigimon = field;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[ExecuteCardEffect] Target type não implementado: {targetType}");
                }
            }

            // Busca todas as cartas em campo e filtra
            List<FieldCard> allFieldCards = FindObjectsByType<FieldCard>(FindObjectsSortMode.None).ToList();

            List<FieldCard> validTargets = allFieldCards.Where(c => criteria.Matches(c)).ToList();

            Debug.Log($"Targets válidos encontrados: {validTargets.Count}");

            if (validTargets.Count > 0)
            {
                // Chama o TargetSelector para seleção manual, passando os alvos válidos e callback de aplicação do efeito
                targetSelector.SelectTargets(criteria, validTargets, (selectedCards) =>
                {
                    foreach (var selectedCard in selectedCards)
                    {
                        ApplyEffectToCard(finalEffect, selectedCard);
                    }
                });
            }
            else
            {
                Debug.LogWarning("[ExecuteCardEffect] Nenhum alvo válido encontrado para o efeito TE.");
            }
        }
    }

    private void ApplyEffectToCard(string effect, FieldCard target)
    {
        switch (effect)
        {
            case "destroy":
                target.DestroyFieldCard();
                break;

            case "discard":
                // Implementar efeito de descartar
                break;

            case "down":
                // Implementar efeito down
                break;

            case "freeze":
                // Implementar efeito freeze
                break;

            default:
                Debug.LogWarning($"[ApplyEffectToCard] Efeito desconhecido: {effect}");
                break;
        }
    }
}
