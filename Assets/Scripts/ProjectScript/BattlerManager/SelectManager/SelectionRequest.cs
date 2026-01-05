using ProjectScript.Interfaces;
using System;
using System.Collections.Generic;

namespace ProjectScript.Selection
{
    public class SelectionRequest
    {
        // preciso colocar os criterios aqui de selecao da carta
        public int amount;
        public Action<List<ISelectable>> onComplete;

        public SelectionRequest(int amount, Action<List<ISelectable>> onComplete)
        {
            this.amount = amount;
            this.onComplete = onComplete;
        }
    }
}
