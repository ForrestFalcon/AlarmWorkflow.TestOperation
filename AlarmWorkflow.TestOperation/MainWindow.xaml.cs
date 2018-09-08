using AlarmWorkflow.Shared.Core;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Mustache;
using PdfSharp;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace AlarmWorkflow.TestOperation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel(this);
        }

        #endregion
    }
}
