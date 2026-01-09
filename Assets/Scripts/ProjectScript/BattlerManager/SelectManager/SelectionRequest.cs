using ProjectScript.Interfaces;
using System;
using System.Collections.Generic;

namespace ProjectScript.Selection
{
    public class SelectionRequest
    {
        // preciso colocar os criterios aqui de selecao da carta
        public int amount;
        public SelectionCriteria criteria;
        public Action<List<ISelectable>> onComplete;

        public SelectionRequest(int amount, SelectionCriteria criteria, Action<List<ISelectable>> onComplete)
        {
            this.amount = amount;
            this.criteria = criteria;
            this.onComplete = onComplete;
        }
    }
}
