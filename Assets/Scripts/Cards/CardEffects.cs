using System;
using UnityEngine;

namespace SinuousProductions
{
    [System.Serializable]
    public class CardEffects
    {
        [Header("Action to activate the effect")]
        public Trigger trigger; // tipo de gatilho do efeito
        [TextArea(4, 8)]
        public string promptEffect;
        #region("Effect Type")
        [Header("TE = Target Effect - Alvo do efeito seguindo pelo efeito no alvo \n" +
            "CE = Condition Effect - é necessário cumprir para ativar o efeito\n" +
            "EF = Effect - efeitos sem restrições\n" +
            "CD = Cost Data - custo de carta data\n" +
            "Down = efeito de abaixar\n" +
            "Draw,1 = comprar 1 carta\n" +
            "OVER = Overclock\n" +
            "DE = Declare nível ou quantidade\n" +
            "PRO = Protection - Proteger de ataques\n" +
            "CL = Colorful")]
        #endregion
        [TextArea(3, 10)]
        public string DescriptionEffect;

        public enum Trigger
        {
            NoTrigger,
            OnPlay, // ao jogar a carta
            Constant, // efeito constante
            Action, // ação específica
            Auto, // efeito automático
            Security, // efeito de segurança
            Protection, // efeito de proteção
            Trash, 
            Colorfull, // efeito colorido
        }
        public enum EffectType
        {
            TargetEffect,
            ConditionEffect,
            Effect,
            CostData,
            Down,
            Overclock,
            Declare,
            Protection,
            ColorfulEffect,
        }
        public enum SidePlayer
        {
            PlayerSide,
            OpponentSide,
            BothSide,
        }
        public enum Effect // tipo de efeito
        {
            AddPower,
            RemovePower,
            AddCard,
            DiscardCard,
            DestroyCard,
            DrawCard, // comprar uma carta
            NegateAttack,
            DownDigimon,
            Freeze, // congelar um Digimon, impedindo virar na faze de virar
            ChangePower, // Muda o poder do Digimon
            SearchCard, // Busca uma carta específica
            PlayToField, // Joga a carta no campo
            RevealTopDeck, // Revela o topo do deck
            CancelEffect, // Cancela o efeito de uma carta
            ColorfulEffect, // Efeito que pode ser usado por qualquer requisito de cor
            Cache,
            ReturnTo,
            ChangeColor, // Muda a cor do Digimon
            AddEffect,
        }
        public enum DurationEffect
        { 
            Condition, // Efeito permanente enquanto seguir a condição.
            EndOfTurn, // Efeito até o final do turno
            EndOfOpponentTurn, // Efeito até o próximo turno
            YourTurn, // Efeito até o final do seu turno
            OpponentTurn, // Efeito até o final do turno do oponente
        }
        public enum ValueVar
        {
            Less,
            More,
            Equal,
            ForeEach,
            MoreThanOpponent,
            DifferentColors,
        }
        public enum Restriction
        {
            OncePerTurn, // Apenas uma vez por turno
            TwicePerTurn, // Apenas duas vezes por turno
            OncePerGame, // Apenas uma vez por jogo
            EndTurn, // noButton final do turno
            ChooseOneEffect,
        }

        public static string[] EffectTypePrompt(string effectPrompt)
        {
            return string.IsNullOrEmpty(effectPrompt)
                ? Array.Empty<string>()
                : effectPrompt.Split(
                    new[] { "\r\n", "\n" },
                    StringSplitOptions.RemoveEmptyEntries
                );
        }
    }
}