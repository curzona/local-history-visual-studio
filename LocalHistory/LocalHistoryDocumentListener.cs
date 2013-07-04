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

using Intel.LocalHistory.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Intel.LocalHistory
{
  class LocalHistoryDocumentListener : IVsRunningDocTableEvents3Adapter
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
    public override int OnBeforeSave(
     uint docCookie
    )
    {
      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering OnBeforeSave() of: {0}", this.ToString()));

      uint pgrfRDTFlags, pdwReadLocks, pdwEditLocks;
      string pbstrMkDocument; IVsHierarchy ppHier;
      uint pitemid; IntPtr ppunkDocData;
      documentTable.GetDocumentInfo(docCookie, out pgrfRDTFlags, out pdwReadLocks, out pdwEditLocks, out pbstrMkDocument, out ppHier, out pitemid, out ppunkDocData);

      documentRepository.CreateRevision(pbstrMkDocument);

      return VSConstants.S_OK;
    }
  }
}
