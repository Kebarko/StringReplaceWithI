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

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        CmpTbFind.Text = Settings.Default.Find;
        CmpTbReplace.Text = Settings.Default.Replace;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        Settings.Default.Find = CmpTbFind.Text;
        Settings.Default.Replace = CmpTbReplace.Text;
        Settings.Default.Save();

        base.OnClosing(e);
    }

    private void CmpBtnNew_OnClick(object sender, RoutedEventArgs e)
    {
        encoding = Encoding.UTF8;

        CmpTbFileContent.Text = string.Empty;
        CmpLbFilePath.Content = string.Empty;
    }

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

    private class Iterator
    {
        public int Init { get; }
        public int Step { get; }
        public char Sign { get; }

        public int? Current { get; set; }

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
