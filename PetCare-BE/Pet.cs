

namespace PetCare_BE
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading.Tasks;
    using Microsoft.Maui.Controls;
    internal class Pet
    {
        private string name;
        private int age;
        private double health;
        private double happiness;
        private double hunger;
        private string[] state;
        private string type;

        public Pet(string name,string type)
        {
            age = 0;
            health = 100.0;
            happiness = 100.0;
            hunger = 100.0;

        }
    }
}