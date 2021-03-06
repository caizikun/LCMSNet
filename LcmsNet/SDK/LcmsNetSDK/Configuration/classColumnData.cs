﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using LcmsNetSDK;

namespace LcmsNetDataClasses.Configuration
{
    /// <summary>
    /// Class that manages all of the information about a given column
    /// </summary>
    [Serializable]
    public class classColumnData : classDataClassBase, INotifyPropertyChangedExt, IEquatable<classColumnData>
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public classColumnData()
        {
            m_name = "NOTSET";
            m_columnIndex = 0;
            m_systemIndex = 0;
            m_status = enumColumnStatus.Idle;
            m_first = false;
        }

        #endregion

        public override string ToString()
        {
            return string.Format("ID = {0} Name = {1}", ID, Name);
        }

        #region Delegate Definitions

        public delegate void DelegateStatusChanged(
            object sender, enumColumnStatus previousStatus, enumColumnStatus newStatus);

        /// <summary>
        /// Delegate definition called if the column first value is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="first"></param>
        public delegate void DelegateFirstChanged(object sender, bool first);

        public delegate void DelegateColorChanged(object sender, Color previousColor, Color newColor);

        public delegate void DelegateNameChanged(object sender, string name, string oldName);

        #endregion

        #region Members

        /// <summary>
        /// Index of the column.
        /// </summary>
        private int m_columnIndex;

        /// <summary>
        /// Name of the column
        /// </summary>
        private string m_name;

        /// <summary>
        /// System index of the column.
        /// </summary>
        private int m_systemIndex;

        /// <summary>
        /// Status of the column
        /// </summary>
        private enumColumnStatus m_status;

        /// <summary>
        /// Fired when the status of a column changes.
        /// </summary>
        [field: NonSerialized]
        public event DelegateStatusChanged StatusChanged;

        /// <summary>
        /// Fired when the color of the column changes.
        /// </summary>
        [field: NonSerialized]
        public event DelegateColorChanged ColorChanged;

        /// <summary>
        /// Fired if the first value of this column is changed.
        /// </summary>
        [field: NonSerialized]
        public event DelegateFirstChanged FirstChanged;

        /// <summary>
        /// An event that indicates the name of the column has changed.
        /// </summary>
        [field: NonSerialized]
        public event DelegateNameChanged NameChanged;

        /// <summary>
        /// Color of the column.
        /// </summary>
        private Color m_columnColor;

        /// <summary>
        /// Flag indicating if this is the first column to run.
        /// </summary>
        private bool m_first;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if the column is the first column designated to run.
        /// </summary>
        public bool First
        {
            get { return m_first; }
            set
            {
                m_first = value;
                FirstChanged?.Invoke(this, value);
            }
        }

        /// <summary>
        /// Gets or sets when the status changes.
        /// </summary>
        public enumColumnStatus Status
        {
            get { return m_status; }
            set
            {
                //
                // If the status has changed,
                // let someone know.
                //
                var previousStatus = m_status;
                if (this.RaiseAndSetIfChangedRetBool(ref m_status, value))
                {
                    StatusChanged?.Invoke(this, previousStatus, m_status);
                }
            }
        }

        /// <summary>
        /// Gets or sets the column index.
        /// </summary>
        public int ID
        {
            get { return m_columnIndex; }
            set { this.RaiseAndSetIfChanged(ref m_columnIndex, value); }
        }

        /// <summary>
        /// Gets or sets the system index.
        /// </summary>
        public int SystemID
        {
            get { return m_systemIndex; }
            set { m_systemIndex = value; }
        }

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set
            {
                var oldName = m_name;
                if (this.RaiseAndSetIfChangedRetBool(ref m_name, value))
                {
                    NameChanged?.Invoke(this, m_name, oldName);
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the column.
        /// </summary>
        public Color Color
        {
            get { return m_columnColor; }
            set
            {
                var oldColor = m_columnColor;
                if (this.RaiseAndSetIfChangedRetBool(ref m_columnColor, value))
                {
                    ColorChanged?.Invoke(this, oldColor, m_columnColor);
                    OnPropertyChanged(nameof(ColorWpf));
                }
            }
        }

        [NotStoredProperty]
        public System.Windows.Media.Color ColorWpf
        {
            get { return System.Windows.Media.Color.FromArgb(Color.A, Color.R, Color.G, Color.B); }
            set { Color = Color.FromArgb(value.A, value.R, value.G, value.B); }
        }

        #endregion

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

#if DotNET4
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

#else
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
#endif

        public bool Equals(classColumnData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return m_columnIndex == other.m_columnIndex && m_systemIndex == other.m_systemIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((classColumnData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (m_columnIndex * 397) ^ m_systemIndex;
            }
        }
    }
}