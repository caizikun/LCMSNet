﻿using System;

namespace LcmsNet.Devices.NetworkStart.Socket
{
    public class classNetStartArgument
    {
        private string mstr_key;
        private string mstr_value;

        public classNetStartArgument()
        {
            mstr_key = "";
            mstr_value = "";
        }

        public classNetStartArgument(string key, string value)
        {
            mstr_key = key;
            mstr_value = value;
        }

        public string Key
        {
            get { return mstr_key; }
            set { mstr_key = value; }
        }
        public string Value
        {
            get { return mstr_value; }
            set { mstr_value = value; }
        }
    }
}
