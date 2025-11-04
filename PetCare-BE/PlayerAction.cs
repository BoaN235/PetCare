using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetCare_BE
{
    public class PlayerAction
    {
        public string text;
        public int actionId;
        public int healthChange;
        public int happyChange;
        public int hungerChange;
        public int moneyChange;

        public PlayerAction(string inputtext, int actionid, int healthChange, int happyChange, int hungerChange, int moneyChange)
        {
            text = inputtext;

        }
    }
}
