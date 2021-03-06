﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LcmsNet.SampleQueue.Forms
{
    public partial class formExpansion : Form
    {
        /// <summary>
        /// Tracks whether the button's event handler has already been registered here before.
        /// </summary>
        Dictionary<Button, bool> m_registeredButton;

        public formExpansion()
        {
            InitializeComponent();
        }

        public void UpdateButtons(List<Button> buttons)
        {
            Initialize(buttons);
        }

        private void Initialize(List<Button> buttons)
        {
            m_registeredButton = new Dictionary<Button, bool>();
            KeyDown += formExpansion_KeyDown;
            LostFocus += formExpansion_LostFocus;
            MouseLeave += formExpansion_MouseLeave;
            FormClosing += formExpansion_FormClosing;

            Controls.Clear();

            var left = 0;
            var width = 0;
            if (buttons.Count > 0)
            {
                width = 60;
                var padding = 2;
                left = 32;
                Width = ((width + padding) * buttons.Count) + left;
                foreach (var button in buttons)
                {
                    if (!m_registeredButton.ContainsKey(button))
                    {
                        m_registeredButton.Add(button, true);
                        button.MouseLeave += button_MouseLeave;
                        button.Click += button_Click;
                    }
                    button.Left = left;
                    button.Width = width;
                    left += (width + padding);
                    button.Height = Height - button.Top - padding;
                    Controls.Add(button);
                }
            }
            Refresh();
            PerformLayout();
        }

        void button_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            if (button.DialogResult == DialogResult.OK)
            {
                DialogResult = DialogResult.OK;
            }
        }

        void button_MouseLeave(object sender, EventArgs e)
        {
            CheckIfMouseGone();
        }

        void formExpansion_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                DialogResult = DialogResult.Cancel;
                Hide();
            }
        }

        private void formExpansion_Load(object sender, EventArgs e)
        {
        }

        #region Exit

        private void CheckIfMouseGone()
        {
            var cursorPoint = Cursor.Position;
            var cursorClient = PointToClient(cursorPoint);

            if (cursorClient.X < 0 || cursorClient.X >= Width)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (cursorClient.Y < 0 || cursorClient.Y >= Height)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        void formExpansion_MouseLeave(object sender, EventArgs e)
        {
            CheckIfMouseGone();
        }

        void formExpansion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        void formExpansion_LostFocus(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion
    }
}