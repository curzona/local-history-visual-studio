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

namespace Intel.LocalHistory.Utilities
{
  class IVsRunningDocTableEvents3Adapter : IVsRunningDocTableEvents3
  {
    public virtual int OnBeforeSave(
     uint docCookie
    )
    {
      return VSConstants.S_OK;
    }

    public virtual int OnAfterAttributeChange(
     uint docCookie,
     uint grfAttribs
    )
    {
      return VSConstants.S_OK;
    }

    public virtual int OnAfterAttributeChangeEx(
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

    public virtual int OnAfterDocumentWindowHide(
        uint docCookie,
        IVsWindowFrame pFrame
    )
    {
      return VSConstants.S_OK;
    }

    public virtual int OnAfterFirstDocumentLock(
     uint docCookie,
     uint dwRDTLockType,
     uint dwReadLocksRemaining,
     uint dwEditLocksRemaining
    )
    {
      return VSConstants.S_OK;
    }

    public virtual int OnAfterSave(
     uint docCookie
    )
    {
      return VSConstants.S_OK;
    }

    public virtual int OnBeforeDocumentWindowShow(
     uint docCookie,
     int fFirstShow,
     IVsWindowFrame pFrame
    )
    {
      return VSConstants.S_OK;
    }


    public virtual int OnBeforeLastDocumentUnlock(
     uint docCookie,
     uint dwRDTLockType,
     uint dwReadLocksRemaining,
     uint dwEditLocksRemaining
    )
    {
      return VSConstants.S_OK;
    }
  }
}
