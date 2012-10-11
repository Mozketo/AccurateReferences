using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace BenClarkRobinson.AccurateReferences.Models
{
    public class ProjectModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<ReferenceModel> Children { get; set; }

        public bool AllChildrenExist
        {
            get
            {
                var anyNotExist = this.Children.Any(c => !c.Exists);
                return !anyNotExist;
            }
        }

        public ProjectModel()
        {
            Id = Guid.NewGuid();
        }
    }

    public class ReferenceModel
    {
        private string _itemLocation;

        public Guid Id { get; set; }
        public ProjectModel Parent { get; set; }
        public IEnumerable<ReferenceModel> SimilarRefences { get; set; }
        public string Path { get; set; }
        public string ItemType { get; set; }
        public bool SpecificVersion { get; set; }
        public Color Color { get; set; }

        public string ItemLocation { 
            get { return _itemLocation; } 
            set { _itemLocation = value.Replace(@"/", @"\"); } 
        }

        public string Filename
        {
            get { return System.IO.Path.GetFileName(this.Path); }
        }

        public bool Exists
        {
            get { return System.IO.File.Exists(this.Path); }
        }

        public ReferenceModel()
        {
            Id = Guid.NewGuid();
            Color = Colors.Black;
        }
    }
}
