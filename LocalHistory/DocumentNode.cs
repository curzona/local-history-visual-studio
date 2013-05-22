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

using System;
using System.Globalization;

namespace Intel.LocalHistory
{
  public class DocumentNode
  {
    private readonly string repositoryPath;
    private readonly string originalPath;
    private readonly string fileName;
    private readonly DateTime time;

    public DocumentNode(string repositoryPath, string originalPath, string fileName, DateTime time)
    {
      this.repositoryPath = repositoryPath;
      this.originalPath = originalPath;
      this.fileName = fileName;
      this.time = time;
    }

    public string RepositoryPath { get { return repositoryPath; } }

    public string OriginalPath { get { return originalPath; } }

    public string FileName { get { return fileName; } }

    public string TimeStamp { get { return time.ToString(CultureInfo.CurrentCulture); } }
  }
}
