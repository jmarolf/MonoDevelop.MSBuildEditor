﻿// Copyright (c) 2016 Xamarin Inc.
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MonoDevelop.MSBuildEditor.Schema
{
	class ItemInfo : VariableInfo
	{
		public ItemInfo (
			string name, string description, string includeDescription = null,
			MSBuildValueKind valueKind = MSBuildValueKind.Unknown,
			Dictionary<string, MetadataInfo> metadata = null)
			: base (name, description, valueKind)
		{
			Metadata = metadata ?? new Dictionary<string, MetadataInfo> ();
			IncludeDescription = includeDescription;
		}

		public Dictionary<string,MetadataInfo> Metadata { get; private set; }

		//custom description for the kinds of items in the include
		public string IncludeDescription { get; }
    }
}