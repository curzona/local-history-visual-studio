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

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System;

namespace Intel.LocalHistory
{
  /// <summary>
  /// Interaction logic for MyControl.xaml
  /// </summary>
  public partial class LocalHistoryControl : UserControl, INotifyPropertyChanged
  {

    public ObservableCollection<DocumentNode> DocumentItems { get; set; }

    public bool HasHistory { get { return DocumentItems.Count != 0; } }

    public DocumentNode LatestDocument { get; set; }

    private IVsDifferenceService differenceService;

    private IVsWindowFrame differenceFrame;

    public LocalHistoryControl()
    {
      InitializeComponent();

      DocumentItems = new ObservableCollection<DocumentNode>();

      // PropertyChanged event propagation
      DocumentItems.CollectionChanged += (o, e) =>
      {
        OnPropertyChanged("DocumentItems");
        OnPropertyChanged("HasHistory");
      };

      // Set the DataContext for binding properties
      DocumentListBox.DataContext = this;
    }

    /// <summary>
    /// Opens a difference window with <code>IVsDifferenceService</code> when a DocumentNode is double clicked.
    /// </summary>
    private void MouseDoubleClickHandler(object sender, MouseButtonEventArgs e)
    {
      if (DocumentListBox.SelectedItem == null)
        return;

      DocumentNode node = ((DocumentNode)DocumentListBox.SelectedItem);

      Debug.Assert(File.Exists(node.RepositoryPath));
      Debug.Assert(File.Exists(node.OriginalPath));
      Debug.Assert(node.OriginalPath == LatestDocument.OriginalPath);

      // Close the last comparison because we only want 1 open at a time
      if (differenceFrame != null) differenceFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);

      // Get the Difference Service we will use to do the comparison
      if (differenceService == null)
      {
        differenceService = (IVsDifferenceService)Package.GetGlobalService(typeof(SVsDifferenceService));
      }

      // Open a comparison between the old file and the current file
      differenceFrame = differenceService.OpenComparisonWindow2(
        node.RepositoryPath, LatestDocument.RepositoryPath,
        node.FileName + " " + node.TimeStamp + " vs  Now",
        node.FileName + " " + node.TimeStamp + " vs  Now",
        node.FileName + " " + node.TimeStamp,
        LatestDocument.FileName + " Now",
        node.FileName + " " + node.TimeStamp + " vs  Now",
        null,
        0);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string propertyName)
    {
      if (this.PropertyChanged != null)
      {
        this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }
}