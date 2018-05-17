using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	static class XmlTypeExtensions
	{
		static Dictionary<ModuleDefinition, IList<XmlnsDefinitionAttribute>> s_xmlnsDefinitions = 
			new Dictionary<ModuleDefinition, IList<XmlnsDefinitionAttribute>>();
		static object _nsLock = new object();

		static IList<XmlnsDefinitionAttribute> GatherXmlnsDefinitionAttributes(ModuleDefinition module)
		{
			var xmlnsDefinitions = new List<XmlnsDefinitionAttribute>();

			foreach (var asmRef in module.AssemblyReferences) {
				var asmDef = module.AssemblyResolver.Resolve(asmRef);
				foreach (var ca in asmDef.CustomAttributes) {
					if (ca.AttributeType.FullName == typeof(XmlnsDefinitionAttribute).FullName) {
						var attr = GetXmlnsDefinition(ca, asmDef);
						xmlnsDefinitions.Add(attr);
					}
				}
			}

			s_xmlnsDefinitions[module] = xmlnsDefinitions;
			return xmlnsDefinitions;
		}

		public static TypeReference GetTypeReference(string xmlType, ModuleDefinition module, BaseNode node)
		{
			var split = xmlType.Split(':');
			if (split.Length > 2)
				throw new XamlParseException($"Type \"{xmlType}\" is invalid", node as IXmlLineInfo);

			string prefix, name;
			if (split.Length == 2) {
				prefix = split[0];
				name = split[1];
			} else {
				prefix = "";
				name = split[0];
			}
			var namespaceuri = node.NamespaceResolver.LookupNamespace(prefix) ?? "";
			return GetTypeReference(new XmlType(namespaceuri, name, null), module, node as IXmlLineInfo);
		}

		public static TypeReference GetTypeReference(string namespaceURI, string typename, ModuleDefinition module, IXmlLineInfo xmlInfo)
		{
			return new XmlType(namespaceURI, typename, null).GetTypeReference(module, xmlInfo);
		}

		public static TypeReference GetTypeReference(this XmlType xmlType, ModuleDefinition module, IXmlLineInfo xmlInfo)
		{
			IList<XmlnsDefinitionAttribute> xmlnsDefinitions = null;
			lock (_nsLock) {					
				if (!s_xmlnsDefinitions.TryGetValue(module, out xmlnsDefinitions))
					xmlnsDefinitions = GatherXmlnsDefinitionAttributes(module);
			}

			var namespaceURI = xmlType.NamespaceUri;
			var elementName = xmlType.Name;
			var typeArguments = xmlType.TypeArguments;

			var lookupAssemblies = new List<XmlnsDefinitionAttribute>();

			var lookupNames = new List<string>();

			foreach (var xmlnsDef in xmlnsDefinitions) {
				if (xmlnsDef.XmlNamespace != namespaceURI)
					continue;
				lookupAssemblies.Add(xmlnsDef);
			}

			if (lookupAssemblies.Count == 0) {
				string ns;
				string typename;
				string asmstring;
				string targetPlatform;

				XmlnsHelper.ParseXmlns(namespaceURI, out typename, out ns, out asmstring, out targetPlatform);
				asmstring = asmstring ?? module.Assembly.Name.Name;
				if (ns != null)
					lookupAssemblies.Add(new XmlnsDefinitionAttribute(namespaceURI, ns) {
						AssemblyName = asmstring
					});
			}

			lookupNames.Add(elementName);
			lookupNames.Add(elementName + "Extension");

			for (var i = 0; i < lookupNames.Count; i++)
			{
				var name = lookupNames[i];
				if (name.Contains(":"))
					name = name.Substring(name.LastIndexOf(':') + 1);
				if (typeArguments != null)
					name += "`" + typeArguments.Count; //this will return an open generic Type
				lookupNames[i] = name;
			}

			TypeReference type = null;
			foreach (var asm in lookupAssemblies)
			{
				if (type != null)
					break;
				foreach (var name in lookupNames)
				{
					if (type != null)
						break;

					var clrNamespace = asm.ClrNamespace;
					var typeName = name.Replace('+', '/'); //Nested types
					var idx = typeName.LastIndexOf('.');
					if (idx >= 0) {
						clrNamespace += '.' + typeName.Substring(0, typeName.LastIndexOf('.'));
						typeName = typeName.Substring(typeName.LastIndexOf('.') + 1);
					}
					type = module.GetTypeDefinition((asm.AssemblyName, clrNamespace, typeName));
				}
			}

			if (type != null && typeArguments != null && type.HasGenericParameters)
			{
				type =
					module.ImportReference(type)
						.MakeGenericInstanceType(typeArguments.Select(x => GetTypeReference(x, module, xmlInfo)).ToArray());
			}

			if (type == null)
				throw new XamlParseException(string.Format("Type {0} not found in xmlns {1}. Ensure third party control libraries are referenced in the code of your project and not just in XAML.", elementName, namespaceURI), xmlInfo);

			return module.ImportReference(type);
		}

		public static XmlnsDefinitionAttribute GetXmlnsDefinition(this CustomAttribute ca, AssemblyDefinition asmDef)
		{
			var attr = new XmlnsDefinitionAttribute(
							ca.ConstructorArguments[0].Value as string,
							ca.ConstructorArguments[1].Value as string);

			string assemblyName = null;
			if (ca.Properties.Count > 0)
				assemblyName = ca.Properties[0].Argument.Value as string;
			attr.AssemblyName = assemblyName ?? asmDef.Name.FullName;
			return attr;
		}
	}
}