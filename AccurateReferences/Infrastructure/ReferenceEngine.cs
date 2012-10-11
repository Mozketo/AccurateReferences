using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using BenClarkRobinson.AccurateReferences.Extensions;
using BenClarkRobinson.AccurateReferences.Infrastructure.Parser;
using BenClarkRobinson.AccurateReferences.Models;

namespace BenClarkRobinson.AccurateReferences.Infrastructure
{
    public class ReferenceEngine
    {
        private readonly IEnumerable<ProjectModel> _references;
        private List<Color> _pairedColors = new List<Color> { Colors.Blue, Colors.BlueViolet, Colors.Brown, Colors.DarkCyan, Colors.DarkSeaGreen };

        public IEnumerable<ProjectModel> References
        {
            get { return _references; }
        }

        public ReferenceEngine(EnvDTE.Solution solution, string startsWith, bool showSystemReferences, bool showMissingOnly)
        {
            if (solution == null || String.IsNullOrEmpty(solution.FullName))
                _references = Enumerable.Empty<ProjectModel>();
            _references = ParseSolutionFile(solution.FullName, startsWith, showSystemReferences, showMissingOnly);
        }

        private IEnumerable<ProjectModel> ParseSolutionFile(string solutionPath, string startsWith, bool showSystemReferences, bool showMissingOnly)
        {
            if (String.IsNullOrEmpty(solutionPath))
                return Enumerable.Empty<ProjectModel>();

            var projects = new SolutionParser()
                .Parse(new FileInfo(solutionPath));

            IList<ProjectModel> projectsAndReferences = projects.Select(p => p.ToModel(startsWith, showSystemReferences, showMissingOnly)).ToList();

            ColoriseDupFilesWithDifferentPaths(ref projectsAndReferences);
            ColoriseMissingFiles(ref projectsAndReferences);

            var red = projectsAndReferences.SelectMany(p => p.Children).Where(c => c.Color != Colors.Black).ToList();

            return projectsAndReferences;
        }

        /// <summary>
        /// If a file (EntityFramework) is referenced multiple times (with different paths) colourise the matches
        /// </summary>
        static Random _r = new Random();
        private void ColoriseDupFilesWithDifferentPaths(ref IList<ProjectModel> projects)
        {
            // If there's a different version reference between 2 or more projects then flag this.
            // Example: ProjectA has EF 4.3.1. ProjectB has EF 5.0. We need to flag this in the UI.
            var children = projects.SelectMany(p => p.Children);
            children.Where(c => c.ItemType != "ProjectReference")
                .ToList()
                .ForEach(c =>
                    {
                        var similar = children.Where(x => x.Filename.Equals(c.Filename, StringComparison.CurrentCultureIgnoreCase)
                            && !x.ItemLocation.Replace(@"..\", "").Equals(c.ItemLocation.Replace(@"..\", ""), StringComparison.CurrentCultureIgnoreCase)).ToList();
                        if (similar.Any())
                        {
                            int i = _r.Next(_pairedColors.Count - 1);
                            Color color = _pairedColors.Skip(i).First();
                            c.SimilarRefences = similar.ToList();
                            c.Color = color;
                            similar.ForEach(x => x.Color = color);
                        }
                    });
            }

        /// <summary>
        /// If there are any files missing then mark these red.
        /// </summary>
        private void ColoriseMissingFiles(ref IList<ProjectModel> projects)
        {
            projects.ToList().ForEach(project => project.Children.Where(r => !r.Exists)
                .ToList()
                .ForEach(r => r.Color = Colors.Red));
        }
    }
}
