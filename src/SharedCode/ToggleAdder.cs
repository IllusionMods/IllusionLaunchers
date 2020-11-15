using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        internal CheckBox _toggle;
        public void SetIsChecked(bool? newValue)
        {
            if (_toggle != null)
                _toggle.IsChecked = newValue;
        }
    }

    public static class PluginToggleManager
    {
        private static readonly List<PluginToggle> _toggleList;

        static PluginToggleManager()
        {
            PluginToggle aighs2, aig, aig2, hs2, dhh = null;
            aig = new PluginToggle("AI_Graphics", Localizable.ToggleAiGraphics, "AI_Graphics", delegate (bool b)
            {
                if (b)
                {
                    dhh.SetIsChecked(false);
                    MessageBox.Show("To use this mod, Press F5 during the game.", "Usage");
                }
            }, false);
            aig2 = new PluginToggle("AIGraphics", Localizable.ToggleGraphicsMod, "AIGraphics", delegate (bool b)
            {
                if (b)
                {
                    dhh.SetIsChecked(false);
                    MessageBox.Show("To use this mod, Press F5 during the game.", "Usage");
                }
            }, false);
            hs2 = new PluginToggle("HS2Graphics", Localizable.ToggleGraphicsMod, "HS2Graphics", delegate (bool b)
            {
                if (b)
                {
                    dhh.SetIsChecked(false);
                    MessageBox.Show("To use this mod, Press F5 during the game.", "Usage");
                }
            }, false);
            aighs2 = new PluginToggle("Graphics", Localizable.ToggleGraphicsMod, "Graphics", delegate (bool b)
            {
                if (b)
                {
                    dhh.SetIsChecked(false);
                    MessageBox.Show("To use this mod, Press F5 during the game.", "Usage");
                }
            }, false);
            dhh = new PluginToggle("DHH", Localizable.ToggleDhh, "DHH_AI4", delegate (bool b)
            {
                if (b)
                {
                    aig.SetIsChecked(false);
                    aig2.SetIsChecked(false);
                    hs2.SetIsChecked(false);
                    aighs2.SetIsChecked(false);
                    MessageBox.Show("To use this mod, Press P during the game.", "Usage");
                }
            }, false);

            _toggleList = new List<PluginToggle>
            {
                aig,
                aig2,
                hs2,
                aighs2,
                dhh,
                new PluginToggle("DHHPH", Localizable.ToggleDhh, "ProjectHighHeel", null, true),
                new PluginToggle("GGmod", Localizable.ToggleGGmod, "GgmodForHS", null, true),
                new PluginToggle("GGmodstudio", Localizable.ToggleGGmodstudio, "GgmodForHS_Studio", null, true),
                new PluginToggle("GGmodneo", Localizable.ToggleGGmodneo, "GgmodForHS_NEO", null, true),
                new PluginToggle("HoneyPot", Localizable.ToggleHoneyPot, "HoneyPot", delegate (bool b)
                {
                    if (b)
                        MessageBox.Show("When HoneyPot is enabled, the game will use a bit longer to load in some scenes due to checking for HoneySelect assets, making it appear to be freezing for a few seconds. This is completely normal.\n\nJust disable this option again if you would rather not have that freeze.", "Usage");
                }, true),
                new PluginToggle("PHIBL", Localizable.TogglePHIBL, "PHIBL", delegate (bool b)
                {
                    if (b)
                    {
                        MessageBox.Show("To use this mod, Press F5 during the game.", "Usage");
                        DisableHelper("PH_PHIBL_PresetLoad_Nyaacho",true,false);
                        DisableHelper("PH_PHIBL_PresetLoad_Original",true,false);
                    }
                    else
                    {
                        DisableHelper("PH_PHIBL_PresetLoad_Nyaacho",true,true);
                        DisableHelper("PH_PHIBL_PresetLoad_Original",true,true);
                    }
                }, true),
                new PluginToggle("RimRemover", Localizable.ToggleRimRemover, "RimRemover", null, false),
                new PluginToggle("ShortcutPlugin", Localizable.ToggleShortcutHS, "ShortcutHSParty", null, true),
                new PluginToggle("Stiletto", Localizable.ToggleStiletto, "Stiletto", null, false),
                new PluginToggle("VRMod", Localizable.ToggleVRMod, "PlayHomeVR", delegate (bool b)
                {
                    if (b)
                        MessageBox.Show("To use this mod, open SteamVR before opening either the main game or studio.", "Usage");
                }, true),
                new PluginToggle("BodyChange", "Enable BodyChanger", "MdgqBodyChange", delegate (bool b)
                {
                    if (b)
                        DisableHelper("_AssemblyLoader",false,false);
                    else
                        DisableHelper("_AssemblyLoader",false,true);
                }, false)
            };
        }

        private static void DisableHelper(string DllName, bool isIPA, bool disable)
        {
            string folder;
            switch (isIPA)
            {
                case true:
                    folder = EnvironmentHelper.GameRootDirectory + "Plugins\\";
                    break;
                default:
                    folder = EnvironmentHelper.BepinPluginsDir;
                    break;
            }

            switch (disable)
            {
                case true:
                    if (File.Exists(folder + DllName + ".dll"))
                        File.Move(folder + DllName + ".dll", folder + DllName + ".dl_");
                    break;
                case false:
                    if (File.Exists(folder + DllName + ".dl_"))
                        File.Move(folder + DllName + ".dl_", folder + DllName + ".dll");
                    break;
            }
        }

        public static void AddToggle(PluginToggle tgl) => _toggleList.Add(tgl);

        public static void CreatePluginToggles(StackPanel togglePanel)
        {
            // Add developer mode toggle ------------------------------------
            if (EnvironmentHelper.IsBepIn)
            {
                var toggleConsole = new CheckBox
                {
                    Name = "toggleConsole",
                    Content = Localizable.ToggleConsole,
                    Foreground = Brushes.White,
                    IsChecked = EnvironmentHelper.DeveloperModeEnabled
                };
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

                c._toggle = toggle;

                togglePanel.Children.Add(toggle);
            }
        }
    }
}