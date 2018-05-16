using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace NSTests { 
	public partial class App : Application
	{
		public App ()
		{
			InitializeComponent ();

			ControlLibrary1.Library.Init();
			ControlLibrary2.Library.Init();

			this.MainPage = new MainPage();
		}
	}
}
