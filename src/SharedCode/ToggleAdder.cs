using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace InitSetting
{
    public class Toggleables
    {
        public Toggleables(string _CodeName, string _ModName, string _PluginDLL, Action<bool> _OnAction, bool _IsIPA)
        {
            CodeName = _CodeName;
            ModName = _ModName;
            PluginDLL = _PluginDLL;
            OnAction = _OnAction;
            IsIPA = _IsIPA;
        }

        public string CodeName { get; }
        public string ModName { get; }
        public string PluginDLL { get; }
        public Action<bool> OnAction { get; }
        public bool IsIPA { get; }
    }

    public static class ToggleablesMan
    {
        public static List<Toggleables> Modlist = new List<Toggleables>();

        public static void SetupModList()
        {
            Modlist.Add(new Toggleables("AI_Graphics", Localizable.ToggleAiGraphics, "AI_Graphics", null, false));
            Modlist.Add(new Toggleables("DHH", Localizable.ToggleDhh, "DHH_AI4", null, false));
            Modlist.Add(new Toggleables("DHHPH", Localizable.ToggleDhh, "ProjectHighHeel", null, true));
            Modlist.Add(new Toggleables("HoneyPot", Localizable.ToggleHoneyPot, "HoneyPot", null, true));
            Modlist.Add(new Toggleables("RimRemover", Localizable.ToggleRimRemover, "RimRemover", null, false));
            Modlist.Add(new Toggleables("Stiletto", Localizable.ToggleStiletto, "Stiletto", null, false));
            Modlist.Add(new Toggleables("VRMod", Localizable.ToggleVRMod, "PlayHomeVR", delegate(bool b)
            {
                if (b)
                    MessageBox.Show("To use this mod, open SteamVR before opening either the main game or studio.", "Usage");
            }, true));

            /*
            Modlist.Add(new Toggleables("Test", "test test", "ProjectHighHeel", delegate (bool b)
            {
                MessageBox.Show(b ? "selected" : "Unselected");
            }, true));
            */
        }
    }
}