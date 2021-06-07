
using LibCommon;
using LibCommon.Extension_methods;
using LibCommon.Forms;
using LibCommon.Helpers;
using LibCommon.Modules;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BankRev
{
  internal class FormMain : Form
  {
    private const int SkipRefreshUITicksMax = 500;
    private int fileToProcessCount = 0;
    private int fileProcessedCount = 0;
    private SearchOption _optSearchOption = SearchOption.TopDirectoryOnly;
    private bool _optOverride = false;
    private bool _optUnbin = false;
    private bool _optDestCustom = false;
    private bool _optPrefix = false;
    private int fileErrorCount = 0;
    private int fileSuccessCount = 0;
    private int fileTableSelectedItem = -1;
    private int fileTableContextMenuSelectedIndex = -1;
    private static FormMain.AppState ApplicationState = FormMain.AppState.Unchanged;
    private bool bFormReady = false;
    private bool bSkipRefreshUI = false;
    private int SkipRefreshUITicks = 500;
    private static Color foregroundOK = Color.Black;
    private static Color backgroundOK = Color.White;
    private static Color foregroundUNK = Color.Orange;
    private static Color backgroundUNK = Color.White;
    private static Color foregroundNOK = Color.Red;
    private static Color backgroundNOK = Color.White;
    private static Color foregroundExcluded = Color.Gray;
    private static Color foregroundIncluded = Color.Black;
    private static Color foregroundProceededOK = Color.DarkGreen;
    private static Color foregroundProceededNOK = Color.DarkRed;
    private int _currentFileIndex = -1;
    internal static string CurrentFilePath = "";
    private IContainer components = (IContainer) null;
    private ToolTip helpTip;
    private Timer refreshUI;
    private ContextMenuStrip fileTableContextMenu;
    private Button btnCon;
    private RichTextBox stdoutBox;
    private CheckBox optSubDirs;
    private Label fileCount;
    private Button browseSource;
    private Label icoStatus;
    private ListView fileTable;
    private ColumnHeader colFileName;
    private ColumnHeader colFileType;
    private ColumnHeader colFilePath;
    private ColumnHeader colFileSize;
    private Label textStatus;
    private ProgressBar barStatus;
    private Button btnInfo;
    private Button btnHelp;
    private Button btnValidate;
    private OpenFileDialog selectKeyFile;
    private ToolStripMenuItem openInExplorerToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripMenuItem selectAllToolStripMenuItem;
    private ToolStripMenuItem invertSelectionToolStripMenuItem;
    private ToolStripMenuItem checkAllToolStripMenuItem;
    private ToolStripMenuItem uncheckAllToolStripMenuItem;
    private GroupBox groupOptions;
    private CheckBox optOverride;
    private Label label1;
    private TextBox optDestDir;
    private Button browse;
    private CheckBox optDest;
    private Button resetList;
    private CheckBox optUnbin;
    private CheckBox optPrefix;
    private ColumnHeader colFileVersion;
    private ColumnHeader colFileProduct;
    private ColumnHeader colFilePrefix;
    private ToolStripMenuItem showDetailsToolStripMenuItem;

    public FormMain() => this.InitializeComponent();

    private void formMain_Load(object sender, EventArgs e)
    {
      this.optOverride.Checked = this.optDest.Checked = this.optUnbin.Checked = this.optSubDirs.Checked = this.optPrefix.Checked = false;
      this.optOverride.Checked = Config.RegGet(Program.AppName, "optOverride", "false").ToBool();
      this._optDestCustom = this.optDest.Checked = this.optDestDir.Enabled = Config.RegGet(Program.AppName, "optDest", "false").ToBool();
      this.optUnbin.Checked = true;
      this.optSubDirs.Checked = Config.RegGet(Program.AppName, "optSubDirs", "false").ToBool();
      this.optPrefix.Checked = Config.RegGet(Program.AppName, "optPrefix", "false").ToBool();
      this.optDestDir.Text = Config.RegGet(Program.AppName, "optDestDir", "");
      this.btnValidate.Enabled = false;
      this.fileTable.ListViewItemSorter = (IComparer) new ListViewColumnSorter();
      this.bFormReady = true;
    }

    private void UpdateOptions()
    {
      if (!this.bFormReady)
        return;
      CheckBox[] checkBoxArray = new CheckBox[5]
      {
        this.optOverride,
        this.optDest,
        this.optUnbin,
        this.optSubDirs,
        this.optPrefix
      };
      foreach (CheckBox checkBox in checkBoxArray)
        Config.RegSet(Program.AppName, checkBox.Name, checkBox.Checked.ToString());
      this._optDestCustom = this.optDestDir.Enabled = this.optDest.Checked;
    }

    private void EnableUi()
    {
      this.btnValidate.Text = "&Process files";
      this.btnValidate.Enabled = true;
      this.groupOptions.Visible = true;
      this.browseSource.Visible = true;
      this.optSubDirs.Visible = true;
      this.fileTable.Cursor = Cursors.Default;
      this.fileTable.Width = 597;
    }

    private void DisableUi(bool cancelButton = true)
    {
      if (cancelButton)
        this.btnValidate.Text = "&Cancel";
      this.btnValidate.Enabled = cancelButton;
      this.groupOptions.Visible = false;
      this.browseSource.Visible = false;
      this.optSubDirs.Visible = false;
      this.fileTable.Width = 796;
    }

    private void btnHelp_Click(object sender, EventArgs e) => Process.Start("https://community.bistudio.com/wiki/BankRev");

    private void browseSource_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
      {
        SelectedPath = Config.RegGet(Program.AppName, "optLastDir", ""),
        ShowNewFolderButton = false
      };
      if (folderBrowserDialog.ShowDialog() == DialogResult.OK && folderBrowserDialog.SelectedPath.IsDirectory())
        this.ScanFolder(folderBrowserDialog.SelectedPath);
      Config.RegSet(Program.AppName, "optLastDir", folderBrowserDialog.SelectedPath);
    }

    private void AddFile(string item)
    {
      int i = 0;
      int duplicates = 0;
      this.AddFile(item, ref duplicates, ref i);
    }

    private void AddFile(string item, ref int duplicates, ref int i)
    {
      string withoutExtension = Path.GetFileNameWithoutExtension(item);
      string extension = Path.GetExtension(item);
      string fullPath = Path.GetFullPath(item);
      string formattedFileSize = item.GetFormattedFileSize();
      bool flag1 = false;
      bool flag2 = extension == ".pbo";
      for (int index = 0; index < this.fileTable.Items.Count; ++index)
      {
        if (!(fullPath != this.fileTable.Items[index].SubItems[2].Text))
        {
          ++duplicates;
          flag1 = true;
        }
      }
      if (!flag2 || flag1)
        return;
      PBO pbo = new PBO(item);
      string[] items = new string[7]
      {
        withoutExtension,
        extension,
        fullPath,
        formattedFileSize,
        pbo.Product,
        pbo.Version,
        pbo.Prefix
      };
      pbo.Dispose();
      ListViewItem listViewItem = new ListViewItem(items);
      this.fileTable.Items.Add(listViewItem);
      listViewItem.Checked = true;
      ++i;
    }

    internal void ScanFolder(string pPath)
    {
      int i = 0;
      int duplicates = 0;
      this.DisplayStatus(string.Format("Searching for files to add from {0}...", (object) pPath), FormMain.AppState.Busy);
      foreach (string file in FileSystem.GetFiles(pPath, "*.*", this._optSearchOption))
        this.AddFile(file, ref duplicates, ref i);
      if (i == 0 && duplicates == 0)
      {
        this.DisplayStatus("No valid file was found", FormMain.AppState.Ready);
        int num = (int) MessageBox.Show("No valid file was found", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
      }
      else if (duplicates > 0)
        this.DisplayStatus(string.Format("Added: {0} ({1} duplicate(s))", (object) i, (object) duplicates), FormMain.AppState.Ready);
      else
        this.DisplayStatus(string.Format("Added: {0}", (object) i), FormMain.AppState.Ready);
    }

    private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem listViewItem in this.fileTable.Items)
        listViewItem.Selected = true;
    }

    private void invertSelectionToolStripMenuItem_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem listViewItem in this.fileTable.Items)
        listViewItem.Selected = !listViewItem.Selected;
    }

    private void checkAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem listViewItem in this.fileTable.Items)
        listViewItem.Checked = true;
    }

    private void uncheckAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem listViewItem in this.fileTable.Items)
        listViewItem.Checked = false;
    }

    private void fileTable_DragDrop(object sender, DragEventArgs e)
    {
      foreach (string str in (string[]) e.Data.GetData(DataFormats.FileDrop, false))
      {
        if (str.IsDirectory())
          this.ScanFolder(str);
        else
          this.AddFile(str);
      }
    }

    private void DragDropInit(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        return;
      e.Effect = DragDropEffects.Copy;
    }

    private void fileTable_KeyDown(object sender, KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        case Keys.Delete:
          this.DeleteSelected();
          break;
        case Keys.D:
          if (e.Modifiers != Keys.Control)
            break;
          this.showDetailsToolStripMenuItem_Click(new object(), new EventArgs());
          break;
      }
    }

    private void fileTable_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      ListView listView = (ListView) sender;
      if (!(listView.ListViewItemSorter is ListViewColumnSorter listViewItemSorter) || e.Column == 3)
        return;
      if (e.Column == listViewItemSorter.SortColumn)
      {
        listViewItemSorter.ReverseSortOrder();
      }
      else
      {
        listViewItemSorter.SortColumn = e.Column;
        listViewItemSorter.Order = SortOrder.Descending;
      }
      listView.Sort();
    }

    private void DeleteSelected()
    {
      foreach (ListViewItem selectedItem in this.fileTable.SelectedItems)
        this.fileTable.Items.Remove(selectedItem);
    }

    private void fileTable_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Right || !this.fileTable.FocusedItem.Bounds.Contains(e.Location))
        return;
      int index = this.fileTable.HitTest(e.X, e.Y).Item.Index;
      if (index >= 0)
      {
        this._currentFileIndex = index;
        string text = this.fileTable.Items[this._currentFileIndex].SubItems[2].Text;
        if (!File.Exists(text))
        {
          this.DisplayStatus(string.Format("Missing file: {0}", (object) text), FormMain.AppState.Failed);
          this.fileTable.Items[this._currentFileIndex].SubItems[0].ForeColor = FormMain.foregroundNOK;
          this.fileTable.Items[this._currentFileIndex].Checked = false;
        }
        else
        {
          FormMain.CurrentFilePath = text;
          this.fileTable.Items[this._currentFileIndex].SubItems[0].ForeColor = FormMain.foregroundOK;
        }
        this.openInExplorerToolStripMenuItem.Enabled = this.showDetailsToolStripMenuItem.Enabled = File.Exists(text);
      }
      else
        this._currentFileIndex = -1;
      this.fileTableContextMenu.Show(Cursor.Position);
    }

    private void resetList_Click(object sender, EventArgs e) => this.fileTable.Items.Clear();

    private void btnValidate_Click(object sender, EventArgs e)
    {
      if (FormMain.ApplicationState == FormMain.AppState.Working)
      {
        this.DisplayStatus("===========", FormMain.AppState.Failed);
        this.DisplayStatus("Cancellation requested", FormMain.AppState.Failed);
        this.DisplayStatus("The process will be cancelled after the current file", FormMain.AppState.Failed);
        this.DisplayStatus("===========", FormMain.AppState.Failed);
        FormMain.ApplicationState = FormMain.AppState.Canceled;
        this.btnValidate.Enabled = false;
      }
      else
      {
        if (this.btnValidate.Text.ToLower().IndexOf("cancel") > 0)
          return;
        if (FormMain.ApplicationState != FormMain.AppState.Working && this._optDestCustom && !this.optDestDir.Text.IsDirectory())
        {
          string text = string.Format("Cannot start - The destination directory doesn't exist:{0}{1}", (object) Environment.NewLine, (object) this.optDestDir.Text);
          Logger.Log.Error((object) text);
          int num = (int) MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        else
        {
          FormMain.ApplicationState = FormMain.AppState.Working;
          this.fileTable.Cursor = Cursors.No;
          this.barStatus.Maximum = this.fileToProcessCount;
          this.barStatus.Value = 0;
          this.fileProcessedCount = 0;
          this.fileErrorCount = 0;
          this.fileSuccessCount = 0;
          if (MessageBox.Show(string.Format(this.optDest.Checked ? "Every unpacked addon will be placed in to the given destination directory.{0}Do you want to proceed" : "Unpack addons will be placed in the same directory as their source.{0}Do you want to proceed", (object) Environment.NewLine), "Process the selected files?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.No)
          {
            this.EnableUi();
          }
          else
          {
            this.DisableUi();
            this.UnpackAddons(this.optUnbin.Checked);
            this.EnableUi();
          }
        }
      }
    }

    private void UnpackAddons(bool unbin)
    {
      Stopwatch stopwatch = new Stopwatch();
      for (int index = 0; index < this.fileTable.Items.Count; ++index)
      {
        if (FormMain.ApplicationState == FormMain.AppState.Canceled)
        {
          this.DisplayStatus("User cancel", FormMain.AppState.Failed);
          break;
        }
        this.icoStatus.Image = (Image) this.GetStateImage(FormMain.AppState.Working);
        string text1 = this.fileTable.Items[index].SubItems[0].Text;
        string text2 = this.fileTable.Items[index].SubItems[2].Text;
        if (!File.Exists(text2) && this.fileTable.Items[index].Checked)
        {
          ++this.fileErrorCount;
          this.DisplayStatus(string.Format("Fail to process {0} - Missing file", (object) text1), FormMain.AppState.Busy);
          this.fileTable.Items[index].SubItems[0].ForeColor = FormMain.foregroundProceededNOK;
        }
        if (this.fileTable.Items[index].Checked && File.Exists(text2))
        {
          stopwatch.Restart();
          PBO pbo = new PBO(text2);
          if (!pbo.Prefix.IsNullOrEmpty())
          {
            if (unbin)
            {
              Process process = new Process()
              {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
              };
              process.OutputDataReceived += new DataReceivedEventHandler(this.StdOutHandler);
              process.ErrorDataReceived += new DataReceivedEventHandler(this.StdErrHandler);
              this.DisplayStatus(string.Format("Processing {0} ({1})...", (object) this.fileTable.Items[index].SubItems[0].Text, (object) this.fileTable.Items[index].SubItems[3].Text), FormMain.AppState.Busy);
              process.StartInfo.Arguments = string.Format("/y /extract {0}{1}", (object) pbo.FilePath.AddQuote(), this._optDestCustom ? (object) (" " + this.optDestDir.Text.AddQuote()) : (object) "");
              Logger.Log.DebugFormat("{0}", (object) pbo.FilePath.AddQuote());
              try
              {
                process.Start();
              }
              catch (Exception ex)
              {
                Logger.Log.FatalFormat("{0}: {1}", (object) process.StartInfo.FileName, (object) ex.Message);
                this.EnableUi();
                throw;
              }
              while (!process.HasExited)
                Application.DoEvents();
              this.UpdateRow(index, this.PostExtractionOps(ref pbo), stopwatch.Elapsed);
              process.Dispose();
            }
            else
            {
              FileInfo fileInfo = pbo.FilePath.ToFileInfo();
              int num = Tools.BankRev.Extract((this._optDestCustom ? this.optDestDir.Text : fileInfo.DirectoryName).AddQuote(), pbo.FilePath.AddQuote());
              this.UpdateRow(index, num == 0, stopwatch.Elapsed);
            }
          }
          else
          {
            this.DisplayStatus(string.Format("Missing prefix, cannot process -  {0} ({1})", (object) this.fileTable.Items[index].SubItems[0].Text, (object) this.fileTable.Items[index].SubItems[3].Text), FormMain.AppState.Busy);
            ++this.fileErrorCount;
          }
          pbo.Dispose();
          this.barStatus.PerformStep();
          ++this.fileProcessedCount;
        }
      }
      if (this.fileSuccessCount > 0)
        this.DisplayStatus(string.Format("{0} file(s) successfully converted", (object) this.fileSuccessCount), FormMain.AppState.Succeeded);
      if (this.fileErrorCount > 0)
        this.DisplayStatus(string.Format("Failed to convert {0} file(s)", (object) this.fileErrorCount), FormMain.AppState.Failed);
      stopwatch.Stop();
      this.bSkipRefreshUI = true;
      this.barStatus.Maximum = 1;
      this.barStatus.Value = 1;
      this.btnValidate.Enabled = true;
      this.fileTable.Cursor = Cursors.Default;
      FormMain.ApplicationState = FormMain.AppState.Unchanged;
    }

    private void UpdateRow(int i, bool success, TimeSpan timeSpan)
    {
      if (success)
      {
        this.fileTable.Items[i].SubItems[0].ForeColor = FormMain.foregroundProceededOK;
        this.fileTable.Items[i].Checked = false;
        this.DisplayStatus(string.Format("{0} processed in {1}", (object) this.fileTable.Items[i].SubItems[0].Text, (object) timeSpan), FormMain.AppState.Busy);
        ++this.fileSuccessCount;
      }
      else
      {
        this.fileTable.Items[i].SubItems[0].ForeColor = FormMain.foregroundProceededNOK;
        this.DisplayStatus(string.Format("Fail to process {0} after {1}", (object) this.fileTable.Items[i].SubItems[0].Text, (object) timeSpan), FormMain.AppState.Busy);
        ++this.fileErrorCount;
      }
    }

    private bool PostExtractionOps(ref PBO pbo)
    {
      FileInfo fileInfo = pbo.FilePath.ToFileInfo();
      if (fileInfo.DirectoryName == null)
        return false;
      if (pbo.Prefix == null)
        return Path.Combine(fileInfo.DirectoryName, pbo.FileName).IsDirectory();
      if (!this._optDestCustom)
        return Path.Combine(fileInfo.DirectoryName, pbo.Prefix).IsDirectory();
      return this._optDestCustom && this.optDestDir.Text.IsDirectory() && Path.Combine(this.optDestDir.Text, pbo.Prefix).IsDirectory();
    }

    internal void StdOutHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
      if (string.IsNullOrEmpty(outLine.Data))
        ;
    }

    private void StdErrHandler(object sender, DataReceivedEventArgs e)
    {
      if (string.IsNullOrEmpty(e.Data))
        return;
      int num = (int) MessageBox.Show(e.Data);
    }

    private void browse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
      {
        ShowNewFolderButton = true,
        SelectedPath = this.optDestDir.Text.IsDirectory() ? this.optDestDir.Text : (string) null
      };
      if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
        return;
      this.optDestDir.Text = folderBrowserDialog.SelectedPath;
      this.optDest.Checked = true;
    }

    private void optDest_CheckedChanged(object sender, EventArgs e) => this.UpdateOptions();

    private void optOverride_CheckedChanged(object sender, EventArgs e) => this.UpdateOptions();

    private void optUnbin_CheckedChanged(object sender, EventArgs e) => this.UpdateOptions();

    private void optPrefix_CheckedChanged(object sender, EventArgs e) => this.UpdateOptions();

    private void optSubDirs_CheckedChanged(object sender, EventArgs e)
    {
      this._optSearchOption = this.optSubDirs.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
      this.UpdateOptions();
    }

    private void optDestDir_TextChanged(object sender, EventArgs e)
    {
      if (this.optDestDir.Text.IsDirectory())
      {
        this.optDestDir.ForeColor = FormMain.foregroundOK;
        this.optDestDir.BackColor = FormMain.backgroundOK;
        if (!this.bFormReady)
          return;
        Config.RegSet(Program.AppName, "optDestDir", this.optDestDir.Text);
      }
      else
      {
        this.optDestDir.ForeColor = FormMain.foregroundNOK;
        this.optDestDir.BackColor = FormMain.backgroundNOK;
      }
    }

    internal void DisplayStatus(string text)
    {
      this.stdoutBox.AppendText(text + Environment.NewLine);
      Logger.Log.Info((object) text);
      this.TextBoxScrollToEnd(this.stdoutBox);
    }

    internal void DisplayStatus(string text, FormMain.AppState state)
    {
      this.icoStatus.Image = (Image) this.GetStateImage(state);
      this.textStatus.Text = text;
      this.stdoutBox.AppendText(text + Environment.NewLine);
      Logger.Log.Info((object) text);
      this.TextBoxScrollToEnd(this.stdoutBox);
    }

    internal void DisplayStatus(string text, FormMain.AppState state, bool showStdOut)
    {
      if (showStdOut)
      {
        Logger.Log.Info((object) text);
        this.stdoutBox.AppendText(text + Environment.NewLine);
      }
      this.icoStatus.Image = (Image) this.GetStateImage(state);
      this.textStatus.Text = text;
    }

    internal void AppendStatus(string text)
    {
      Logger.Log.Info((object) text);
      this.stdoutBox.AppendText(text);
    }

    private void TextBoxScrollToEnd(RichTextBox _textBox)
    {
      _textBox.SelectionStart = _textBox.Text.Length;
      _textBox.ScrollToCaret();
    }

    private void refreshUI_Tick(object sender, EventArgs e)
    {
      if (!this.bSkipRefreshUI && this.SkipRefreshUITicks == 500)
      {
        int num = 0;
        for (int index = 0; index < this.fileTable.Items.Count; ++index)
        {
          if (this.fileTable.Items[index].Checked)
            ++num;
        }
        switch (num)
        {
          case 0:
            this.DisplayStatus("Nothing to do", FormMain.AppState.Succeeded, false);
            this.fileCount.Text = "The queue is empty";
            this.btnValidate.Enabled = false;
            break;
          case 1:
            this.fileCount.Text = string.Format("Queue to process: {0} file", (object) 1);
            this.DisplayStatus("Ready to proceed", FormMain.AppState.Ready, false);
            this.btnValidate.Enabled = true;
            break;
          default:
            this.fileCount.Text = string.Format("Queue to process: {0} files", (object) num);
            this.DisplayStatus("Ready to proceed", FormMain.AppState.Ready, false);
            this.btnValidate.Enabled = true;
            break;
        }
        if (num > 500)
          this.refreshUI.Interval = 200;
        if (num > 1000)
          this.refreshUI.Interval = 375;
        if (num > 1500)
          this.refreshUI.Interval = 500;
        this.fileToProcessCount = num;
      }
      else
      {
        --this.SkipRefreshUITicks;
        if (this.SkipRefreshUITicks < 1)
        {
          this.bSkipRefreshUI = false;
          this.SkipRefreshUITicks = 500;
        }
      }
    }

    internal Bitmap GetStateImage(FormMain.AppState state)
    {
      switch (state)
      {
        case FormMain.AppState.Ready:
        case FormMain.AppState.Succeeded:
          return Resources.img_taskSucceeded();
        case FormMain.AppState.Canceled:
          return Resources.img_taskCanceled();
        case FormMain.AppState.Busy:
        case FormMain.AppState.Working:
          return Resources.img_taskPlanned();
        default:
          return Resources.img_taskFailed();
      }
    }

    private void openInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this._currentFileIndex == -1)
        return;
      FileSystem.ViewInExplorer(this.fileTable.Items[this._currentFileIndex].SubItems[2].Text);
    }

    private void showDetailsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this._currentFileIndex == -1)
        return;
      int num = (int) new Details().ShowDialog();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new Container();
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (FormMain));
      this.helpTip = new ToolTip(this.components);
      this.optSubDirs = new CheckBox();
      this.btnCon = new Button();
      this.optOverride = new CheckBox();
      this.refreshUI = new Timer(this.components);
      this.fileTableContextMenu = new ContextMenuStrip(this.components);
      this.openInExplorerToolStripMenuItem = new ToolStripMenuItem();
      this.showDetailsToolStripMenuItem = new ToolStripMenuItem();
      this.toolStripSeparator1 = new ToolStripSeparator();
      this.selectAllToolStripMenuItem = new ToolStripMenuItem();
      this.invertSelectionToolStripMenuItem = new ToolStripMenuItem();
      this.checkAllToolStripMenuItem = new ToolStripMenuItem();
      this.uncheckAllToolStripMenuItem = new ToolStripMenuItem();
      this.stdoutBox = new RichTextBox();
      this.fileCount = new Label();
      this.icoStatus = new Label();
      this.fileTable = new ListView();
      this.colFileName = new ColumnHeader();
      this.colFileType = new ColumnHeader();
      this.colFilePath = new ColumnHeader();
      this.colFileSize = new ColumnHeader();
      this.colFileProduct = new ColumnHeader();
      this.colFileVersion = new ColumnHeader();
      this.colFilePrefix = new ColumnHeader();
      this.textStatus = new Label();
      this.barStatus = new ProgressBar();
      this.btnValidate = new Button();
      this.browseSource = new Button();
      this.btnInfo = new Button();
      this.btnHelp = new Button();
      this.selectKeyFile = new OpenFileDialog();
      this.groupOptions = new GroupBox();
      this.optPrefix = new CheckBox();
      this.optUnbin = new CheckBox();
      this.label1 = new Label();
      this.optDestDir = new TextBox();
      this.browse = new Button();
      this.optDest = new CheckBox();
      this.resetList = new Button();
      this.fileTableContextMenu.SuspendLayout();
      this.groupOptions.SuspendLayout();
      this.SuspendLayout();
      this.optSubDirs.Location = new Point(645, 42);
      this.optSubDirs.Name = "optSubDirs";
      this.optSubDirs.Size = new Size(135, 24);
      this.optSubDirs.TabIndex = 28;
      this.optSubDirs.Text = "Scan sub directories";
      this.helpTip.SetToolTip((Control) this.optSubDirs, "This has an effect on when a new source directory is added");
      this.optSubDirs.UseVisualStyleBackColor = true;
      this.optSubDirs.CheckedChanged += new EventHandler(this.optSubDirs_CheckedChanged);
      this.btnCon.BackgroundImage = (Image) componentResourceManager.GetObject("btnCon.BackgroundImage");
      this.btnCon.BackgroundImageLayout = ImageLayout.Zoom;
      this.btnCon.Enabled = false;
      this.btnCon.Location = new Point(783, 376);
      this.btnCon.Name = "btnCon";
      this.btnCon.Size = new Size(25, 25);
      this.btnCon.TabIndex = 30;
      this.helpTip.SetToolTip((Control) this.btnCon, "Attach debug console");
      this.btnCon.UseVisualStyleBackColor = true;
      this.optOverride.AutoEllipsis = true;
      this.optOverride.Location = new Point(9, 93);
      this.optOverride.Name = "optOverride";
      this.optOverride.Size = new Size(178, 24);
      this.optOverride.TabIndex = 20;
      this.optOverride.Text = "Override existing files";
      this.helpTip.SetToolTip((Control) this.optOverride, "If the destination file exists, it will be overridden");
      this.optOverride.UseVisualStyleBackColor = true;
      this.optOverride.Visible = false;
      this.optOverride.CheckedChanged += new EventHandler(this.optOverride_CheckedChanged);
      this.refreshUI.Enabled = true;
      this.refreshUI.Tick += new EventHandler(this.refreshUI_Tick);
      this.fileTableContextMenu.Items.AddRange(new ToolStripItem[7]
      {
        (ToolStripItem) this.openInExplorerToolStripMenuItem,
        (ToolStripItem) this.showDetailsToolStripMenuItem,
        (ToolStripItem) this.toolStripSeparator1,
        (ToolStripItem) this.selectAllToolStripMenuItem,
        (ToolStripItem) this.invertSelectionToolStripMenuItem,
        (ToolStripItem) this.checkAllToolStripMenuItem,
        (ToolStripItem) this.uncheckAllToolStripMenuItem
      });
      this.fileTableContextMenu.Name = "fileTableContextMenu";
      this.fileTableContextMenu.Size = new Size(184, 142);
      this.openInExplorerToolStripMenuItem.Name = "openInExplorerToolStripMenuItem";
      this.openInExplorerToolStripMenuItem.Size = new Size(183, 22);
      this.openInExplorerToolStripMenuItem.Text = "Open in Explorer";
      this.openInExplorerToolStripMenuItem.Click += new EventHandler(this.openInExplorerToolStripMenuItem_Click);
      this.showDetailsToolStripMenuItem.Name = "showDetailsToolStripMenuItem";
      this.showDetailsToolStripMenuItem.ShortcutKeys = Keys.D | Keys.Control;
      this.showDetailsToolStripMenuItem.Size = new Size(183, 22);
      this.showDetailsToolStripMenuItem.Text = "Show Details";
      this.showDetailsToolStripMenuItem.Click += new EventHandler(this.showDetailsToolStripMenuItem_Click);
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new Size(180, 6);
      this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
      this.selectAllToolStripMenuItem.ShortcutKeys = Keys.A | Keys.Control;
      this.selectAllToolStripMenuItem.Size = new Size(183, 22);
      this.selectAllToolStripMenuItem.Text = "Select All";
      this.selectAllToolStripMenuItem.Click += new EventHandler(this.selectAllToolStripMenuItem_Click);
      this.invertSelectionToolStripMenuItem.Name = "invertSelectionToolStripMenuItem";
      this.invertSelectionToolStripMenuItem.Size = new Size(183, 22);
      this.invertSelectionToolStripMenuItem.Text = "Invert selection";
      this.invertSelectionToolStripMenuItem.Click += new EventHandler(this.invertSelectionToolStripMenuItem_Click);
      this.checkAllToolStripMenuItem.Name = "checkAllToolStripMenuItem";
      this.checkAllToolStripMenuItem.Size = new Size(183, 22);
      this.checkAllToolStripMenuItem.Text = "Check All";
      this.checkAllToolStripMenuItem.Click += new EventHandler(this.checkAllToolStripMenuItem_Click);
      this.uncheckAllToolStripMenuItem.Name = "uncheckAllToolStripMenuItem";
      this.uncheckAllToolStripMenuItem.Size = new Size(183, 22);
      this.uncheckAllToolStripMenuItem.Text = "Uncheck All";
      this.uncheckAllToolStripMenuItem.Click += new EventHandler(this.uncheckAllToolStripMenuItem_Click);
      this.stdoutBox.BackColor = SystemColors.Control;
      this.stdoutBox.Font = new Font("Consolas", 6.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 238);
      this.stdoutBox.Location = new Point(12, 376);
      this.stdoutBox.Name = "stdoutBox";
      this.stdoutBox.ReadOnly = true;
      this.stdoutBox.Size = new Size(597, 102);
      this.stdoutBox.TabIndex = 29;
      this.stdoutBox.Text = "";
      this.fileCount.AutoEllipsis = true;
      this.fileCount.Font = new Font("Consolas", 6.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 238);
      this.fileCount.ImageAlign = ContentAlignment.MiddleLeft;
      this.fileCount.Location = new Point(615, 467);
      this.fileCount.Name = "fileCount";
      this.fileCount.Size = new Size(191, 20);
      this.fileCount.TabIndex = 27;
      this.fileCount.Text = "Status...";
      this.fileCount.TextAlign = ContentAlignment.MiddleLeft;
      this.icoStatus.Location = new Point(788, 487);
      this.icoStatus.Name = "icoStatus";
      this.icoStatus.Size = new Size(20, 20);
      this.icoStatus.TabIndex = 25;
      this.fileTable.AllowDrop = true;
      this.fileTable.CheckBoxes = true;
      this.fileTable.Columns.AddRange(new ColumnHeader[7]
      {
        this.colFileName,
        this.colFileType,
        this.colFilePath,
        this.colFileSize,
        this.colFileProduct,
        this.colFileVersion,
        this.colFilePrefix
      });
      this.fileTable.ContextMenuStrip = this.fileTableContextMenu;
      this.fileTable.FullRowSelect = true;
      this.fileTable.Location = new Point(12, 11);
      this.fileTable.Name = "fileTable";
      this.fileTable.Size = new Size(597, 359);
      this.fileTable.Sorting = SortOrder.Ascending;
      this.fileTable.TabIndex = 24;
      this.fileTable.UseCompatibleStateImageBehavior = false;
      this.fileTable.View = View.Details;
      this.fileTable.ColumnClick += new ColumnClickEventHandler(this.fileTable_ColumnClick);
      this.fileTable.DragDrop += new DragEventHandler(this.fileTable_DragDrop);
      this.fileTable.DragEnter += new DragEventHandler(this.DragDropInit);
      this.fileTable.KeyDown += new KeyEventHandler(this.fileTable_KeyDown);
      this.fileTable.MouseClick += new MouseEventHandler(this.fileTable_MouseClick);
      this.colFileName.Text = "Name";
      this.colFileName.Width = 165;
      this.colFileType.Text = "Type";
      this.colFileType.Width = 36;
      this.colFilePath.Text = "Path";
      this.colFilePath.Width = 229;
      this.colFileSize.Text = "Size";
      this.colFileSize.TextAlign = HorizontalAlignment.Right;
      this.colFileProduct.DisplayIndex = 5;
      this.colFileProduct.Text = "Product";
      this.colFileProduct.Width = 50;
      this.colFileVersion.DisplayIndex = 4;
      this.colFileVersion.Text = "Ver.";
      this.colFileVersion.Width = 54;
      this.colFilePrefix.Text = "Prefix";
      this.colFilePrefix.Width = 116;
      this.textStatus.AutoEllipsis = true;
      this.textStatus.Font = new Font("Consolas", 6.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 238);
      this.textStatus.ImageAlign = ContentAlignment.MiddleLeft;
      this.textStatus.Location = new Point(615, 487);
      this.textStatus.Name = "textStatus";
      this.textStatus.Size = new Size(162, 20);
      this.textStatus.TabIndex = 23;
      this.textStatus.Text = "Status...";
      this.textStatus.TextAlign = ContentAlignment.MiddleLeft;
      this.barStatus.Location = new Point(12, 484);
      this.barStatus.Name = "barStatus";
      this.barStatus.Size = new Size(597, 20);
      this.barStatus.Step = 1;
      this.barStatus.TabIndex = 22;
      this.btnValidate.Font = new Font("Consolas", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 238);
      this.btnValidate.Location = new Point(614, 406);
      this.btnValidate.Name = "btnValidate";
      this.btnValidate.Size = new Size(194, 56);
      this.btnValidate.TabIndex = 19;
      this.btnValidate.Text = "&Process files";
      this.btnValidate.UseVisualStyleBackColor = true;
      this.btnValidate.Click += new EventHandler(this.btnValidate_Click);
      this.browseSource.BackgroundImageLayout = ImageLayout.None;
      this.browseSource.Font = new Font("Consolas", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 238);
      this.browseSource.Image = (Image) componentResourceManager.GetObject("browseSource.Image");
      this.browseSource.ImageAlign = ContentAlignment.MiddleRight;
      this.browseSource.Location = new Point(617, 11);
      this.browseSource.Name = "browseSource";
      this.browseSource.Size = new Size(191, 28);
      this.browseSource.TabIndex = 26;
      this.browseSource.Text = "Add a source directory";
      this.browseSource.UseVisualStyleBackColor = true;
      this.browseSource.Click += new EventHandler(this.browseSource_Click);
      this.btnInfo.BackgroundImage = (Image) componentResourceManager.GetObject("btnInfo.BackgroundImage");
      this.btnInfo.BackgroundImageLayout = ImageLayout.Zoom;
      this.btnInfo.Enabled = false;
      this.btnInfo.FlatAppearance.BorderSize = 0;
      this.btnInfo.Location = new Point(721, 376);
      this.btnInfo.Name = "btnInfo";
      this.btnInfo.Size = new Size(25, 25);
      this.btnInfo.TabIndex = 21;
      this.btnInfo.UseVisualStyleBackColor = true;
      this.btnHelp.BackgroundImage = (Image) componentResourceManager.GetObject("btnHelp.BackgroundImage");
      this.btnHelp.BackgroundImageLayout = ImageLayout.Zoom;
      this.btnHelp.Location = new Point(752, 376);
      this.btnHelp.Name = "btnHelp";
      this.btnHelp.Size = new Size(25, 25);
      this.btnHelp.TabIndex = 20;
      this.btnHelp.UseVisualStyleBackColor = true;
      this.btnHelp.Click += new EventHandler(this.btnHelp_Click);
      this.groupOptions.Controls.Add((Control) this.optPrefix);
      this.groupOptions.Controls.Add((Control) this.optUnbin);
      this.groupOptions.Controls.Add((Control) this.optOverride);
      this.groupOptions.Controls.Add((Control) this.label1);
      this.groupOptions.Controls.Add((Control) this.optDestDir);
      this.groupOptions.Controls.Add((Control) this.browse);
      this.groupOptions.Controls.Add((Control) this.optDest);
      this.groupOptions.Controls.Add((Control) this.resetList);
      this.groupOptions.Font = new Font("Consolas", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 238);
      this.groupOptions.Location = new Point(617, 72);
      this.groupOptions.Name = "groupOptions";
      this.groupOptions.Size = new Size(191, 298);
      this.groupOptions.TabIndex = 18;
      this.groupOptions.TabStop = false;
      this.groupOptions.Text = "Options";
      this.optPrefix.AutoEllipsis = true;
      this.optPrefix.Location = new Point(9, 153);
      this.optPrefix.Name = "optPrefix";
      this.optPrefix.Size = new Size(178, 24);
      this.optPrefix.TabIndex = 23;
      this.optPrefix.Text = "Use PBO Prefix";
      this.optPrefix.UseVisualStyleBackColor = true;
      this.optPrefix.Visible = false;
      this.optPrefix.CheckedChanged += new EventHandler(this.optPrefix_CheckedChanged);
      this.optUnbin.AutoEllipsis = true;
      this.optUnbin.Location = new Point(9, 123);
      this.optUnbin.Name = "optUnbin";
      this.optUnbin.Size = new Size(178, 24);
      this.optUnbin.TabIndex = 22;
      this.optUnbin.Text = "Unbinarize config files";
      this.optUnbin.UseVisualStyleBackColor = true;
      this.optUnbin.Visible = false;
      this.optUnbin.CheckedChanged += new EventHandler(this.optUnbin_CheckedChanged);
      this.label1.AutoSize = true;
      this.label1.Font = new Font("Consolas", 9f, FontStyle.Regular, GraphicsUnit.Point, (byte) 238);
      this.label1.Location = new Point(6, 16);
      this.label1.Name = "label1";
      this.label1.Size = new Size(154, 14);
      this.label1.TabIndex = 19;
      this.label1.Text = "Destination directory";
      this.optDestDir.Enabled = false;
      this.optDestDir.Location = new Point(9, 66);
      this.optDestDir.Name = "optDestDir";
      this.optDestDir.Size = new Size(179, 20);
      this.optDestDir.TabIndex = 18;
      this.optDestDir.TextChanged += new EventHandler(this.optDestDir_TextChanged);
      this.browse.BackgroundImage = (Image) componentResourceManager.GetObject("browse.BackgroundImage");
      this.browse.BackgroundImageLayout = ImageLayout.Center;
      this.browse.Location = new Point(163, 37);
      this.browse.Name = "browse";
      this.browse.Size = new Size(25, 25);
      this.browse.TabIndex = 17;
      this.browse.UseVisualStyleBackColor = true;
      this.browse.Click += new EventHandler(this.browse_Click);
      this.optDest.AutoSize = true;
      this.optDest.Location = new Point(9, 42);
      this.optDest.Name = "optDest";
      this.optDest.Size = new Size(92, 17);
      this.optDest.TabIndex = 16;
      this.optDest.Text = "use custom?";
      this.optDest.UseVisualStyleBackColor = true;
      this.optDest.CheckedChanged += new EventHandler(this.optDest_CheckedChanged);
      this.resetList.Location = new Point(6, 269);
      this.resetList.Name = "resetList";
      this.resetList.Size = new Size(178, 23);
      this.resetList.TabIndex = 11;
      this.resetList.Text = "Reset list";
      this.resetList.UseVisualStyleBackColor = true;
      this.resetList.Click += new EventHandler(this.resetList_Click);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.AutoSize = true;
      this.ClientSize = new Size(820, 519);
      this.Controls.Add((Control) this.btnCon);
      this.Controls.Add((Control) this.stdoutBox);
      this.Controls.Add((Control) this.optSubDirs);
      this.Controls.Add((Control) this.fileCount);
      this.Controls.Add((Control) this.browseSource);
      this.Controls.Add((Control) this.icoStatus);
      this.Controls.Add((Control) this.fileTable);
      this.Controls.Add((Control) this.textStatus);
      this.Controls.Add((Control) this.barStatus);
      this.Controls.Add((Control) this.btnInfo);
      this.Controls.Add((Control) this.btnHelp);
      this.Controls.Add((Control) this.btnValidate);
      this.Controls.Add((Control) this.groupOptions);
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MaximizeBox = false;
      this.Name = nameof (FormMain);
      this.SizeGripStyle = SizeGripStyle.Hide;
      this.Text = "BankRev";
      this.Load += new EventHandler(this.formMain_Load);
      this.fileTableContextMenu.ResumeLayout(false);
      this.groupOptions.ResumeLayout(false);
      this.groupOptions.PerformLayout();
      this.ResumeLayout(false);
    }

    public enum AppState
    {
      Unchanged,
      Ready,
      Canceled,
      Succeeded,
      Failed,
      Busy,
      Working,
    }

    public enum SourceFormat
    {
      PBO,
      EBO,
    }

    private delegate void SetTextCallback(string text);
  }
}
