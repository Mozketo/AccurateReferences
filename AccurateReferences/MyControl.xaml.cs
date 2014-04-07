using BenClarkRobinson.AccurateReferences.Extensions;
using BenClarkRobinson.AccurateReferences.Infrastructure;
using BenClarkRobinson.AccurateReferences.Infrastructure.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BenClarkRobinson.AccurateReferences.Models;


namespace BenClarkRobinson.AccurateReferences
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class MyControl : UserControl
    {
        public MyControl()
        {
            InitializeComponent();
            InitializeReferences();
        }

        public void OnSolutionLoad()
        {
            var refTreeView = (TreeView)this.FindName("tree");
            if (refTreeView == null)
                return;

            refTreeView.Items.Clear();
            refTreeView.Items.Add(new TreeViewItem { Header = "Please click the refresh button to see all references" });
        }

        public void OnSolutionUnload() 
        {
            var refTreeView = (TreeView)this.FindName("tree");
            if (refTreeView == null)
                return;

            refTreeView.Items.Clear();
            refTreeView.Items.Add(new TreeViewItem { Header = "Please open a solution to see all references" });
        }


    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void search_Click(object sender, RoutedEventArgs e)
        {
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            var searchTextBox = (TextBox)this.FindName("search");
            var showSystemReferences = (CheckBox)this.FindName("showSystemReferences");
            var showMissingOnly = (RadioButton)this.FindName("showMissingOnly");

            var solution = Infrastructure.Core.Instance.Dte.Solution;
            var engine = new ReferenceEngine(solution, searchTextBox.Text, showSystemReferences.IsChecked.Value, showMissingOnly.IsChecked.Value);
            UpdateUi(engine.References);
        }

        private void UpdateUi(IEnumerable<ProjectModel> projects)
        {
            var refTreeView = (TreeView)this.FindName("tree");
            if (refTreeView == null)
                return;

            var searchTextBox = (TextBox)this.FindName("search");
            if (searchTextBox == null)
                return;

            var showConflictingOnlyCtrl = ((RadioButton)this.FindName("showConflictingOnly"));
            if (showConflictingOnlyCtrl == null)
                return;

            bool showConflictingOnly = showConflictingOnlyCtrl.IsChecked.Value;

            refTreeView.Items.Clear();

            if (!projects.Any())
            {
                refTreeView.Items.Add(new TreeViewItem {Header = "Please open a solution to see all references"});
                return;
            }

            foreach (ProjectModel project in projects.OrderBy(p => p.Name))
            {
                if (!project.Children.Any())
                    continue;

                // Is showing only conflicting Refs and if this project doesn't have any children. Well then don't show anything.
                if (showConflictingOnly && !project.Children.Any(c => c.IsConflicting))
                    continue;

                var parentNode = project.ToTreeViewItem();
                if (!String.IsNullOrEmpty(searchTextBox.Text) || showConflictingOnly)
                    parentNode.ExpandSubtree(); // If searching for something force open the subtrees
                refTreeView.Items.Add(parentNode);

                foreach (ReferenceModel child in project.Children)
                {
                    var childNode = child.ToTreeViewItem(showConflictingOnly);
                    if (childNode != null)
                        parentNode.Items.Add(childNode);
                }
            }
        }

        private void tree_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var tviSender = sender as TreeView;
            if (tviSender == null || tviSender.SelectedItem == null)
                return;

            if (String.IsNullOrEmpty(tviSender.SelectedValue.ToString()))
                return;

            var item = tviSender.SelectedItem as TreeViewItem;
            if (item == null)
                return;

            var header = item.Header.ToString();
            if (header.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase) > -1)
            {
                header = header.Substring(header.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase));
                if (header.IndexOf(" (", StringComparison.CurrentCultureIgnoreCase) > -1)
                    header = header.Substring(0, header.IndexOf(" (", StringComparison.CurrentCultureIgnoreCase));
            }

            var filename = Path.GetFileName(header);
            if (String.IsNullOrEmpty(filename))
                return;

            var searchTextBox = (TextBox)this.FindName("search");
            if (searchTextBox == null)
                return;

            searchTextBox.Text = filename;
            InitializeReferences();
        }
    }
}