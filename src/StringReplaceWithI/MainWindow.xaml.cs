using KE.StringReplaceWithI.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace KE.StringReplaceWithI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Encoding encoding = Encoding.UTF8;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is initialized. Loads settings for the find and replace text boxes.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        CmpTbFind.Text = Settings.Default.Find;
        CmpTbReplace.Text = Settings.Default.Replace;
    }

    /// <summary>
    /// Invoked when the application is closing. Saves the current settings for the find and replace text boxes.
    /// </summary>
    /// <param name="e">The event arguments that can cancel the closing event.</param>
    protected override void OnClosing(CancelEventArgs e)
    {
        Settings.Default.Find = CmpTbFind.Text;
        Settings.Default.Replace = CmpTbReplace.Text;
        Settings.Default.Save();

        base.OnClosing(e);
    }

    /// <summary>
    /// Handles the Click event of the New button. Resets the state of the application.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void CmpBtnNew_OnClick(object sender, RoutedEventArgs e)
    {
        encoding = Encoding.UTF8;

        CmpTbFileContent.Text = string.Empty;
        CmpLbFilePath.Content = string.Empty;
    }

    /// <summary>
    /// Handles the Click event of the Open button. Opens a file dialog to select a file and loads its content.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void CmpBtnOpen_OnClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "All Files (*.*)|*.*";
        openFileDialog.Multiselect = false;
        if (openFileDialog.ShowDialog(this) == true)
        {
            string path = openFileDialog.FileName;
            using (StreamReader sr = new StreamReader(path, true))
            {
                encoding = sr.CurrentEncoding;
                CmpTbFileContent.Text = sr.ReadToEnd();
                CmpLbFilePath.Content = openFileDialog.FileName;
            }
        }
    }

    /// <summary>
    /// Handles the Click event of the Save button. Saves the current content to the file if it exists, otherwise prompts the Save As dialog.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void CmpBtnSave_OnClick(object sender, RoutedEventArgs e)
    {
        if (File.Exists(CmpLbFilePath.Content?.ToString()))
        {
            string path = CmpLbFilePath.Content.ToString();
            using (StreamWriter sw = new StreamWriter(path, false, encoding))
            {
                await sw.WriteAsync(CmpTbFileContent.Text);
            }
        }
        else
        {
            CmpBtnSaveAs_OnClick(this, new RoutedEventArgs());
        }
    }

    /// <summary>
    /// Handles the Click event of the Save As button. Prompts a Save File dialog to save the current content to a specified file.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void CmpBtnSaveAs_OnClick(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "All Files (*.*)|*.*";
        if (saveFileDialog.ShowDialog(this) == true)
        {
            using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false, encoding))
            {
                await sw.WriteAsync(CmpTbFileContent.Text);
            }
        }
    }

    /// <summary>
    /// Handles the Click event of the Replace button. Performs find and replace operation with iterator support.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void CmpBtnReplace_OnClick(object sender, RoutedEventArgs e)
    {
        // Extract iterators from replace string
        string iteratorPattern = @"\\i(\{(-?\d+),\s?(\d+),\s?([+-])\})?"; // \i{init, step, sign}
        Regex iteratorRegex = new Regex(iteratorPattern);
        MatchCollection iteratorMatches = iteratorRegex.Matches(CmpTbReplace.Text);

        List<Iterator> iterators = new();
        foreach (Match match in iteratorMatches)
        {
            if (match.Success && match.Groups.Count == 5)
            {
                if (string.IsNullOrEmpty(match.Groups[1].Value))
                {
                    iterators.Add(new());
                }
                else
                {
                    iterators.Add(new(int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value), match.Groups[4].Value[0]));
                }
            }
        }

        // Replace all occurences of searched string
        int count = 0;
        CmpTbFileContent.Text = Regex.Replace(CmpTbFileContent.Text, CmpTbFind.Text, (match) =>
        {
            count++;

            int iteratorIdx = 0;
            return Regex.Replace(CmpTbReplace.Text, iteratorPattern, (match) =>
            {
                Iterator iterator = iterators[iteratorIdx++];
                iterator.MoveNext();
                return iterator.Current.ToString();
            });
        });

        MessageBox.Show(string.Format("{0} occurence(s) replaced.", count), "Find and Replace",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Represents an iterator that can increment or decrement a value by a specified step.
    /// </summary>
    private class Iterator
    {
        /// <summary>
        /// Gets the initial value for the iterator.
        /// </summary>
        public int Init { get; }

        /// <summary>
        /// Gets the step value by which the iterator is incremented or decremented.
        /// </summary>
        public int Step { get; }

        /// <summary>
        /// Gets the sign that indicates whether the iterator increments ('+') or decrements ('-') the value.
        /// </summary>
        public char Sign { get; }

        /// <summary>
        /// Gets or sets the current value of the iterator. If null, the iterator has not started yet.
        /// </summary>
        public int? Current { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator"/> class with the specified initial value, step, and sign.
        /// </summary>
        /// <param name="init">The initial value of the iterator. Default value is 1.</param>
        /// <param name="step">The step value for each iteration. Default value is 1.</param>
        /// <param name="sign">The sign that determines the direction of the iteration. Must be either '+' or '-'. Default value is '+'.</param>
        /// <exception cref="ArgumentException">Thrown when the sign is neither '+' nor '-'.</exception>
        public Iterator(int init = 1, int step = 1, char sign = '+')
        {
            if (sign != '+' && sign != '-')
            {
                throw new ArgumentException("Invalid sign!");
            }

            Init = init;
            Step = step;
            Sign = sign;
        }

        /// <summary>
        /// Moves the iterator to the next value based on the step and sign.
        /// </summary>
        public void MoveNext()
        {
            if (Current == null)
            {
                Current = Init;
            }
            else
            {
                switch (Sign)
                {
                    case '+':
                        Current += Step;
                        break;
                    case '-':
                        Current -= Step;
                        break;
                }
            }
        }
    }
}
