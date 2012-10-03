using BenClarkRobinson.AccurateReferences.Infrastructure.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            var solution = Infrastructure.Core.Instance.Dte.Solution;
            ParseSolutionFile(solution.FullName);
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void search_Click(object sender, RoutedEventArgs e)
        {
            var solution = Infrastructure.Core.Instance.Dte.Solution;
            ParseSolutionFile(solution.FullName);
        }

        private void ParseSolutionFile(string solutionPath)
        {
            var projects = new SolutionParser().Parse(new FileInfo(solutionPath));

            var refTreeView = (TreeView)this.FindName("tree");
            var search = (TextBox)this.FindName("search");
            
            refTreeView.Items.Clear();
            foreach (var project in projects)
            {
                var treeNode = new TreeViewItem(); //String.Format("Project: {0}", project.ProjectName));
                treeNode.Header = String.Format("Project: {0}", project.ProjectName);

                var missingRef = false;
                foreach (var item in project.References)
                {
                    var projectPath = item.Project.DirectoryPath;

                    string itemLocation = item.UnevaluatedInclude;

                    var hintPath = item.Metadata.FirstOrDefault(m => m.Name == "HintPath");
                    if (hintPath != null)
                        itemLocation = hintPath.UnevaluatedValue;

                    if (!String.IsNullOrEmpty(search.Text) && !itemLocation.ToLower().Contains(search.Text.ToLower()))
                        continue;

                    var path = System.IO.Path.Combine(projectPath, itemLocation);
                    if (!itemLocation.Contains(@"\") || File.Exists(path))
                    {
                        var node = new TreeViewItem(); //TreeNode(String.Format("{0}: {1}", item.ItemType, itemLocation));
                        node.Header = String.Format("{0}: {1}", item.ItemType, itemLocation);

                        bool isSpecificVersion = false;
                        var specificVersion = item.Metadata.FirstOrDefault(m => m.Name == "SpecificVersion");
                        if ((specificVersion == null || specificVersion.UnevaluatedValue == "True") && item.ItemType == "Reference")
                        {
                            node.Header = String.Format("{0} (Specific Version)", node.Header);
                            isSpecificVersion = true;
                        }

                        //if ((chkSpecificVersion.Checked && isSpecificVersion) || !chkSpecificVersion.Checked)
                        treeNode.Items.Add(node);
                    }
                    else
                    {
                        var node = new TreeViewItem(); //TreeNode(String.Format("{0}: {1}", item.ItemType, itemLocation)) { ForeColor = Color.Red };
                        node.Header = String.Format("{0}: {1}", item.ItemType, itemLocation);
                        node.Foreground = new SolidColorBrush(Colors.Red);
                        treeNode.Items.Add(node);
                        missingRef = true;
                    }

                }

                if (treeNode.Items.Count > 0)
                {
                    if (missingRef)
                    {
                        treeNode.Foreground = new SolidColorBrush(Colors.Red);
                        treeNode.ExpandSubtree();
                    }
                    refTreeView.Items.Add(treeNode);
                }
            }

            //if (refTreeView.TopNode != null)
            //    refTreeView.TopNode.EnsureVisible();
        }
    }
}