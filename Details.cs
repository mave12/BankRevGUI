
using BankRev.Properties;
using LibCommon.Extension_methods;
using LibCommon.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BankRev
{
  public class Details : Form
  {
    private readonly PBO _pbo = new PBO(FormMain.CurrentFilePath, true);
    private IContainer components = (IContainer) null;
    private Label dataProduct;
    private Label dataType;
    private Label dataSize;
    private Label label6;
    private Label label5;
    private Label yetAnotherLabel;
    private Label label3;
    private Label dataFileName;
    private TextBox dataFilePath;
    private Button browse;
    private Label label2;
    private ToolTip tip;
    private Button btnClose;
    private Label dataPrefix;
    private Label label8;
    private Label dataVersion;
    private Label label10;
    private Label dataSA;
    private Label label14;
    private Label dataFileCnt;
    private Label label12;

    public Details() => this.InitializeComponent();

    private void Details_Load(object sender, EventArgs e)
    {
      this.dataFileName.Text = this._pbo.FileName;
      this.dataFilePath.Text = this._pbo.FilePath;
      this.dataSize.Text = this._pbo.FilePath.GetFormattedFileSize();
      this.dataType.Text = this._pbo.AddonType.ToString();
      this.dataProduct.Text = this._pbo.Product;
      this.dataPrefix.Text = this._pbo.Prefix;
      this.dataVersion.Text = this._pbo.Version;
      this.dataSA.Text = this._pbo.Standalone.ToString();
      this.dataFileCnt.Text = this._pbo.FileList.Count.ToString();
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
      if (this._pbo != null)
        this._pbo.Dispose();
      this.Close();
    }

    private void browse_Click(object sender, EventArgs e) => FileSystem.ViewInExplorer(this._pbo.FilePath);

    protected override bool ProcessDialogKey(Keys keyData)
    {
      if (Control.ModifierKeys != Keys.None || keyData != Keys.Escape)
        return base.ProcessDialogKey(keyData);
      this.Close();
      return true;
    }

    private void Details_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.Escape)
        return;
      this.Close();
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
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (Details));
      this.dataProduct = new Label();
      this.dataType = new Label();
      this.dataSize = new Label();
      this.label6 = new Label();
      this.label5 = new Label();
      this.yetAnotherLabel = new Label();
      this.label3 = new Label();
      this.dataFileName = new Label();
      this.dataFilePath = new TextBox();
      this.browse = new Button();
      this.label2 = new Label();
      this.tip = new ToolTip(this.components);
      this.btnClose = new Button();
      this.dataPrefix = new Label();
      this.label8 = new Label();
      this.dataVersion = new Label();
      this.label10 = new Label();
      this.dataSA = new Label();
      this.label14 = new Label();
      this.dataFileCnt = new Label();
      this.label12 = new Label();
      this.SuspendLayout();
      this.dataProduct.AutoSize = true;
      this.dataProduct.Location = new Point(93, 117);
      this.dataProduct.Name = "dataProduct";
      this.dataProduct.Size = new Size(16, 13);
      this.dataProduct.TabIndex = 44;
      this.dataProduct.Text = "...";
      this.dataType.AutoSize = true;
      this.dataType.Location = new Point(93, 91);
      this.dataType.Name = "dataType";
      this.dataType.Size = new Size(16, 13);
      this.dataType.TabIndex = 43;
      this.dataType.Text = "...";
      this.dataSize.AutoSize = true;
      this.dataSize.Location = new Point(93, 65);
      this.dataSize.Name = "dataSize";
      this.dataSize.Size = new Size(16, 13);
      this.dataSize.TabIndex = 42;
      this.dataSize.Text = "...";
      this.label6.AutoSize = true;
      this.label6.Location = new Point(13, 117);
      this.label6.Name = "label6";
      this.label6.Size = new Size(47, 13);
      this.label6.TabIndex = 41;
      this.label6.Text = "Product:";
      this.label5.AutoSize = true;
      this.label5.Location = new Point(13, 91);
      this.label5.Name = "label5";
      this.label5.Size = new Size(34, 13);
      this.label5.TabIndex = 40;
      this.label5.Text = "Type:";
      this.yetAnotherLabel.AutoSize = true;
      this.yetAnotherLabel.Location = new Point(13, 65);
      this.yetAnotherLabel.Name = "yetAnotherLabel";
      this.yetAnotherLabel.Size = new Size(33, 13);
      this.yetAnotherLabel.TabIndex = 38;
      this.yetAnotherLabel.Text = "Size: ";
      this.label3.AutoSize = true;
      this.label3.Location = new Point(9, 37);
      this.label3.Name = "label3";
      this.label3.Size = new Size(32, 13);
      this.label3.TabIndex = 35;
      this.label3.Text = "Path:";
      this.dataFileName.AutoSize = true;
      this.dataFileName.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, (byte) 238);
      this.dataFileName.Location = new Point(93, 18);
      this.dataFileName.Name = "dataFileName";
      this.dataFileName.Size = new Size(41, 13);
      this.dataFileName.TabIndex = 34;
      this.dataFileName.Text = "label1";
      this.dataFilePath.Location = new Point(96, 34);
      this.dataFilePath.Name = "dataFilePath";
      this.dataFilePath.ReadOnly = true;
      this.dataFilePath.Size = new Size(160, 20);
      this.dataFilePath.TabIndex = 33;
      this.browse.BackgroundImage = (Image) Resources.folder;
      this.browse.BackgroundImageLayout = ImageLayout.Center;
      this.browse.Location = new Point(262, 31);
      this.browse.Name = "browse";
      this.browse.Size = new Size(25, 25);
      this.browse.TabIndex = 32;
      this.browse.UseVisualStyleBackColor = true;
      this.browse.Click += new EventHandler(this.browse_Click);
      this.label2.AutoSize = true;
      this.label2.Location = new Point(9, 18);
      this.label2.Name = "label2";
      this.label2.Size = new Size(29, 13);
      this.label2.TabIndex = 30;
      this.label2.Text = "File: ";
      this.btnClose.Location = new Point(212, 234);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new Size(75, 23);
      this.btnClose.TabIndex = 31;
      this.btnClose.Text = "&Close";
      this.btnClose.UseVisualStyleBackColor = true;
      this.btnClose.Click += new EventHandler(this.btnClose_Click);
      this.dataPrefix.AutoSize = true;
      this.dataPrefix.Location = new Point(93, 143);
      this.dataPrefix.Name = "dataPrefix";
      this.dataPrefix.Size = new Size(16, 13);
      this.dataPrefix.TabIndex = 48;
      this.dataPrefix.Text = "...";
      this.label8.AutoSize = true;
      this.label8.Location = new Point(13, 143);
      this.label8.Name = "label8";
      this.label8.Size = new Size(36, 13);
      this.label8.TabIndex = 47;
      this.label8.Text = "Prefix:";
      this.dataVersion.AutoSize = true;
      this.dataVersion.Location = new Point(93, 169);
      this.dataVersion.Name = "dataVersion";
      this.dataVersion.Size = new Size(16, 13);
      this.dataVersion.TabIndex = 52;
      this.dataVersion.Text = "...";
      this.label10.AutoSize = true;
      this.label10.Location = new Point(13, 169);
      this.label10.Name = "label10";
      this.label10.Size = new Size(45, 13);
      this.label10.TabIndex = 51;
      this.label10.Text = "Version:";
      this.dataSA.AutoSize = true;
      this.dataSA.Location = new Point(93, 195);
      this.dataSA.Name = "dataSA";
      this.dataSA.Size = new Size(16, 13);
      this.dataSA.TabIndex = 56;
      this.dataSA.Text = "...";
      this.label14.AutoSize = true;
      this.label14.Location = new Point(13, 195);
      this.label14.Name = "label14";
      this.label14.Size = new Size(76, 13);
      this.label14.TabIndex = 55;
      this.label14.Text = "Is standalone?";
      this.dataFileCnt.AutoSize = true;
      this.dataFileCnt.Location = new Point(93, 221);
      this.dataFileCnt.Name = "dataFileCnt";
      this.dataFileCnt.Size = new Size(16, 13);
      this.dataFileCnt.TabIndex = 60;
      this.dataFileCnt.Text = "...";
      this.label12.AutoSize = true;
      this.label12.Location = new Point(13, 221);
      this.label12.Name = "label12";
      this.label12.Size = new Size(56, 13);
      this.label12.TabIndex = 59;
      this.label12.Text = "File count:";
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.BackColor = Color.White;
      this.ClientSize = new Size(299, 269);
      this.Controls.Add((Control) this.dataFileCnt);
      this.Controls.Add((Control) this.label12);
      this.Controls.Add((Control) this.dataSA);
      this.Controls.Add((Control) this.label14);
      this.Controls.Add((Control) this.dataVersion);
      this.Controls.Add((Control) this.label10);
      this.Controls.Add((Control) this.dataPrefix);
      this.Controls.Add((Control) this.label8);
      this.Controls.Add((Control) this.dataProduct);
      this.Controls.Add((Control) this.dataType);
      this.Controls.Add((Control) this.dataSize);
      this.Controls.Add((Control) this.label6);
      this.Controls.Add((Control) this.label5);
      this.Controls.Add((Control) this.yetAnotherLabel);
      this.Controls.Add((Control) this.label3);
      this.Controls.Add((Control) this.dataFileName);
      this.Controls.Add((Control) this.dataFilePath);
      this.Controls.Add((Control) this.browse);
      this.Controls.Add((Control) this.label2);
      this.Controls.Add((Control) this.btnClose);
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (Details);
      this.ShowInTaskbar = false;
      this.Text = nameof (Details);
      this.Load += new EventHandler(this.Details_Load);
      this.KeyDown += new KeyEventHandler(this.Details_KeyDown);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
