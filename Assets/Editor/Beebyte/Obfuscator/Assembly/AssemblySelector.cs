﻿/*
 * Copyright (c) 2018 Beebyte Limited. All rights reserved.
 */
using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_2017_3_OR_NEWER
using UnityEditor.Compilation;
#endif
using UnityEngine;

namespace Beebyte.Obfuscator.Assembly
{
	public class AssemblySelector
	{
		private readonly HashSet<string> _compiledAssemblyPaths = new HashSet<string>();
		private readonly HashSet<string> _assemblyPaths = new HashSet<string>();
		
		public AssemblySelector(Options options)
		{
			if (options == null) throw new ArgumentException("options must not be null", "options");
			if (options.compiledAssemblies == null) throw new ArgumentException(
				"options.compiledAssemblies must not be null", "options");
			if (options.assemblies == null) throw new ArgumentException(
				"options.assemblies must not be null", "options");
			if (Application.dataPath == null) throw new ArgumentException("Application.dataPath must not be null");
			
			foreach (string assemblyName in options.compiledAssemblies)
			{
				_compiledAssemblyPaths.Add(FindDllLocation(assemblyName));
			}
			foreach (string assemblyName in options.assemblies)
			{
				_assemblyPaths.Add(FindDllLocation(assemblyName));
			}

#if UNITY_2017_3_OR_NEWER
			if (!options.includeCompilationPipelineAssemblies) return;

			string projectDir = Path.GetDirectoryName(Application.dataPath);

#if UNITY_2018_1_OR_NEWER
			foreach (UnityEditor.Compilation.Assembly assembly in CompilationPipeline.GetAssemblies(AssembliesType.Player))
			{
#else
			foreach (UnityEditor.Compilation.Assembly assembly in CompilationPipeline.GetAssemblies())
			{
				if ((assembly.flags & AssemblyFlags.EditorAssembly) != 0)
				{
					continue;
				}
#endif
				if (assembly.name.Contains("-firstpass"))
				{
					continue;
				}
				
				if (assembly.sourceFiles.Length == 0)
				{
					continue;
				}

				if (assembly.sourceFiles[0].StartsWith("Packages"))
				{
					continue;
				}

				string dllLocation = Path.Combine(projectDir, assembly.outputPath).Replace('\\', '/');
				
				// If the assembly is for a different build target platform, oddly it will still be in the compilation
				// pipeline, however the file won't actually exist.
				if (File.Exists(dllLocation))
				{
					_assemblyPaths.Add(dllLocation);
				}
			}
#endif
		}

		public ICollection<string> GetCompiledAssemblyPaths()
		{
			return _compiledAssemblyPaths;
		}
		
		public ICollection<string> GetAssemblyPaths()
		{
			return _assemblyPaths;
		}

		private static string FindDllLocation(string suffix)
		{
			if (string.IsNullOrEmpty(suffix))
			{
				throw new ArgumentException(
					"Empty or null DLL names are forbidden (check Obfuscator Options assemblies / compiled assemblies list)");
			}
			
			foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					if (assembly.Location.Equals(string.Empty))
					{
						DisplayFailedAssemblyParseWarning(assembly);
					}
					else if (assembly.Location.EndsWith(suffix))
					{
						return assembly.Location.Replace('\\', '/');
					}
				}
				catch (NotSupportedException)
				{
					DisplayFailedAssemblyParseWarning(assembly);
				}
			}

			throw new ArgumentException(
				suffix + " was not found (check Obfuscator Options assemblies / compiled assemblies list)");
		}

		private static void DisplayFailedAssemblyParseWarning(System.Reflection.Assembly assembly)
		{
			Debug.LogWarning("Could not parse dynamically created assembly (string.Empty location) " +
							 assembly.FullName +
							 ". If you extend classes from within this assembly that in turn extend from " +
							 "MonoBehaviour you will need to manually annotate these classes with [Skip]");
		}
	}
}
