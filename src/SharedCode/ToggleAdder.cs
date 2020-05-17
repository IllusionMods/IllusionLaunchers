using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace InitSetting
{
    public class Toggleables
    {
        public string CodeName { get; private set; }
        public string ModName { get; private set; }
        public string PluginDLL { get; private set; } 
        public Action<bool> OnAction { get; private set; }
        public bool IsIPA { get; private set; }

        public Toggleables(string _CodeName, string _ModName, string _PluginDLL, Action<bool> _OnAction, bool _IsIPA)
        {
            CodeName = _CodeName;
            ModName = _ModName;
            PluginDLL = _PluginDLL;
            OnAction = _OnAction;
            IsIPA = _IsIPA;
        }
    }

    public static class ToggleablesMan
    {
        public static List<Toggleables> Modlist = new List<Toggleables>();

        public static void SetupModList()
        {
            Modlist.Add(new Toggleables("AI_Graphics", "AI Graphics", "AI_Graphics", null, false));
            Modlist.Add(new Toggleables("DHH", "DHH", "DHH_AI4", null, false));
            
            Modlist.Add(new Toggleables("Test", "test test", "ProjectHighHeel", delegate (bool b)
            {
                MessageBox.Show(b ? "selected" : "Unselected");
            }, true));
        }
    }
}