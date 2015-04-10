using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POESKillTree.Views;

namespace POESKillTree.SkillTreeFiles
{
    public class SkilltreeUndoState
    {
        public int characterIndex { get; set; }
        public string level { get; set; }
        public string skillTreeURL { get; set; }
        
        public void getCurrentState(MainWindow window)
        {
            characterIndex = window.cbCharType.SelectedIndex;
            level = window.tbLevel.Text;
            skillTreeURL = window.tbSkillURL.Text;
        }
        public void setCurrentState(MainWindow window)
        {
            window.cbCharType.SelectedIndex = characterIndex;
            window.tbLevel.Text = level;
            window.tbSkillURL.Text = skillTreeURL;
        }
        public override string ToString()
        {
            return characterIndex + " : " + level + " : " + skillTreeURL;
        }
    }
}
