using KE.StringReplaceWithI.Properties;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace KE.StringReplaceWithI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
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
            using (StreamReader sr = new StreamReader(path, GetEncoding(path)))
            {
                CmpTbFileContent.Text = sr.ReadToEnd();
                CmpLbFilePath.Content = openFileDialog.FileName;
            }
        }
    }

    private async void CmpBtnSave_OnClick(object sender, RoutedEventArgs e)
    {
        if (File.Exists(CmpLbFilePath.Content.ToString()))
        {
            string path = CmpLbFilePath.Content.ToString();
            using (StreamWriter sw = new StreamWriter(path, false, GetEncoding(path)))
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
            using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.Default))
            {
                await sw.WriteAsync(CmpTbFileContent.Text);
            }
        }
    }

    private void CmpBtnReplace_OnClick(object sender, RoutedEventArgs e)
    {
        string[] finds = CmpTbFind.Text.Split('@');
        string[] replaces = CmpTbReplace.Text.Split('@');
        string text = CmpTbFileContent.Text;

        if (finds.Length != replaces.Length)
        {
            throw new InvalidDataException("Mis-match");
        }

        int occurences = 0;
        for (int i = 0; i < finds.Length; i++)
        {
            Regex regex = new Regex(finds[i]);
            MatchCollection matches = regex.Matches(text);

            int iterator = matches.Count;
            foreach (Match match in matches.Cast<Match>().Reverse())
            {
                text = text.Substring(0, match.Index) +
                       replaces[i].Replace("\\i", iterator.ToString()) +
                       text.Substring(match.Index + match.Length);

                iterator--;
                occurences++;
            }
        }

        CmpTbFileContent.Text = text;

        MessageBox.Show(string.Format("{0} occurence(s) replaced.", occurences), "Find and Replace",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Determines a text file's encoding by analyzing its byte order mark (BOM).
    /// Defaults to ASCII when detection of the text file's endianness fails.
    /// </summary>
    /// <param name="filename">The text file to analyze.</param>
    /// <returns>The detected encoding.</returns>
    private static Encoding GetEncoding(string filename)
    {
        // Read the BOM
        var bom = new byte[4];
        using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
            file.Read(bom, 0, 4);
        }

        // Analyze the BOM
        if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76)
            return Encoding.UTF7;
        if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
            return Encoding.UTF8;
        if (bom[0] == 0xff && bom[1] == 0xfe)
            return Encoding.Unicode; //UTF-16LE
        if (bom[0] == 0xfe && bom[1] == 0xff)
            return Encoding.BigEndianUnicode; //UTF-16BE
        if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)
            return Encoding.UTF32;
        return Encoding.Default;
    }
}
