using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.Forms.MessageBox;

namespace InitSetting
{
    public class PluginToggle
    {
        public PluginToggle(string codeId, string displayName, string pluginDllWithoutExtension, Action<bool> enabledChangedAction, bool isIpa)
        {
            CodeId = codeId;
            DisplayName = displayName;
            PluginDllWithoutExtension = pluginDllWithoutExtension;
            EnabledChangedAction = enabledChangedAction;
            IsIPA = isIpa;
        }

        public string CodeId { get; }
        public string DisplayName { get; }
        public string PluginDllWithoutExtension { get; }
        public Action<bool> EnabledChangedAction { get; }
        public bool IsIPA { get; }
    }

    public static class PluginToggleManager
    {
        private static readonly List<PluginToggle> _toggleList = new List<PluginToggle>
        {
            new PluginToggle("AI_Graphics", Localizable.ToggleAiGraphics, "AI_Graphics", null, false),
            new PluginToggle("DHH", Localizable.ToggleDhh, "DHH_AI4", null, false),
            new PluginToggle("DHHPH", Localizable.ToggleDhh, "ProjectHighHeel", null, true),
            new PluginToggle("GGmod", Localizable.ToggleGGmod, "GgmodForHS", null, true),
            new PluginToggle("GGmodstudio", Localizable.ToggleGGmodstudio, "GgmodForHS_Studio", null, true),
            new PluginToggle("GGmodneo", Localizable.ToggleGGmodneo, "GgmodForHS_NEO", null, true),
            new PluginToggle("AIHS2Graphics", Localizable.ToggleGraphicsMod, "Graphics", null, false),
            new PluginToggle("HoneyPot", Localizable.ToggleHoneyPot, "HoneyPot", null, true),
            new PluginToggle("RimRemover", Localizable.ToggleRimRemover, "RimRemover", null, false),
            new PluginToggle("ShortcutPlugin", Localizable.ToggleShortcutHS, "ShortcutHSParty", null, true),
            new PluginToggle("Stiletto", Localizable.ToggleStiletto, "Stiletto", null, false),
            new PluginToggle("VRMod", Localizable.ToggleVRMod, "PlayHomeVR", delegate (bool b)
            {
                if (b)
                    MessageBox.Show("To use this mod, open SteamVR before opening either the main game or studio.", "Usage");
            }, true)
        };

        public static void AddToggle(PluginToggle tgl) => _toggleList.Add(tgl);

        public static void CreatePluginToggles(StackPanel togglePanel)
        {
            // Add developer mode toggle ------------------------------------
            var devEnabled = EnvironmentHelper.DeveloperModeEnabled;
            if(devEnabled.HasValue)
            {
                var toggleConsole = new CheckBox
                {
                    Name = "toggleConsole",
                    Content = Localizable.ToggleConsole,
                    Foreground = Brushes.White,
                    IsChecked = devEnabled.Value
                };

                switch (EnvironmentHelper.DeveloperModeEnabled)
                {
                    case null:
                        toggleConsole.IsEnabled = false;
                        toggleConsole.Visibility = Visibility.Collapsed;
                        break;
                    case false:
                        toggleConsole.IsChecked = false;
                        break;
                    case true:
                        toggleConsole.IsChecked = true;
                        break;
                }
                toggleConsole.Checked += (sender, args) => EnvironmentHelper.DeveloperModeEnabled = true;
                toggleConsole.Unchecked += (sender, args) => EnvironmentHelper.DeveloperModeEnabled = false;
                togglePanel.Children.Add(toggleConsole);
            }

            // Add toggles from the list ------------------------------------
            foreach (var c in _toggleList)
            {
                var rootDir = c.IsIPA ? EnvironmentHelper.IPAPluginsDir : EnvironmentHelper.BepinPluginsDir;

                if (!Directory.Exists(rootDir))
                    continue;

                var pluginFile = Directory.GetFiles(rootDir, c.PluginDllWithoutExtension + ".dl*", SearchOption.AllDirectories).FirstOrDefault();

                if (pluginFile == null)
                    continue;

                var f = new FileInfo(pluginFile);
                var name = pluginFile.Substring(0, f.FullName.Length - f.Extension.Length + 1);

                var toggle = new CheckBox
                {
                    Name = c.CodeId,
                    Content = c.DisplayName,
                    Foreground = Brushes.White,
                    IsChecked = f.Extension == ".dll"
                };
                toggle.Checked += (sender, args) =>
                {
                    c.EnabledChangedAction?.Invoke(true);
                    f.MoveTo(Path.Combine(f.FullName, name + ".dll"));
                };
                toggle.Unchecked += (sender, args) =>
                {
                    c.EnabledChangedAction?.Invoke(false);
                    f.MoveTo(Path.Combine(f.FullName, name + ".dl_"));
                };

                togglePanel.Children.Add(toggle);
            }
        }
    }
}