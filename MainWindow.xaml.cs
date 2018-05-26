﻿using System;
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
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                if ((bool)AutorunCheckBox.IsChecked)
                {
                    key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
                }
                else
                {
                    key.DeleteValue(curAssembly.GetName().Name, false);
                }
            }

        }
    }
}
