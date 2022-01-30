using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace InitSetting
{
    /// <summary>
    /// Interaction logic for VRSelectionWindow.xaml
    /// </summary>
    public partial class VRSelectionWindow : Window
    {
        public VRSelectionWindow()
        {
            InitializeComponent();
        }

        private static readonly List<KeyValuePair<string, Action>> _buttons = new List<KeyValuePair<string, Action>>();

        public static int AvailableVrModes => _buttons.Count;

        public static void TryAddVrButton(string displayName, Func<bool> isAvailable, Action run)
        {
            try
            {
                if (isAvailable())
                {
                    _buttons.Add(new KeyValuePair<string, Action>(displayName, run));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static bool ShowDialogOrStartVr(MainWindow owner)
        {
            if (AvailableVrModes == 0)
            {
                System.Windows.Forms.MessageBox.Show(
                    "There are no VR modules or plugins detected. Install official VR module or a supported plugin and try again.",
                    "Start VR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (AvailableVrModes == 1)
            {
                return TryStart(_buttons.Single().Value);
            }

            var win = new VRSelectionWindow();
            win.Owner = owner;
            return win.ShowDialog() ?? false;
        }

        public override void EndInit()
        {
            foreach (var button in _buttons)
            {
                var but = new Button();
                but.Content = button.Key.Replace("_", "__");
                but.Margin = new Thickness(5);
                but.Click += (sender, args) => { StartAndClose(button.Value); };
                ButtonPanel.Children.Add(but);
            }

            var butc = new Button();
            butc.Content = "Cancel";
            butc.Margin = new Thickness(5, 15, 5, 5);
            butc.Click += (sender, args) => { DialogResult = false; Close(); };
            ButtonPanel.Children.Add(butc);

            base.EndInit();
        }

        private static bool TryStart(Action button)
        {
            try
            {
                button();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to start VR mode. Make sure that game files are accessible and try starting the module manually. If this is an issue with the launcher, please report it.\n\n" + e, "Start VR", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool StartAndClose(Action button)
        {
            if (TryStart(button))
            {
                if (IsVisible)
                    DialogResult = true;
                Close();
                return true;
            }
            else
            {
                if (IsVisible)
                    DialogResult = false;
                return false;
            }
        }
    }
}
