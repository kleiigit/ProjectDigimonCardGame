
using SinuousProductions;
using UnityEngine;

namespace ProjectScript.Interfaces
{
    public interface IPile
    {
        void AddCard(Card cardData);
        void RemoveCard(GameObject cardObject);
        void UpdateVisuals();
    }
}
