﻿using Microsoft.Win32;
using MosaiqueServeur.Models;
using MosaiqueServeur.Packets.ServerPackets;
using Serveur.Controllers.Server;
using System;
using System.Globalization;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using static MosaiqueServeur.Assets.Controls.WordTextBox;

namespace MosaiqueServeur.Views
{
    public partial class FrmRegValueEditWord : Form
    {
        private readonly ClientMosaique _connectClient;

        private readonly RegValueData _value;

        private readonly string _keyPath;



        #region CONSTANT

        private const string DWORD_WARNING = "The decimal value entered is greater than the maximum value of a DWORD (32-bit number). Should the value be truncated in order to continue?";
        private const string QWORD_WARNING = "The decimal value entered is greater than the maximum value of a QWORD (64-bit number). Should the value be truncated in order to continue?";

        #endregion

        public FrmRegValueEditWord(string keyPath, RegValueData value, ClientMosaique c)
        {
            _connectClient = c;
            _keyPath = keyPath;
            _value = value;

            InitializeComponent();

            this.valueNameTxtBox.Text = value.Name;


            var serializer = new JavaScriptSerializer();

            if (value.Kind == RegistryValueKind.DWord)
            {
                this.Text = "Edit DWORD (32-bit) Value";
                this.valueDataTxtBox.Type = WordType.DWORD;
                //var result = serializer.DeserializeObject(value.Data);
                try
                {
                    this.valueDataTxtBox.Text = ((uint)int.Parse(value.Data.ToString())).ToString("x");
                }
                catch
                {
                    this.valueDataTxtBox.Text = ((uint)int.Parse("0")).ToString("x");
                }
            }
            else
            {
                this.Text = "Edit QWORD (64-bit) Value";
                this.valueDataTxtBox.Type = WordType.QWORD;
               // var result = serializer.DeserializeObject(value.Data);
                this.valueDataTxtBox.Text = ((ulong)long.Parse(value.Data.ToString())).ToString("x");
            }
        }
        
        private void radioHex_CheckboxChanged(object sender, EventArgs e)
        {
            if (valueDataTxtBox.IsHexNumber == radioHexa.Checked)
                return;

            if (valueDataTxtBox.IsConversionValid() || IsOverridePossible())
                valueDataTxtBox.IsHexNumber = radioHexa.Checked;
            else
                radioDecimal.Checked = true;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (valueDataTxtBox.IsConversionValid() || IsOverridePossible())
            {
                object valueData = null;
                if (_value.Kind == RegistryValueKind.DWord)
                    valueData = (int)valueDataTxtBox.UIntValue;
                else
                    valueData = (long)valueDataTxtBox.ULongValue;

                new DoChangeRegistryValue(_keyPath, new RegValueData(_value.Name, _value.Kind, new JavaScriptSerializer().Serialize(valueData))).Execute(_connectClient);
            }
            else
            {
                //Prevent exit
                DialogResult = DialogResult.None;
            }

            this.Close();
        }

        private DialogResult ShowWarning(string msg, string caption)
        {
            return MessageBox.Show(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }

        private bool IsOverridePossible()
        {
            string message = _value.Kind == RegistryValueKind.DWord ? DWORD_WARNING : QWORD_WARNING;

            return ShowWarning(message, "Overflow") == DialogResult.Yes;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
