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
	public partial class Gh2691 : ContentPage
	{
		public Gh2691()
		{
			InitializeComponent();
		}

		public Gh2691(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			const string c_xaml = @"
				<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
							 xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							 xmlns:test=""http://xamarin.com/schemas/2014/forms/testing""
							 x:Class=""Xamarin.Forms.Xaml.UnitTests.Gh2691"">
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
			public void TestXamlParserAndGenerator(bool useCompiledXaml)
			{
				Gh2691 issue2691 = new Gh2691(useCompiledXaml);
				var label = issue2691.FindByName("_testLabel") as TestLabel;
				Assert.IsNotNull(label);
			}

			[TestCase]
			public void TestXamlCompiler()
			{
				MockCompiler.Compile(typeof(Gh2691));
			}

			[TestCase]
			public void TestXamlGenerator()
			{
				string xamlInputFile = CreateXamlInputFile();
				var item = new TaskItem(xamlInputFile);
				item.SetMetadata("TargetPath", xamlInputFile);

				string testAssemblyBinPath =
#if DEBUG
					"Debug";
#else
					"Release";
#endif

				var generator = new XamlGTask()
				{
					BuildEngine = new DummyBuildEngine(),
					AssemblyName = "test",
					Language = "C#",
					XamlFiles = new[] { item },
					OutputPath = Path.GetDirectoryName(xamlInputFile),
					References = Path.GetFullPath(
						Path.Combine(
							Directory.GetCurrentDirectory(),
							$@"..\..\..\Xamarin.Forms.Xaml.UnitTests.XmlnsDefinitionAttribute\bin\{testAssemblyBinPath}\netstandard2.0\Xamarin.Forms.Xaml.UnitTests.XmlnsDefinitionAttribute.dll"))
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
	}
}