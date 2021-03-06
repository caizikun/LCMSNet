﻿using System;
using System.Windows.Forms;

namespace LcmsNet.Method.Forms
{
    public partial class formConvertToSeconds : Form
    {
        public formConvertToSeconds()
        {
            InitializeComponent();
        }

        public formConvertToSeconds(int seconds, int precision)
        {
            InitializeComponent();

            try
            {
                mnum_minutes.Value = Convert.ToDecimal(Math.Round(Convert.ToDouble(seconds) / 60.0, 0));
                mnum_seconds.Value = Convert.ToDecimal(seconds % 60);

                mnum_decimalPlaces.Value = Convert.ToDecimal(precision);
            }
            catch
            {
            }
        }

        public ConversionType ConversionType
        {
            get
            {
                if (radioButton1.Checked)
                {
                    return ConversionType.Time;
                }
                return ConversionType.Precision;
            }
        }

        public int GetTimeInSeconds()
        {
            var minutes = Convert.ToInt32(mnum_minutes.Value);
            var seconds = Convert.ToInt32(mnum_seconds.Value);

            return minutes * 60 + seconds;
        }

        public int GetDecimalPlaces()
        {
            return Convert.ToInt32(mnum_decimalPlaces.Value);
        }
    }

    public enum ConversionType
    {
        Time,
        Precision
    }
}