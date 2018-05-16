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

		[SetUp]
		public void SetUp()
		{
			Device.PlatformServices = new MockPlatformServices();
		}

		[TestCase(false)]
		[TestCase(true)]
		public void TestXamlParser(bool useCompiledXaml)
		{
			Issue2691 issue2691 = new Issue2691(useCompiledXaml);
			var label = issue2691.FindByName("_testLabel") as TestLabel;
			Assert.IsNotNull(label);
		}

		[TestCase]
		public void TestXamlCompiler()
		{
			MockCompiler.Compile(typeof(Issue2691));
		}

		[TestCase] 
		public void TestXamlGenerator()
		{			
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
				References = @"C:\VSProjects\Xamarin.Forms\Xamarin.Forms.Xaml.UnitTests.XmlnsDefinitionAttribute\bin\Debug\netstandard2.0\Xamarin.Forms.Xaml.UnitTests.XmlnsDefinitionAttribute.dll"
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