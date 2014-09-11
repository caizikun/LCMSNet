﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

using LcmsNetDataClasses.Devices;
using LcmsNetDataClasses.Logging;

namespace LcmsNet.Devices.Dashboard
{
    /// <summary>
    /// 
    /// </summary>
    public partial class formDeviceAddForm : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public formDeviceAddForm()
        {
            InitializeComponent();
            mtree_availableDevices.KeyUp                += new KeyEventHandler(mtree_availableDevices_KeyUp);
            mtree_availableDevices.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(mtree_availableDevices_NodeMouseDoubleClick);
            mlistbox_devices.KeyUp                      += new KeyEventHandler(mlistbox_devices_KeyUp);
        }

        void mlistbox_devices_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keys.Delete == e.KeyData)
            {
                RemoveSelectedItems();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mtree_availableDevices_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            AddSelectedNode();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mtree_availableDevices_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddSelectedNode();
            }
        }
        /// <summary>
        /// Adds the supplied plugins to the check box list.
        /// </summary>
        /// <param name="plugins"></param>
        public void AddPluginInformation(List<classDevicePluginInformation> plugins)
        {                        
            mtree_availableDevices.BeginUpdate();
            mtree_availableDevices.Nodes.Clear();
            foreach (classDevicePluginInformation info in plugins)
            {
                TreeNode rootNode = null;
                if (!mtree_availableDevices.Nodes.ContainsKey(info.DeviceAttribute.Category))
                {
                    TreeNode newParent  = new TreeNode();
                    newParent.Text      = info.DeviceAttribute.Category;
                    newParent.Name      = info.DeviceAttribute.Category;
                    rootNode            = newParent;
                    mtree_availableDevices.Nodes.Add(newParent);
                }
                else
                {
                    rootNode = mtree_availableDevices.Nodes[info.DeviceAttribute.Category];
                }

                TreeNode node   = new TreeNode();
                node.Name       = info.DeviceAttribute.Name;
                node.Text       = info.DeviceAttribute.Name;
                node.Tag        = info;

                rootNode.Nodes.Add(node);
            }
            mtree_availableDevices.ExpandAll();
            mtree_availableDevices.EndUpdate();
        }
        public List<classDevicePluginInformation> GetSelectedPlugins()
        {
            List<classDevicePluginInformation> plugins = new List<classDevicePluginInformation>();
            foreach(object o in mlistbox_devices.Items)
            {
                classDevicePluginInformation plugin = o as classDevicePluginInformation;
                if (plugin != null)
                {
                    plugins.Add(plugin);
                }
            }
            return plugins;
        }
        /// <summary>
        /// Gets or sets the initialize on add flag.
        /// </summary>
        public bool InitializeOnAdd
        {
            get
            {
                return mcheckBox_initializeOnAdd.Checked;
            }
            set
            {
                mcheckBox_initializeOnAdd.Checked = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mbutton_ok_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mbutton_cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mbutton_add_Click(object sender, EventArgs e)
        {
            AddSelectedNode();
        }
        /// <summary>
        /// Adds the selected node to the list box of devices to be loaded.
        /// </summary>
        private void AddSelectedNode()
        {
            if (mtree_availableDevices.SelectedNode != null)
            {
                if (mtree_availableDevices.SelectedNode.Tag == null)
                    return;

                mlistbox_devices.Items.Add(mtree_availableDevices.SelectedNode.Tag);
            }
        }
        private void RemoveSelectedItems()
        {
            if (mlistbox_devices.SelectedItems != null)
            {
                mlistbox_devices.BeginUpdate();

                List<object> pluginsToRemove = new List<object>();
                foreach (object o in mlistbox_devices.SelectedItems)
                {
                    if (o != null)
                    {
                        pluginsToRemove.Add(o);
                    }
                }
                foreach (object o in pluginsToRemove)
                {
                    mlistbox_devices.Items.Remove(o);
                }
                mlistbox_devices.EndUpdate();
            }
        }
        private void mbutton_remove_Click(object sender, EventArgs e)
        {
            RemoveSelectedItems();
        }
    }
}
