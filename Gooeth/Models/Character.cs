using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gooeth.Models
{
    public class Character
    {
        public string Name { get; set; }
        public string Race { get; set; }
        public string Class { get; set; }
        public string Hometown { get; set; }
        public string Weapon { get; set; }
        public string Item { get; set; }
        public int Level { get; set; }
        public int HP { get; set; }        
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Wisdom { get; set; }
        public int Intellect { get; set; }
        public int Charisma { get; set; }        
    }
}