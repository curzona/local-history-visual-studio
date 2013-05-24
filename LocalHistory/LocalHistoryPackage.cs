/*
Copyright 2013 Intel Corporation

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace Intel.LocalHistory
{
  /// <summary>
  /// This is the class that implements the package exposed by this assembly.
  ///
  /// The minimum requirement for a class to be considered a valid package for Visual Studio
  /// is to implement the IVsPackage interface and register itself with the shell.
  /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
  /// to do it: it derives from the Package class that provides the implementation of the 
  /// IVsPackage interface and uses the registration attributes defined in the framework to 
  /// register itself and its components with the shell.
  /// </summary>
  // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
  // a package.
  [PackageRegistration(UseManagedResourcesOnly = true)]
  // This attribute is used to register the information needed to show this package
  // in the Help/About dialog of Visual Studio.
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
  // This attribute is needed to let the shell know that this package exposes some menus.
  [ProvideMenuResource("Menus.ctmenu", 1)]
  // This attribute registers a tool window exposed by this package.
  [ProvideToolWindow(typeof(LocalHistoryToolWindow))]
  [Guid(GuidList.guidLocalHistoryPkgString)]
  [ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82")]
  public sealed class LocalHistoryPackage : Package, IVsSolutionEvents
  {
    private EnvDTE.DTE dte;
    private uint solutionCookie;
    private uint rdtCookie;
    private DocumentRepository documentRepository;
    private LocalHistoryDocumentListener documentListener;

    private ToolWindowPane toolWindow;

    public LocalHistoryPackage()
    {
      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
    }

    /////////////////////////////////////////////////////////////////////////////
    // Overridden Package Implementation
    #region Package Members

    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited.
    /// </summary>
    protected override void Initialize()
    {
      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
      base.Initialize();

      // Add our command handlers for menu (commands must exist in the .vsct file)
      OleMenuCommandService mcs = (OleMenuCommandService)GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      if (null != mcs)
      {
        // Create the command for the menu item.
        CommandID menuCommandID = new CommandID(GuidList.guidLocalHistoryCmdSet, (int)PkgCmdIDList.cmdidLocalHistoryMenuItem);
        MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
        mcs.AddCommand(menuItem);

        // Create the command for the tool window
        CommandID toolwndCommandID = new CommandID(GuidList.guidLocalHistoryCmdSet, (int)PkgCmdIDList.cmdidLocalHistoryWindow);
        MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
        mcs.AddCommand(menuToolWin);
      }

      IVsSolution solution = (IVsSolution)GetService(typeof(SVsSolution));
      ErrorHandler.ThrowOnFailure(solution.AdviseSolutionEvents(this, out solutionCookie));
    }
    #endregion

    /// <summary>
    /// This function is the callback used to execute a command when the a menu item is clicked.
    /// See the Initialize method to see how the menu item is associated to this function using
    /// the OleMenuCommandService service and the MenuCommand class.
    /// </summary>
    private void MenuItemCallback(object sender, EventArgs e)
    {
      string projectItem = dte.SelectedItems.Item(1).ProjectItem.FileNames[0];

      List<DocumentNode> revisions = documentRepository.GetRevisions(projectItem);

      if (revisions.Count == 0)
      {
        IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
        Guid clsid = Guid.Empty;
        int result;
        Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                   0,
                   ref clsid,
                   "LocalHistory",
                   string.Format(CultureInfo.CurrentCulture, "No history found for {0}", projectItem),
                   string.Empty,
                   0,
                   OLEMSGBUTTON.OLEMSGBUTTON_OK,
                   OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                   OLEMSGICON.OLEMSGICON_INFO,
                   0,        // false
                   out result));

        return;
      }
      // Make sure the tool window is visible to the user
      ShowToolWindow(null, null);

      LocalHistoryControl control = (LocalHistoryControl)toolWindow.Content;

      // Remove revision from the revision list that belong to the previous document 
      control.DocumentItems.Clear();

      // Add the project item and its history to the revision list
      control.LatestDocument = new DocumentNode(projectItem, projectItem, Path.GetFileName(projectItem), DateTime.Now);
      foreach (DocumentNode revision in revisions) { control.DocumentItems.Add(revision); }
    }

    /// <summary>
    /// This function is called when the user clicks the menu item that shows the 
    /// tool window. See the Initialize method to see how the menu item is associated to 
    /// this function using the OleMenuCommandService service and the MenuCommand class.
    /// </summary>
    private void ShowToolWindow(object sender, EventArgs e)
    {
      if (toolWindow == null)
      {
        // Get the instance number 0 of this tool window. This window is single instance so this instance
        // is actually the only one.
        // The last flag is set to true so that if the tool window does not exists it will be created.
        toolWindow = this.FindToolWindow(typeof(LocalHistoryToolWindow), 0, true);
        if ((null == toolWindow) || (null == toolWindow.Frame))
        {
          throw new NotSupportedException(Resources.CanNotCreateWindow);
        }

        // Provide the control with the Visual Studio Difference Service to compare files
        LocalHistoryControl control = (LocalHistoryControl)toolWindow.Content;
        control.DifferenceService = (IVsDifferenceService)GetService(typeof(SVsDifferenceService));

        // TODO: remove this
        // BUG: This will cause a null pointer exception if no solution is open when the user opens the tool window.
        documentRepository.Control = control;
      }

      // Make sure the tool window is visible to the user
      IVsWindowFrame windowFrame = (IVsWindowFrame)toolWindow.Frame;
      Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
    }

    /// <summary>
    /// When a solution is opened, this function creates a new <code>DocumentRepository</code> and 
    /// registers the <code>LocalHistoryDocumentListener</code> to listen for save events.
    /// </summary>
    public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
    {
      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering OnAfterOpenSolution() of: {0}", this.ToString()));

      dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
      if (dte == null) ErrorHandler.ThrowOnFailure(1);

      // The solution name can be empty if the user opens a file without opening a solution
      if (dte.Solution != null && dte.Solution.FullName.Length != 0)
      {
        IVsRunningDocumentTable documentTable = (IVsRunningDocumentTable)Package.GetGlobalService(typeof(SVsRunningDocumentTable));

        Debug.WriteLine(dte.Solution.FullName);

        // Create a new document repository for the solution
        string solutionDirectory = System.IO.Path.GetDirectoryName(dte.Solution.FullName);
        string repositoryDirectory = System.IO.Path.Combine(solutionDirectory, ".localhistory");
        documentRepository = new DocumentRepository(solutionDirectory, repositoryDirectory);

        // Create and register a document listener that will handle save events
        documentListener = new LocalHistoryDocumentListener(documentTable, documentRepository);
        documentTable.AdviseRunningDocTableEvents(documentListener, out rdtCookie);
      }

      return VSConstants.S_OK;
    }

    #region Unused IVsSolutionEvents

    public int OnAfterCloseSolution(
        Object pUnkReserved)
    {
      return VSConstants.S_OK;
    }

    public int OnAfterLoadProject(
        IVsHierarchy pStubHierarchy,
        IVsHierarchy pRealHierarchy)
    {
      return VSConstants.S_OK;
    }

    public int OnAfterOpenProject(
        IVsHierarchy pHierarchy,
        int fAdded)
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeCloseProject(
        IVsHierarchy pHierarchy,
        int fRemoved)
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeCloseSolution(
        Object pUnkReserved)
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeUnloadProject(
        IVsHierarchy pRealHierarchy,
        IVsHierarchy pStubHierarchy)
    {
      return VSConstants.S_OK;
    }

    public int OnQueryCloseProject(
        IVsHierarchy pHierarchy,
        int fRemoving,
        ref int pfCancel)
    {
      pfCancel = VSConstants.S_OK;

      return VSConstants.S_OK;
    }

    public int OnQueryCloseSolution(
        Object pUnkReserved,
        ref int pfCancel)
    {
      pfCancel = VSConstants.S_OK;

      return VSConstants.S_OK;
    }

    public int OnQueryUnloadProject(
        IVsHierarchy pRealHierarchy,
        ref int pfCancel)
    {
      pfCancel = VSConstants.S_OK;

      return VSConstants.S_OK;
    }

    #endregion
  }
}
