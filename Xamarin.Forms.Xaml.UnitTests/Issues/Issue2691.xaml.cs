using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Utilities;
using Xamarin.Forms;
using Xamarin.Forms.Build.Tasks;

using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;
using Xamarin.Forms.Xaml.UnitTests.XmlnsDefinitionAttribute;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TestFixture]
	public class Issue2691Tests
	{  
		const string c_xaml = @"
				<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
							 xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							 xmlns:test=""http://xamarin.com/schemas/2014/forms/testing""
							 x:Class=""Xamarin.Forms.Xaml.UnitTests.Issue2691"">
					<ContentPage.Content>
						<StackLayout>
							<test:TestLabel x:Name=""_testLabel""
											Text=""Welcome to Xamarin.Forms!""
											VerticalOptions=""CenterAndExpand"" 
											HorizontalOptions=""CenterAndExpand"" />
						</StackLayout>
					</ContentPage.Content>
				</ContentPage>";
		const string c_References = "Xamarin.Forms.Xaml.UnitTests.XmlnsDefinitionAttribute.dll";		

		[TestCase(false)]
		[TestCase(true)] 
		public void CreateFromXamlCompiler(bool useCompiledXaml)
		{
			TestLabel testLabel = new TestLabel();	//

			string xamlInputFile = CreateXamlInputFile();
			var item = new TaskItem(xamlInputFile);
			item.SetMetadata("TargetPath", xamlInputFile);
			var generator = new XamlGTask()         
			{
				BuildEngine = new DummyBuildEngine(),
				AssemblyName = "test",
				Language = "C#",
				XamlFiles = new[] { item }, 
				OutputPath = Path.GetDirectoryName(xamlInputFile),
				References = c_References
			};

			Assert.IsTrue(generator.Execute()); 
		}

		string CreateXamlInputFile()
		{
			string fileName = Path.GetTempFileName();
			File.WriteAllText(fileName, c_xaml);
			return fileName;
		}		
	}

	public partial class Issue2691 : ContentPage
	{
		public Issue2691()
		{
			InitializeComponent();
		}

		public Issue2691(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
	}
}