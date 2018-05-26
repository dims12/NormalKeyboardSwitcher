using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Win32;
using System.Reflection;
using System.ComponentModel;

namespace NormalKeyboardSwitcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KeyboardListener keyboardListener;
        private ForegroundWindowListener foregroundWindowListener;
        private InputController inputController;
        private RegistryKey autorunKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        private Assembly curAssembly = Assembly.GetExecutingAssembly();


        public MainWindow()
        {
            InitializeComponent();

            inputController = new InputController();
            keyboardListener = new KeyboardListener(FirstKey.Control);
            foregroundWindowListener = new ForegroundWindowListener();

            // when languange change key combinations are pressed then call appropriate methods of input controller
            keyboardListener.NextTemporaryInputLanguage += inputController.NextTemporaryInputLanguage;
            keyboardListener.SwitchToTemporaryInputLanguage += inputController.SwitchToTemporaryInputLanguage;
            keyboardListener.DropTemporaryInputLanguage += inputController.DropTemporaryInputLanguage;

            // if input language is changed in input controller, then send it to foreground window
            inputController.InputLanguageChanged += foregroundWindowListener.InputLangChangeRequest;

            // if foreground window changed, then send to it a request to change to current language from the controller
            foregroundWindowListener.ForegroundWindowChanged += (hwnd) => { foregroundWindowListener.InputLangChangeRequest(hwnd, inputController.UsedInputLanguage); };

            // binds list of system languages to a list box in GUI
            LanguagesListBox.ItemsSource = inputController.UsedInputLanguages;

            Binding TemporaryInputLanguageBinding = new Binding("TemporaryInputLanguage");
            TemporaryInputLanguageBinding.Mode = BindingMode.OneWay;
            TemporaryInputLanguageBinding.Source = inputController;
            LanguagesListBox.SetBinding(ListBox.SelectedIndexProperty, TemporaryInputLanguageBinding);


            // settin initial value of autorun checkbox
            AutorunCheckBox.IsChecked = Autorun;


        }

        bool Autorun
        {
            get
            {
                object oldAutorunValue = autorunKey.GetValue(curAssembly.GetName().Name);
                return oldAutorunValue != null && oldAutorunValue.Equals(curAssembly.Location);
            }

            set
            {
                if (value && !Autorun)
                {
                    autorunKey.SetValue(curAssembly.GetName().Name, curAssembly.Location);
                }
                else if( !value && Autorun )
                {
                    autorunKey.DeleteValue(curAssembly.GetName().Name, false);
                }
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                Hide();

            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?\nNormal Keyboard Switcher won't switch keyboards anymore.\nIf you want to hide it's window then select 'No' here and minimize it's window", 
                "Exit Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult != MessageBoxResult.Yes) {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }



        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            if(LanguagesListBox.SelectedIndex>=0)
            {
                inputController.SwitchToLanguage(LanguagesListBox.SelectedIndex);
            }
        }

        private void AutorunCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (AutorunCheckBox.IsChecked != null)
            {
                Autorun = (bool)AutorunCheckBox.IsChecked;
            }

        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
