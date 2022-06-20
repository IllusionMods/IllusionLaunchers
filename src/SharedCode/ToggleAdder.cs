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
        public PluginToggle(string codeId, string displayName, string pluginToolTip, string pluginDllWithoutExtension, Action<bool> enabledChangedAction, bool isIpa)
        {
            CodeId = codeId;
            DisplayName = displayName;
            PluginToolTip = pluginToolTip;
            PluginDllWithoutExtension = pluginDllWithoutExtension;
            EnabledChangedAction = enabledChangedAction;
            IsIPA = isIpa;
        }

        public string CodeId { get; }
        public string DisplayName { get; }
        public string PluginToolTip { get; }
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
            aig = new PluginToggle("AI_Graphics", Localizable.ToggleAiGraphics, Localizable.TooltipGraphicsMod, "AI_Graphics", delegate (bool b)
            {
                if (b)
                {
                    dhh.SetIsChecked(false);
                    MessageBox.Show("To use this mod, Press F5 during the game.", "Usage");
                }
            }, false);
            aig2 = new PluginToggle("AIGraphics", Localizable.ToggleGraphicsMod, Localizable.TooltipGraphicsMod, "AIGraphics", delegate (bool b)
            {
                if (b)
                {
                    dhh.SetIsChecked(false);
                    MessageBox.Show("To use this mod, Press F5 during the game.", "Usage");
                }
            }, false);
            hs2 = new PluginToggle("HS2Graphics", Localizable.ToggleGraphicsMod, Localizable.TooltipGraphicsMod, "HS2Graphics", delegate (bool b)
            {
                if (b)
                {
                    dhh.SetIsChecked(false);
                    MessageBox.Show("To use this mod, Press F5 during the game.", "Usage");
                }
            }, false);
            aighs2 = new PluginToggle("Graphics", Localizable.ToggleGraphicsMod, Localizable.TooltipGraphicsMod, "Graphics", delegate (bool b)
            {
                if (b)
                {
                    dhh.SetIsChecked(false);
                    MessageBox.Show("To use this mod, Press F5 during the game.", "Usage");
                }
            }, false);
            dhh = new PluginToggle("DHH", Localizable.ToggleDhh, Localizable.TooltipDhh, "DHH_AI4", delegate (bool b)
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
                new PluginToggle("DHHPH", Localizable.ToggleDhh, Localizable.TooltipDhhPH, "ProjectHighHeel", null, true),
                new PluginToggle("GgmodForPlayClub", Localizable.ToggleGGmod, Localizable.TooltipGGmod, "GgmodForPlayClub", null, true),
                new PluginToggle("GgmodForPlayClubStudio", Localizable.ToggleGGmodstudioPC, Localizable.TooltipGGmod, "GgmodForPlayClubStudio", null, true),
                new PluginToggle("TouchyFeely", Localizable.ToggleTouchyFeely, Localizable.TooltipTouchyFeely, "TouchyFeely", null, true),
                new PluginToggle("GGmod", Localizable.ToggleGGmod, Localizable.TooltipGGmod, "GgmodForHS", null, true),
                new PluginToggle("GGmodstudio", Localizable.ToggleGGmodstudio, Localizable.TooltipGGmod, "GgmodForHS_Studio", null, true),
                new PluginToggle("GGmodneo", Localizable.ToggleGGmodneo, Localizable.TooltipGGmod, "GgmodForHS_NEO", null, true),
                new PluginToggle("HoneyPot", Localizable.ToggleHoneyPot, Localizable.TooltipHoneyPot, "HoneyPot", delegate (bool b)
                {
                    if (b)
                        MessageBox.Show("When HoneyPot is enabled, the game will use a bit longer to load in some scenes due to checking for HoneySelect assets, making it appear to be freezing for a few seconds. This is completely normal.\n\nJust disable this option again if you would rather not have that freeze.", "Usage");
                }, true),
                new PluginToggle("PHIBL", Localizable.TogglePHIBL, Localizable.TooltipPHIBL, "PHIBL", delegate (bool b)
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
                new PluginToggle("RimRemover", Localizable.ToggleRimRemover, "", "*RimRemover", null, false),
                new PluginToggle("AutoSave", Localizable.ActivateAutosave, "", "*AutoSave", null, false),
                new PluginToggle("ShortcutPlugin", Localizable.ToggleShortcutHS, "", "ShortcutHSParty", null, true),
                new PluginToggle("BetterAA", "Activate BetterAA", "", "*_BetterAA", null, false),
                new PluginToggle("PostProcessingEffects", "Activate PostProcessingEffects", "", "PostProcessingEffect", delegate (bool b)
                {
                    if (b)
                    {
                        MessageBox.Show("This mod is known to cause issues saving coordinates, please disable if you're experiencing problems.", "Warning");
                        DisableHelper("PostProcessingRuntime",false,false);
                    }
                    else
                    {
                        DisableHelper("PostProcessingRuntime",false,true);
                    }
                }, false),
                new PluginToggle("Stiletto", Localizable.ToggleStiletto, Localizable.TooltipGGmod, "*Stiletto", null, false),
                new PluginToggle("VRMod", Localizable.ToggleVRMod, Localizable.TooltipVRMod, "PlayHomeVR", delegate (bool b)
                {
                    if (b)
                        MessageBox.Show("To use this mod, open SteamVR before opening either the main game or studio.", "Usage");
                }, true),
                new PluginToggle("PCVRMod", Localizable.ToggleVRMod, Localizable.TooltipVRMod, "PlayClubVR", delegate (bool b)
                {
                    if (b)
                    {
                        DisableHelper("LeapCSharp.NET3.5",true,false);
                        DisableHelper("GamePadClub",true,false);
                        DisableHelper("PlayClubStudioVR",true,false);
                        DisableHelper("SpeechTransport",true,false);
                        DisableHelper("VRGIN.U46",true,false);
                        DisableHelper("WindowsInput",true,false);
                        DisableHelper("XInputDotNetPure",true,false);
                    }
                    else
                    {
                        DisableHelper("LeapCSharp.NET3.5",true,true);
                        DisableHelper("GamePadClub",true,true);
                        DisableHelper("PlayClubStudioVR",true,true);
                        DisableHelper("SpeechTransport",true,true);
                        DisableHelper("VRGIN.U46",true,true);
                        DisableHelper("WindowsInput",true,true);
                        DisableHelper("XInputDotNetPure",true,true);
                    }
                }, true),
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
            // Add sideloader maintaining mode toggle ----------------------
            if (EnvironmentHelper.SideloaderMaintainerMode)
            {
                var toggleSideloadMaintain = new CheckBox
                {
                    Name = "toggleSideloaderMaintain",
                    Content = "Sideloader Maintainer Mode",
                    Foreground = Brushes.White,
                    IsChecked = EnvironmentHelper.SideloaderMaintainerEnabled
                };
                toggleSideloadMaintain.Checked += (sender, args) => EnvironmentHelper.SideloaderMaintainerEnabled = true;
                toggleSideloadMaintain.Unchecked += (sender, args) => EnvironmentHelper.SideloaderMaintainerEnabled = false;
                toggleSideloadMaintain.ToolTip = "Enable test modpack.\nOnly meant for testing new modpack builds.";
                togglePanel.Children.Add(toggleSideloadMaintain);
            }

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
                toggleConsole.ToolTip = "Enable console to see game status console.";
                togglePanel.Children.Add(toggleConsole);
            }

            // Add experimental mode toggle ---------------------------------
            var toggleExperimental = new CheckBox
            {
                Name = "toggleExperimental",
                Content = Localizable.ExperimentalMode,
                Foreground = Brushes.White,
                IsChecked = EnvironmentHelper.BleedingModeEnabled
            };
            toggleExperimental.Checked += (sender, args) => EnvironmentHelper.BleedingModeEnabled = true;
            toggleExperimental.Unchecked += (sender, args) => EnvironmentHelper.BleedingModeEnabled = false;
            toggleExperimental.ToolTip = "Allow installation of experimental mods to this game.";
            togglePanel.Children.Add(toggleExperimental);

            

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

                // Doing a quick check to make sure there isn't both enabled and disabled variants in the folder
                var DupCheck = Path.Combine(f.FullName, name);
                if (File.Exists(DupCheck + ".dll") && File.Exists(DupCheck + ".dl_"))
                    File.Delete(DupCheck + ".dl_");

                var tooltip = new ToolTip
                {
                    Content = c.PluginToolTip

                };

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

                if(c.PluginToolTip != "")
                    toggle.ToolTip = tooltip;

                c._toggle = toggle;

                togglePanel.Children.Add(toggle);
            }
        }
    }
}