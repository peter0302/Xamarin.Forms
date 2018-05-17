using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.Issues
{	
	[Issue(IssueTracker.Github, 2691, "Make XmlnsDefinitionAttribute Public", PlatformAffected.All)]
	[Preserve(AllMembers = true)]
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