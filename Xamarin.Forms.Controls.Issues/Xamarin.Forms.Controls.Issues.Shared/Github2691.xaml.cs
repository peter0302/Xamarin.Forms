using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{	
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2691, "Make XmlnsDefinitionAttribute Public", PlatformAffected.All)]
	public partial class Github2691 : ContentPage
	{
		public Github2691()
		{
			InitializeComponent ();

			// Necessary to ensure custom control library gets loaded into
			// the AppDomain so the parser can find it.
			Issues.CustomNamespaceUris.Library.Init();
		}
	}
}