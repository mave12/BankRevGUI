
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace BankRev.Properties
{
  [CompilerGenerated]
  [DebuggerNonUserCode]
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (object.ReferenceEquals((object) BankRev.Properties.Resources.resourceMan, (object) null))
          BankRev.Properties.Resources.resourceMan = new ResourceManager("BankRev.Properties.Resources", typeof (BankRev.Properties.Resources).Assembly);
        return BankRev.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get => BankRev.Properties.Resources.resourceCulture;
      set => BankRev.Properties.Resources.resourceCulture = value;
    }

    internal static Bitmap folder => (Bitmap) BankRev.Properties.Resources.ResourceManager.GetObject(nameof (folder), BankRev.Properties.Resources.resourceCulture);
  }
}
