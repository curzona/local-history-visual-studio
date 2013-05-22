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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Intel.LocalHistory
{
  class DocumentRepository
  {
    // Epoch used for converting to unix time.
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1);

    public string SolutionDirectory { get; set; }

    public string RepositoryDirectory { get; set; }

    // TODO: TODO: remove this
    public LocalHistoryControl Control { get; set; }

    /// <summary>
    /// Creates a new <code>DocumentRepository</code> for the given solution and repository.
    /// </summary>
    public DocumentRepository(string solutionDirectory, string repositoryDirectory)
    {
      SolutionDirectory = solutionDirectory;
      RepositoryDirectory = repositoryDirectory;
    }

    /// <summary>
    /// Creates a new new revision in the repository for the given project item.
    /// </summary>
    public DocumentNode CreateRevision(string filePath)
    {
      if (filePath == null) throw new ArgumentNullException("filePath");
      if (!filePath.StartsWith(SolutionDirectory)) throw new ArgumentException("filePath is outside of the current Workspace");

      DocumentNode newNode = null;

      try
      {
        DateTime dateTime = DateTime.Now;
        newNode = GetRevision(filePath, dateTime);

        // Create the parent directory if it doesn't exist
        string dirPath = Path.GetDirectoryName(newNode.RepositoryPath);
        if (!File.Exists(dirPath))
        {
          Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "In GetRevision() of: {0} creating {1}", this.ToString(), dirPath));

          System.IO.Directory.CreateDirectory(dirPath);
        }

        // Copy the file to the repository
        File.Copy(filePath, newNode.RepositoryPath, true);

        // TODO: TODO: remove this
        if (Control != null && Control.LatestDocument.OriginalPath.Equals(newNode.OriginalPath))
        {
          Control.DocumentItems.Insert(0, newNode);
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
      }

      return newNode;
    }

    /// <summary>
    /// Returns a DocumentNode object for the given project item and datetime.
    /// </summary>
    public DocumentNode GetRevision(string filePath, DateTime dateTime)
    {
      if (filePath == null) throw new ArgumentNullException("filePath");
      if (!filePath.StartsWith(SolutionDirectory)) throw new ArgumentException("filePath is outside of the current Workspace");

      string unixTime = ToUnixTime(dateTime).ToString();
      string fileName = Path.GetFileName(filePath);
      string relativePath = filePath.Replace(SolutionDirectory + "\\", "");
      string dirPath = Path.GetDirectoryName(relativePath);
      string newPath = Path.Combine(RepositoryDirectory, dirPath, unixTime + "$" + fileName);

      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "In GetRevision() of: {0} relativePath = {1}", this.ToString(), relativePath));
      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "In GetRevision() of: {0} newPath = {1}", this.ToString(), newPath));

      return new DocumentNode(newPath, filePath, fileName, dateTime);
    }

    /// <summary>
    /// Returns all DocumentNode objects in the repository for the given project item.
    /// </summary>
    public List<DocumentNode> GetRevisions(string filePath)
    {
      if (filePath == null) throw new ArgumentNullException("filePath");
      if (!filePath.StartsWith(SolutionDirectory)) throw new ArgumentException("filePath is outside of the current Workspace");

      string relativePath = filePath.Replace(SolutionDirectory + "\\", "");
      string newPath = Path.Combine(RepositoryDirectory, relativePath);
      string dirPath = Path.GetDirectoryName(newPath);
      string fileName = Path.GetFileName(newPath);

      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "In GetRevisions() of: {0} relativePath = {1}", this.ToString(), relativePath));
      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "In GetRevisions() of: {0} newPath = {1}", this.ToString(), newPath));
      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "In GetRevisions() of: {0} dirPath = {1}", this.ToString(), dirPath));

      string[] files = Directory.GetFiles(dirPath);
      List<DocumentNode> copies = new List<DocumentNode>();

      foreach (string file in files)
      {
        if (file.StartsWith(dirPath) && file.EndsWith(fileName))
        {
          Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "In GetRevisions() of: {0} found {1}", this.ToString(), file));
          copies.Add(GetDocumentNode(file));
        }
        else
        {
          Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "In GetRevisions() of: {0} skipping {1}", this.ToString(), file));
        }
      }

      copies.Reverse();

      return copies;
    }

    /// <summary>
    /// Returns a DocumentNode from the given repository item.
    /// </summary>
    public DocumentNode GetDocumentNode(string filePath)
    {
      if (filePath == null) throw new ArgumentNullException("filePath");
      if (!filePath.StartsWith(RepositoryDirectory)) throw new ArgumentException("filePath is outside of the current Repository");

      string[] parts = Path.GetFileName(filePath).Split('$');
      string fileName = parts[1];

      string dirPath = Path.GetDirectoryName(filePath.Replace(RepositoryDirectory + "\\", ""));
      string originalPath = Path.Combine(SolutionDirectory, dirPath, fileName);

      DateTime dateTime = ToDateTime(Convert.ToInt64(parts[0]));

      return new DocumentNode(filePath, originalPath, fileName, dateTime);
    }

    private DateTime ToDateTime(long unixTime)
    {
      return Epoch.ToLocalTime().AddSeconds(unixTime);
    }

    private long ToUnixTime(DateTime dateTime)
    {
      return (long)(dateTime - Epoch.ToLocalTime()).TotalSeconds;
    }
  }
}
