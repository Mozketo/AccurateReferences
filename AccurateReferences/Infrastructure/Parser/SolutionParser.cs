using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Build.Evaluation;

namespace BenClarkRobinson.AccurateReferences.Infrastructure.Parser
{
    public class SolutionParser
    {
        public IEnumerable<SolutionProject> Parse(FileInfo fileInfo)
        {
            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
            var projects = new Solution(fileInfo.FullName).Projects;
            projects.ForEach(project =>
            {
                var path = Path.Combine(fileInfo.Directory.FullName, project.RelativePath);
                var refs = References(new FileInfo(path));
                project.References = refs;
            });
            return projects;
        }

        IEnumerable<ProjectItem> References(FileInfo projectFile)
        {
            if (!File.Exists(projectFile.FullName))
                return Enumerable.Empty<ProjectItem>();
            Project project;
            try
            {
                project = new Microsoft.Build.Evaluation.Project(projectFile.FullName);
            }
            catch (Exception e)
            {
                return Enumerable.Empty<ProjectItem>();
            }
            var references = project.Items.Where(m => m.ItemType == "Reference" || m.ItemType == "ProjectReference");
            return references;
        }
    }
}
