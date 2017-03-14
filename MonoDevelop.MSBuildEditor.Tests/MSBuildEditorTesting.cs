﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Core.Text;
using MonoDevelop.CSharpBinding;
using MonoDevelop.CSharpBinding.Tests;
using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.TypeSystem;
using MonoDevelop.Projects;

namespace MonoDevelop.MSBuildEditor.Tests
{
	//largely copied from MonoDevelop.AspNet.Tests.WebForms.WebFormsTesting
	// MIT License
	// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
	static class MSBuildEditorTesting
	{
		public static async Task<CompletionDataList> CreateProvider (string text, string extension, bool isCtrlSpace = false)
		{
			var result = await CreateEditor (text, extension);
			var textEditorCompletion = result.Extension;
			string editorText = result.EditorText;
			TestViewContent sev = result.ViewContent;
			int cursorPosition = text.IndexOf ('$');

			var ctx = textEditorCompletion.GetCodeCompletionContext (sev);

			if (isCtrlSpace)
				return await textEditorCompletion.CodeCompletionCommand (ctx) as CompletionDataList;
			else {
				var task = textEditorCompletion.HandleCodeCompletionAsync (ctx, editorText [cursorPosition - 1]);
				if (task != null) {
					return await task as CompletionDataList;
				}
				return null;
			}
		}

		struct CreateEditorResult
		{
			public WebFormsTestingEditorExtension Extension;
			public string EditorText;
			public TestViewContent ViewContent;
		}

		static async Task<CreateEditorResult> CreateEditor (string text, string extension)
		{
			string editorText;
			TestViewContent sev;
			string parsedText;
			int cursorPosition = text.IndexOf ('$');
			int endPos = text.IndexOf ('$', cursorPosition + 1);
			if (endPos == -1)
				parsedText = editorText = text.Substring (0, cursorPosition) + text.Substring (cursorPosition + 1);
			else {
				parsedText = text.Substring (0, cursorPosition) + new string (' ', endPos - cursorPosition) + text.Substring (endPos + 1);
				editorText = text.Substring (0, cursorPosition) + text.Substring (cursorPosition + 1, endPos - cursorPosition - 1) + text.Substring (endPos + 1);
				cursorPosition = endPos - 1;
			}

			var project = Services.ProjectService.CreateDotNetProject ("C#");
			project.References.Add (ProjectReference.CreateAssemblyReference ("System"));
			project.References.Add (ProjectReference.CreateAssemblyReference ("System.Web"));
			project.FileName = UnitTests.TestBase.GetTempFile (".csproj");
			string file = UnitTests.TestBase.GetTempFile (extension);
			project.AddFile (file);

			sev = new TestViewContent ();
			sev.Project = project;
			sev.ContentName = file;
			sev.Text = editorText;
			sev.CursorPosition = cursorPosition;

			var tww = new TestWorkbenchWindow ();
			tww.ViewContent = sev;

			var doc = new TestDocument (tww);
			doc.Editor.FileName = sev.ContentName;
			var parser = new MSBuildDocumentParser ();
			var options = new ParseOptions {
				Project = project,
				FileName = sev.ContentName,
				Content = new StringTextSource (parsedText)
			};
			var parsedDoc = await parser.Parse (options, default (CancellationToken)) as MSBuildParsedDocument;
			doc.HiddenParsedDocument = parsedDoc;

			return new CreateEditorResult {
				Extension = new WebFormsTestingEditorExtension (doc),
				EditorText = editorText,
				ViewContent = sev
			};
		}

		public class WebFormsTestingEditorExtension : MSBuildTextEditorExtension
		{
			public WebFormsTestingEditorExtension (Document doc)
			{
				Initialize (doc.Editor, doc);
			}

			public CodeCompletionContext GetCodeCompletionContext (TestViewContent sev)
			{
				var ctx = new CodeCompletionContext ();
				ctx.TriggerOffset = sev.CursorPosition;

				int line, column;
				sev.GetLineColumnFromPosition (ctx.TriggerOffset, out line, out column);
				ctx.TriggerLine = line;
				ctx.TriggerLineOffset = column - 1;

				return ctx;
			}
		}
	}
}