using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using System.Globalization;
using EnvDTE80;

namespace BenClarkRobinson.AccurateReferences.Infrastructure
{
    public sealed class Core
    {
        private static readonly Core instance = new Core();
        public static Core Instance { get { return instance; } }

        static Core() { }

        private IVsOutputWindowPane lazyOutputWindowPane;
        private List<object> _events = new List<object>();

        public AccurateReferences2010Package AccurateReferencesPackage { get; set; }

        public SolutionEventsListener SolutionEventsListener { get; private set; }
        public EnvDTE80.DTE2 Dte { get; private set; }
        public Events2 Events { get; private set; }

        public Project StartupProject
        {
            get
            {
                var startupProjects = (Array)Dte.Solution.SolutionBuild.StartupProjects;
                if (startupProjects == null)
                    return null;

                if (startupProjects.Length > 1)
                    throw new ApplicationException("The solution cannot contain more than one startup project. Active startup projects: " + String.Join(", ", startupProjects));

                Project startupProject = Dte.Solution.Projects.Item(startupProjects.GetValue(0));
                return startupProject;
            }
        }

        public Core()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Start Core(): {0}", this.ToString()));

            //SolutionEventsListener class from http://stackoverflow.com/questions/2525457/automating-visual-studio-with-envdte 
            // via Elisha http://stackoverflow.com/users/167149/elisha
            SolutionEventsListener = new SolutionEventsListener();

            SolutionEventsListener.OnAfterCloseSolution += () =>
            {
                ToolWindowPane window = this.AccurateReferencesPackage.FindToolWindow(typeof(MyToolWindow), 0, true);
                if (window != null)
                    ((MyControl)window.Content).OnSolutionUnload();
            };

            SolutionEventsListener.OnAfterOpenSolution += () =>
            {
                ToolWindowPane window = this.AccurateReferencesPackage.FindToolWindow(typeof(MyToolWindow), 0, true);
                if (window != null)
                    ((MyControl)window.Content).OnSolutionLoad();
            };

            Dte = Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;
            Events = Dte.Events as Events2;
        }
    }
}
