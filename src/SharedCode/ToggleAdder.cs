using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.Forms.MessageBox;

namespace InitSetting
{
    public class PluginToggle
    {
        public PluginToggle(string codeId, string displayName, string pluginToolTip, string pluginDllWithoutExtension, Action<bool> enabledChangedAction, bool isIpa, bool isPatcher)
        {
            CodeId = codeId;
            DisplayName = displayName;
            PluginToolTip = pluginToolTip;
            PluginDllWithoutExtension = pluginDllWithoutExtension;
            EnabledChangedAction = enabledChangedAction;
            IsIPA = isIpa;
            IsPatcher = isPatcher;
        }

        public string CodeId { get; }
        public string DisplayName { get; }
        public string PluginToolTip { get; }
        public string PluginDllWithoutExtension { get; }
        public Action<bool> EnabledChangedAction { get; }
        public bool IsIPA { get; }
        public bool IsPatcher { get; }

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
                    MessageBox.Show(Localizable.MessageBoxGraphicsMod, "Usage");
                }
            }, false, false);
            aig2 = new PluginToggle("AIGraphics", Localizable.ToggleGraphicsMod, Localizable.TooltipGraphicsMod, "AIGraphics", delegate (bool b)
            {
                if (b)
                {
                    dhh.SetIsChecked(false);
                    MessageBox.Show(Localizable.MessageBoxGraphicsMod, "Usage");
                }
            }, false, false);
            hs2 = new PluginToggle("HS2Graphics", Localizable.ToggleGraphicsMod, Localizable.TooltipGraphicsMod, "HS2Graphics", delegate (bool b)
            {
                if (b)
                {
                    dhh.SetIsChecked(false);
                    MessageBox.Show(Localizable.MessageBoxGraphicsMod, "Usage");
                }
            }, false, false);
            aighs2 = new PluginToggle("Graphics", Localizable.ToggleGraphicsMod, Localizable.TooltipGraphicsMod, "Graphics", delegate (bool b)
            {
                if (b)
                {
                    dhh.SetIsChecked(false);
                    MessageBox.Show(Localizable.MessageBoxGraphicsMod, "Usage");
                }
            }, false, false);
            dhh = new PluginToggle("DHH", Localizable.ToggleDhh, Localizable.TooltipDhh, "DHH_AI4", delegate (bool b)
            {
                if (b)
                {
                    aig.SetIsChecked(false);
                    aig2.SetIsChecked(false);
                    hs2.SetIsChecked(false);
                    aighs2.SetIsChecked(false);
                    MessageBox.Show(Localizable.MessageBoxDHH, "Usage");
                }
            }, false, false);

            _toggleList = new List<PluginToggle>
            {
                aig,
                aig2,
                hs2,
                aighs2,
                dhh,
                new PluginToggle("SplashScreen", "Enable SplashScreen", "", "BepInEx.SplashScreen.Patcher.BepInEx5", delegate (bool b)
                {
                    if (b)
                    {
                        DisableHelper("BepInEx.SplashScreen.Patcher",false,true,false);
                        DisableHelper("BepInEx.SplashScreen.Patcher.BepInEx5",false,true,false);
                    }
                    else
                    {
                        DisableHelper("BepInEx.SplashScreen.Patcher",false,true,true);
                        DisableHelper("BepInEx.SplashScreen.Patcher.BepInEx5",false,true,true);
                    }
                }, true, true),
                new PluginToggle("OfflineMode", "Enable Offline Mode", "Disallows online connectivity, allowing the game to be played offline", "WebRequestBlocker", null, false, false),
                new PluginToggle("DHHPH", Localizable.ToggleDhh, Localizable.TooltipDhhPH, "ProjectHighHeel", null, true, false),
                new PluginToggle("GgmodForPlayClub", Localizable.ToggleGGmod, Localizable.TooltipGGmod, "GgmodForPlayClub", null, true, false),
                new PluginToggle("GgmodForPlayClubStudio", Localizable.ToggleGGmodstudioPC, Localizable.TooltipGGmod, "GgmodForPlayClubStudio", null, true, false),
                new PluginToggle("TouchyFeely", Localizable.ToggleTouchyFeely, Localizable.TooltipTouchyFeely, "TouchyFeely", null, true, false),
                new PluginToggle("GGmod", Localizable.ToggleGGmod, Localizable.TooltipGGmod, "GgmodForHS", null, true, false),
                new PluginToggle("GGmodstudio", Localizable.ToggleGGmodstudio, Localizable.TooltipGGmod, "GgmodForHS_Studio", null, true, false),
                new PluginToggle("GGmodneo", Localizable.ToggleGGmodneo, Localizable.TooltipGGmod, "GgmodForHS_NEO", null, true, false),
                new PluginToggle("HoneyPot", Localizable.ToggleHoneyPot, Localizable.TooltipHoneyPot, "HoneyPot", delegate (bool b)
                {
                    if (b)
                        MessageBox.Show(Localizable.WarningHoneyPot, Localizable.TypeUsage);
                }, true, false),
                new PluginToggle("PHIBL", Localizable.TogglePHIBL, Localizable.TooltipPHIBL, "PHIBL", delegate (bool b)
                {
                    if (b)
                    {
                        MessageBox.Show(Localizable.TooltipGraphicsMod, Localizable.TypeUsage);
                        DisableHelper("PH_PHIBL_PresetLoad_Nyaacho",true,false,true);
                        DisableHelper("PH_PHIBL_PresetLoad_Original",true,false,true);
                    }
                    else
                    {
                        DisableHelper("PH_PHIBL_PresetLoad_Nyaacho",true,false,true);
                        DisableHelper("PH_PHIBL_PresetLoad_Original",true,false,true);
                    }
                }, true, false),
                new PluginToggle("RimRemover", Localizable.ToggleRimRemover, "", "*RimRemover", null, false, false),
                new PluginToggle("AutoSave", Localizable.ActivateAutosave, "", "*AutoSave", null, false, false),
                new PluginToggle("ShortcutPlugin", Localizable.ToggleShortcutHS, "", "ShortcutHSParty", null, true, false),
                new PluginToggle("BetterAA", Localizable.BetterAA, "", "*_BetterAA", null, false, false),
                new PluginToggle("PovX", "Activate PovX", "", "*PovX", null, false, false),
                new PluginToggle("PostProcessingEffects", "Activate PostProcessingEffects", "", "PostProcessingEffect", delegate (bool b)
                {
                    if (b)
                    {
                        MessageBox.Show(Localizable.WarningPostPros, Localizable.TypeWarn);
                        DisableHelper("PostProcessingRuntime",false,false,false);
                    }
                    else
                    {
                        DisableHelper("PostProcessingRuntime",false,false,true);
                    }
                }, false, false),
                new PluginToggle("Stiletto", Localizable.ToggleStiletto, Localizable.TooltipGGmod, "*Stiletto", null, false, false),
                new PluginToggle("VRMod", Localizable.ToggleVRMod, Localizable.TooltipVRMod, "PlayHomeVR", delegate (bool b)
                {
                    if (b)
                        MessageBox.Show(Localizable.WarningPHVR, Localizable.TypeUsage);
                }, true, false),
                new PluginToggle("PCVRMod", Localizable.ToggleVRMod, Localizable.TooltipVRMod, "PlayClubVR", delegate (bool b)
                {
                    if (b)
                    {
                        DisableHelper("LeapCSharp.NET3.5",true,false,false);
                        DisableHelper("GamePadClub",true,false,false);
                        DisableHelper("PlayClubStudioVR",true,false,false);
                        DisableHelper("SpeechTransport",true,false,false);
                        DisableHelper("VRGIN.U46",true,false,false);
                        DisableHelper("WindowsInput",true,false,false);
                        DisableHelper("XInputDotNetPure",true,false,false);
                    }
                    else
                    {
                        DisableHelper("LeapCSharp.NET3.5",true,false,true);
                        DisableHelper("GamePadClub",true,false,true);
                        DisableHelper("PlayClubStudioVR",true,false,true);
                        DisableHelper("SpeechTransport",true,false,true);
                        DisableHelper("VRGIN.U46",true,false,true);
                        DisableHelper("WindowsInput",true,false,true);
                        DisableHelper("XInputDotNetPure",true,false,true);
                    }
                }, true, false),
            };
        }

        private static void DisableHelper(string DllName, bool isIPA, bool isPatcher, bool disable)
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

            if (isPatcher)
            {
                folder = EnvironmentHelper.GameRootDirectory + "BepInEx\\patchers";
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
                toggleConsole.ToolTip = Localizable.TooltipConsole;
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
            toggleExperimental.ToolTip = Localizable.TooltipBleeding;
            togglePanel.Children.Add(toggleExperimental);

            

            // Add toggles from the list ------------------------------------
            foreach (var c in _toggleList)
            {
                var rootDir = c.IsIPA ? EnvironmentHelper.IPAPluginsDir : EnvironmentHelper.BepinPluginsDir;
                rootDir = c.IsPatcher ? EnvironmentHelper.GameRootDirectory + "BepInEx\\patchers" : EnvironmentHelper.BepinPluginsDir;

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
                    f.MoveTo(Path.Combine(f.FullName, name + ".dl_").Replace("..","."));
                };

                if(c.PluginToolTip != "")
                    toggle.ToolTip = tooltip;

                c._toggle = toggle;

                togglePanel.Children.Add(toggle);
            }
        }
    }
}