﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace addressesbookybook
{
    public partial class addressstuff : Form
    {
        public addressstuff()
        {
            InitializeComponent();
        }

        private void buttonsave_Click(object sender, EventArgs e)
        {
            SaveFileDialog(form1.cs[Design]);

        }

        private void buttonquit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonadd_Click(object sender, EventArgs e)
        {
           // not conecting and is infuriating
        }

        private void buttonremove_Click(object sender, EventArgs e)
        {
            CheckedListBox.CheckedIndexCollection(Remove checked stuff );

        }

        private void buttonedit_Click(object sender, EventArgs e)
        {
          //should conect to other form but doesn't
        }
        public class Adres
        {
            public string firstname;
            public string lastname; 
            public string street;
            public string city;  
            public string state;
            public string zip;
            public override string ToString()
            {
                return base.ToString();
            }
        }
    }
}
