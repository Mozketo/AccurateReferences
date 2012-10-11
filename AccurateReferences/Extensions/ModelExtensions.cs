using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using BenClarkRobinson.AccurateReferences.Models;

namespace BenClarkRobinson.AccurateReferences.Extensions
{
    public static class ModelExtensions
    {
        public static TreeViewItem ToTreeViewItem(this ProjectModel project)
        {
            var node = new TreeViewItem { Header = project.Name };
            if (!project.AllChildrenExist)
                node.ExpandSubtree();
            return node;
        }

        public static TreeViewItem ToTreeViewItem(this ReferenceModel reference)
        {
            var node = new TreeViewItem
            {
                Header = String.Format("{0}: {1}", reference.ItemType, reference.ItemLocation),
                Foreground = new SolidColorBrush(reference.Color),
            };

            if (!reference.Exists)
                node.Header = String.Format("{0} (Missing)", node.Header);

            if (reference.SimilarRefences != null && reference.SimilarRefences.Any())
            {
                var parents = reference.SimilarRefences.Select(r => r.Parent).Distinct().ToCsv(i => i.Name, ", ");
                node.Header = String.Format("{0} (Different to: {1})", node.Header, parents);
            }

            return node;
        }
    }
}
