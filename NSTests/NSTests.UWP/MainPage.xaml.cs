namespace NSTests.UWP
{
	public sealed partial class MainPage
	{
		public MainPage()
		{
			InitializeComponent();

			LoadApplication(new NSTests.App());
		}
	}
}