using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NormalKeyboardSwitcher
{
    class UsedInputLanguage
    {


        private InputLanguage inputLanguage;
        private bool used;

        public UsedInputLanguage(InputLanguage inputLanguage)
        {
            this.inputLanguage = inputLanguage;
        }

        public InputLanguage InputLanguage
        {
            get
            {
                return inputLanguage;
            }
        }

        public bool Used
        {
            get
            {
                return used;
            }
            set
            {
                used = value;
            }
        }

        override public string ToString()
        {
            return inputLanguage.LayoutName;
        }
            
    }
}
