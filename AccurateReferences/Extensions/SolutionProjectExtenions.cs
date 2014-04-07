using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenClarkRobinson.AccurateReferences.Infrastructure.Parser;
using BenClarkRobinson.AccurateReferences.Models;

namespace BenClarkRobinson.AccurateReferences.Extensions
{
    public static class SolutionProjectExtenions
    {
        public static ProjectModel ToModel(this SolutionProject project, string startsWith, bool showSystemReferences, bool showMissingOnly)
        {
            var model = new ProjectModel { Name = project.ProjectName, Id = Guid.Parse(project.ProjectGuid) };
            model.Children = project.GetReferences(model, startsWith, showSystemReferences, showMissingOnly);
            return model;
        }

        public static IEnumerable<ReferenceModel> GetReferences(this SolutionProject project, ProjectModel parentModel, string startsWith, bool showSystemReferences, bool showMissingOnly)
        {
            var items = new List<ReferenceModel>();

            foreach (var item in project.References)
            {
                var projectPath = item.Project.DirectoryPath;

                string itemLocation = item.UnevaluatedInclude;

                var hintPath = item.Metadata.FirstOrDefault(m => m.Name == "HintPath");
                if (hintPath != null)
                    itemLocation = hintPath.UnevaluatedValue;

                if (!showSystemReferences && (item.UnevaluatedInclude.StartsWith("System") || item.UnevaluatedInclude.StartsWith("Microsoft") || item.UnevaluatedInclude.StartsWith("mscor")))
                    continue;

                if (!String.IsNullOrEmpty(startsWith) && !itemLocation.ToLower().Contains(startsWith.ToLower()))
                    continue;

                var path = System.IO.Path.Combine(projectPath, itemLocation);
                if (!itemLocation.Contains(@"\") || File.Exists(path))
                {
                    var projectItemModel = new ReferenceModel
                    {
                        Path = path,
                        ItemLocation = itemLocation,
                        ItemType = item.ItemType,
                        Parent = parentModel
                    };

                    var specificVersion = item.Metadata.FirstOrDefault(m => m.Name == "SpecificVersion");
                    if ((specificVersion == null || specificVersion.UnevaluatedValue == "True") && item.ItemType == "Reference")
                    {
                        projectItemModel.SpecificVersion = true;
                    }

                    if (showMissingOnly && !File.Exists(path))
                        items.Add(projectItemModel);
                    else if (!showMissingOnly)
                        items.Add(projectItemModel);
                }
                else
                {
                    var projectItemModel = new ReferenceModel
                    {
                        Path = path,
                        ItemLocation = itemLocation,
                        ItemType = item.ItemType,
                        Parent = parentModel
                    };
                    
                    if (showMissingOnly)
                    {
                        if (!File.Exists(path))
                            items.Add(projectItemModel);
                    }
                    else
                    {
                        items.Add(projectItemModel);
                    }
                }
            }

            return items;
        }
    }
}
