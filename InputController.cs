using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NormalKeyboardSwitcher
{

    delegate void InputLanguageChangedDelegate(UsedInputLanguage usedInputLanguage);

    /// <summary>
    /// Wraps interaction with OS input language subsystem
    /// </summary>
    class InputController : INotifyPropertyChanged
    {

        private int temporaryInputLanguage = 0;
        private ObservableCollection<UsedInputLanguage> usedInputLanguages = new ObservableCollection<UsedInputLanguage>();

        public InputController()
        {
            PopulateUsedInputLanguages();
        }

        public void PopulateUsedInputLanguages()
        {
            usedInputLanguages.Clear();
            foreach (InputLanguage il in InputLanguage.InstalledInputLanguages)
            {
                if ((int)il.Handle == 0x04190419 && usedInputLanguages.Count()>=2)
                {
                    usedInputLanguages.Insert(1, new UsedInputLanguage(il));
                }
                else
                {
                    usedInputLanguages.Add(new UsedInputLanguage(il));
                }
                
            }
            TemporaryInputLanguage = 0;
        }

        public ObservableCollection<UsedInputLanguage> UsedInputLanguages
        {
            get
            {
                return usedInputLanguages;
            }
        }

        public int TemporaryInputLanguage
        {
            get
            {
                if (temporaryInputLanguage >= 0 && temporaryInputLanguage < UsedInputLanguages.Count)
                {
                    return temporaryInputLanguage;
                }
                else {
                    return 0;
                }
            
            }
            set
            {
                temporaryInputLanguage = value % UsedInputLanguages.Count;
                NotifyPropertyChanged("TemporaryInputLanguage");
                NotifyPropertyChanged("UsedInputLanguage");
                FireInputLanguageChanged();
            }
        }

        public UsedInputLanguage UsedInputLanguage
        {
            get
            {
                return UsedInputLanguages[TemporaryInputLanguage];
            }
        }

        public UsedInputLanguage GetInputLanguage(IntPtr kbd) {
            foreach(UsedInputLanguage inputLanguage in UsedInputLanguages) {
                if( inputLanguage.InputLanguage.Handle == kbd) {
                    return inputLanguage;
                }
            }
            return null;
        }

        public int NextTemporaryInputLanguage()
        {
            TemporaryInputLanguage++;
            return TemporaryInputLanguage;
        }

        public void DropTemporaryInputLanguage()
        {
            TemporaryInputLanguage = 0;
        }


        public void SwitchToLanguage(int index)
        {
            UsedInputLanguage usedInputLanguage = usedInputLanguages[index];
            usedInputLanguages.RemoveAt(index);
            usedInputLanguages.Insert(0, usedInputLanguage);
            TemporaryInputLanguage = 0;
        }

        public void SwitchToTemporaryInputLanguage()
        {
            SwitchToLanguage(TemporaryInputLanguage);
        }

        public event InputLanguageChangedDelegate InputLanguageChanged;

        public void FireInputLanguageChanged()
        {
            InputLanguageChanged?.Invoke(UsedInputLanguage);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
