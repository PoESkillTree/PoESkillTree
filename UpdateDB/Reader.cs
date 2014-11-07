using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gem = POESKillTree.SkillTreeFiles.ItemDB.Gem;

namespace UpdateDB
{
    // The base class of item data reader.
    public abstract class Reader
    {
        // Returns gem data.
        abstract public Gem FetchGem(string name);
        // Prints normal message.
        protected void Info(string message) { Program.Info(message); }
        // Prints verbose message.
        protected void Verbose(string message) { Program.Verbose(message); }
        // Prints warning message.
        protected void Warning(string message) { Program.Warning(message); }
    }
}
