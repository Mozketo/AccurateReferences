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
            InitializeReferences();
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void search_Click(object sender, RoutedEventArgs e)
        {
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            var solution = Infrastructure.Core.Instance.Dte.Solution;
            var result = ParseSolutionFile(solution.FullName);
#if (DEBUG)
            var x = result.ToList();
#endif
            result = ProcessProjectItemModels(result);
            UpdateUi(result);            
        }

        private IEnumerable<ProjectItemModel> ParseSolutionFile(string solutionPath)
        {
            var projects = new SolutionParser().Parse(new FileInfo(solutionPath));
            var searchTextBox = (TextBox)this.FindName("search");
            var showSystemReferences = (CheckBox)this.FindName("showSystemReferences");

            var items = new List<ProjectItemModel>();

            foreach (var project in projects)
            {
                var parent = new ProjectItemModel { Path = project.ProjectName, Id = Guid.Parse(project.ProjectGuid) };
                items.Add(parent);

                foreach (var item in project.References)
                {
                    var projectPath = item.Project.DirectoryPath;

                    string itemLocation = item.UnevaluatedInclude;

                    var hintPath = item.Metadata.FirstOrDefault(m => m.Name == "HintPath");
                    if (hintPath != null)
                        itemLocation = hintPath.UnevaluatedValue;

                    if (!showSystemReferences.IsChecked.Value && (item.UnevaluatedInclude.StartsWith("System") || item.UnevaluatedInclude.StartsWith("Microsoft")))
                        continue;

                    if (!String.IsNullOrEmpty(searchTextBox.Text) && !itemLocation.ToLower().Contains(searchTextBox.Text.ToLower()))
                        continue;

                    var path = System.IO.Path.Combine(projectPath, itemLocation);
                    if (!itemLocation.Contains(@"\") || File.Exists(path))
                    {
                        var projectItemModel = new ProjectItemModel { Path = path, ItemLocation = itemLocation, ItemType = item.ItemType };
                        projectItemModel.Parent = parent.Id;

                        var specificVersion = item.Metadata.FirstOrDefault(m => m.Name == "SpecificVersion");
                        if ((specificVersion == null || specificVersion.UnevaluatedValue == "True") && item.ItemType == "Reference")
                        {
                            projectItemModel.SpecificVersion = true;
                        }

                        items.Add(projectItemModel);
                    }
                    else
                    {
                        var projectItemModel = new ProjectItemModel { Path = path, ItemLocation = itemLocation, ItemType = item.ItemType };
                        projectItemModel.Parent = parent.Id;
                        items.Add(projectItemModel);
                    }
                }
            }
            return items;
        }

        static Random _r = new Random();
        List<System.Windows.Media.Color> pairedColors = new List<Color> { Colors.Blue, Colors.BlueViolet, Colors.Brown, Colors.DarkCyan, Colors.DarkSeaGreen };
        private IEnumerable<ProjectItemModel> ProcessProjectItemModels(IEnumerable<ProjectItemModel> projectItems)
        {
            // If there's a different version reference between 2 or more projects then flag this.
            // Example: ProjectA has EF 4.3.1. ProjectB has EF 5.0. We need to flag this in the UI.
            foreach (var projectItem in projectItems)
            {
                var similar = projectItems.Where(i => i.Filename.Equals(projectItem.Filename, StringComparison.InvariantCultureIgnoreCase)).ToList();
                var similarWithDifferentPath = similar.Where(s => !s.Path.Equals(projectItem.Path) && s.Id != projectItem.Id).ToList();
                if (similarWithDifferentPath.Any())
                {
                    projectItem.HasDifferentVersion = true;
                    similarWithDifferentPath.ForEach(m => m.HasDifferentVersion = true);
                }
            }

            return projectItems;
        }

        private void UpdateUi(IEnumerable<ProjectItemModel> items)
        {
            var refTreeView = (TreeView)this.FindName("tree");
            if (refTreeView == null)
                return;

            refTreeView.Items.Clear();

            var parents = items.Where(m => m.Parent == Guid.Empty).ToList();
            foreach (var parent in parents)
            {
                ProjectItemModel parentRef = parent;
                var parentNode = new TreeViewItem {Header = parentRef.Filename};
                refTreeView.Items.Add(parentNode);

                bool missingChild = false;
                var children = items.Where(m => m.Parent == parentRef.Id).ToList();
                foreach (var child in children)
                {
                    var childNode = new TreeViewItem
                                        {Header = String.Format("{0}: {1}", child.ItemType, child.ItemLocation)};

                    if (child.HasDifferentVersion)
                        childNode.Foreground = new SolidColorBrush(Colors.Blue);

                    if (!child.Exists)
                    {
                        missingChild = true;
                        childNode.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    parentNode.Items.Add(childNode);

                    if (missingChild)
                        parentNode.ExpandSubtree();
                }
            }
        }

        private void ParseSolutionFile2(string solutionPath)
        {
            var projects = new SolutionParser().Parse(new FileInfo(solutionPath));

            var refTreeView = (TreeView)this.FindName("tree");
            var searchTextBox = (TextBox)this.FindName("search");
            var showSystemReferences = (CheckBox)this.FindName("showSystemReferences");
            
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



                    if (!String.IsNullOrEmpty(searchTextBox.Text) && !itemLocation.ToLower().Contains(searchTextBox.Text.ToLower()))
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

    public class ProjectItemModel
    {
        public Guid Id { get; set; }
        public Guid Parent { get; set; }
        public string Path { get; set; }
        public string ItemType { get; set; }
        public string ItemLocation { get; set; }
        public bool SpecificVersion { get; set; }
        public bool HasDifferentVersion { get; set; }

        public string Filename { 
            get { return System.IO.Path.GetFileName(this.Path); }
        }

        public bool Exists
        {
            get { return System.IO.File.Exists(this.Path); }
        }

        public ProjectItemModel()
        {
            Parent = Guid.Empty;
            Id = Guid.NewGuid();
        }
    }
}