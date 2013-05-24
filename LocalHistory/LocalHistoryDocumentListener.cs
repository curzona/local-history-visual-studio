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
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Intel.LocalHistory
{
  class LocalHistoryDocumentListener : IVsRunningDocTableEvents3
  {
    private readonly IVsRunningDocumentTable documentTable;
    private readonly DocumentRepository documentRepository;

    public LocalHistoryDocumentListener(IVsRunningDocumentTable documentTable, DocumentRepository documentRepository)
    {
      this.documentTable = documentTable;
      this.documentRepository = documentRepository;
    }

    /// <summary>
    /// When this event is triggered on a project item, a copy of the file is saved to the <code>DocumentRepository</code>.
    /// </summary>
    public int OnBeforeSave(
     uint docCookie
    )
    {
      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering OnBeforeSave() of: {0}", this.ToString()));

      uint pgrfRDTFlags, pdwReadLocks, pdwEditLocks;
      string pbstrMkDocument; IVsHierarchy ppHier;
      uint pitemid; IntPtr ppunkDocData;
      documentTable.GetDocumentInfo(docCookie, out pgrfRDTFlags, out pdwReadLocks, out pdwEditLocks, out pbstrMkDocument, out ppHier, out pitemid, out ppunkDocData);

      // Only save revisions of file in the current solution directory
      if (pbstrMkDocument.StartsWith(documentRepository.SolutionDirectory))
      {
        documentRepository.CreateRevision(pbstrMkDocument);
      }

      return VSConstants.S_OK;
    }

    #region Unused IVsRunningDocTableEvents3

    public int OnAfterAttributeChange(
     uint docCookie,
     uint grfAttribs
    )
    {
      return VSConstants.S_OK;
    }

    public int OnAfterAttributeChangeEx(
        uint docCookie,
        uint grfAttribs,
        IVsHierarchy pHierOld,
        uint itemidOld,
        string pszMkDocumentOld,
        IVsHierarchy pHierNew,
        uint itemidNew,
        string pszMkDocumentNew
    )
    {
      return VSConstants.S_OK;
    }

    public int OnAfterDocumentWindowHide(
        uint docCookie,
        IVsWindowFrame pFrame
    )
    {
      return VSConstants.S_OK;
    }

    public int OnAfterFirstDocumentLock(
     uint docCookie,
     uint dwRDTLockType,
     uint dwReadLocksRemaining,
     uint dwEditLocksRemaining
    )
    {
      return VSConstants.S_OK;
    }

    public int OnAfterSave(
     uint docCookie
    )
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeDocumentWindowShow(
     uint docCookie,
     int fFirstShow,
     IVsWindowFrame pFrame
    )
    {
      return VSConstants.S_OK;
    }


    public int OnBeforeLastDocumentUnlock(
     uint docCookie,
     uint dwRDTLockType,
     uint dwReadLocksRemaining,
     uint dwEditLocksRemaining
    )
    {
      return VSConstants.S_OK;
    }

    #endregion
  }
}
