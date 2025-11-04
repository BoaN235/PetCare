using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetCare_BE
{
    public class PetGameState
    {
        public int week = 1;
        public bool SaveGame()
        {
            return true;
        }

        public bool LoadGame()
        {
            week = 1;
            return true;
        }

        public bool NewGame()
        {
            return true;
        }

        public bool ExitGame()
        {
            return true;
        }
    }
}
