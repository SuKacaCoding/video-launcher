using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace GongSolutions.Shell
{
    /// <summary>
    /// Provides a toolbar suitable for use in file Open/Save dialogs.
    /// </summary>
    /// <remarks>
    /// This control provides a toolbar containing a <see cref="ShellComboBox"/> and the various
    /// navigation buttons as found in a standard file dialog. By setting the <see
    /// cref="ShellView"/> property, the toolbar will automatically control the navigation of a
    /// ShellView&gt; control in response to the user's actions.
    /// </remarks>
    public partial class FileDialogToolbar : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the FileDialogToolbar control.
        /// </summary>
        public FileDialogToolbar()
        {
            InitializeComponent();
            Dock = DockStyle.Top;
            toolStrip.Renderer = new CustomRenderer();
        }

        /// <summary>
        /// Gets/sets the root folder displayed in the toolbar's drop-down folder list.
        /// </summary>
        [Editor(typeof(ShellItemEditor), typeof(UITypeEditor))]
        public ShellItem RootFolder
        {
            get { return shellComboBox.RootFolder; }
            set { shellComboBox.RootFolder = value; }
        }

        /// <summary>
        /// Gets/sets the folder currently selected in the toolbar's combo box.
        /// </summary>
        [Editor(typeof(ShellItemEditor), typeof(UITypeEditor))]
        public ShellItem SelectedFolder
        {
            get { return shellComboBox.SelectedFolder; }
            set { shellComboBox.SelectedFolder = value; }
        }

        /// <summary>
        /// Gets/sets a <see cref="ShellView"/> whose navigation should be controlled by the toolbar.
        /// </summary>
        [DefaultValue(null), Category("Behaviour")]
        public ShellView ShellView
        {
            get { return shellComboBox.ShellView; }
            set
            {
                shellComboBox.ShellView = value;
                shellComboBox.Enabled = viewMenuButton.Enabled = (value != null);
                UpdateButtons();
            }
        }

        /// <summary>
        /// Occurs when the <see cref="FileDialogToolbar"/> needs to know what items it should
        /// display in its drop-down list.
        /// </summary>
        public event FilterItemEventHandler FilterItem
        {
            add { shellComboBox.FilterItem += value; }
            remove { shellComboBox.FilterItem -= value; }
        }

        private bool ShouldSerializeRootFolder()
        {
            return shellComboBox.ShouldSerializeRootFolder();
        }

        private bool ShouldSerializeSelectedFolder()
        {
            return shellComboBox.ShouldSerializeSelectedFolder();
        }

        private void UpdateButtons()
        {
            if (ShellView != null)
            {
                backButton.Enabled = ShellView.CanNavigateBack;
                upButton.Enabled = ShellView.CanNavigateParent;
                newFolderButton.Enabled = ShellView.CanCreateFolder;
            }
        }

        private void shellComboBox_Changed(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        private void shellView_Navigated(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            ShellView.NavigateBack();
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            ShellView.NavigateParent();
        }

        private void newFolderButton_Click(object sender, EventArgs e)
        {
            ShellView.CreateNewFolder();
        }

        private void viewThumbnailsMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            ShellView.View = (ShellViewStyle)Convert.ToInt32(item.Tag);
        }

        private class CustomRenderer : ToolStripSystemRenderer
        {
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
            }
        }
    }
}