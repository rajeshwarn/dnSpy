﻿/*
    Copyright (C) 2014-2017 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;

namespace dnSpy.Debugger.DotNet.Metadata {
	/// <summary>
	/// Type name
	/// </summary>
	public struct DmdTypeName {
		/// <summary>
		/// Namespace or null
		/// </summary>
		public string Namespace;

		/// <summary>
		/// Name
		/// </summary>
		public string Name;

		/// <summary>
		/// Nested type names, separated with '+'
		/// </summary>
		public string Extra;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="namespace">Namespace or null</param>
		/// <param name="name">Name</param>
		public DmdTypeName(string @namespace, string name) {
			Namespace = @namespace;
			Name = name;
			Extra = null;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="namespace">Namespace or null</param>
		/// <param name="name">Name</param>
		/// <param name="extra">Nested type names, separated with '+'</param>
		public DmdTypeName(string @namespace, string name, string extra) {
			Namespace = @namespace;
			Name = name;
			Extra = extra;
		}

		/// <summary>
		/// Creates a <see cref="DmdTypeName"/>
		/// </summary>
		/// <param name="type">Type</param>
		/// <returns></returns>
		public static DmdTypeName Create(DmdType type) {
			if (type.TypeSignatureKind == DmdTypeSignatureKind.Type) {
				var declType = type.DeclaringType;
				if ((object)declType == null)
					return new DmdTypeName(type.MetadataNamespace, type.MetadataName);

				if ((object)declType.DeclaringType == null)
					return new DmdTypeName(declType.MetadataNamespace, declType.MetadataName, type.MetadataName);

				var list = ListCache<DmdType>.AllocList();
				for (;;) {
					if ((object)type.DeclaringType == null)
						break;
					list.Add(type);
					type = type.DeclaringType;
				}
				var sb = ObjectCache.AllocStringBuilder();
				for (int i = list.Count - 1; i >= 0; i--) {
					if (i != list.Count - 1)
						sb.Append('+');
					sb.Append(list[i].MetadataName);
				}
				ListCache<DmdType>.Free(ref list);
				return new DmdTypeName(type.MetadataNamespace, type.MetadataName, ObjectCache.FreeAndToString(ref sb));
			}

			return new DmdTypeName(null, string.Empty);
		}

		/// <summary>
		/// Gets the type name
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			if (Namespace == null) {
				if (Extra == null)
					return Name;
				return Name + "+" + Extra;
			}
			if (Extra == null)
				return Namespace + "." + Name;
			return Namespace + "." + Name + "+" + Extra;
		}
	}

	/// <summary>
	/// <see cref="DmdTypeName"/> equality comparer
	/// </summary>
	public sealed class DmdTypeNameEqualityComparer : IEqualityComparer<DmdTypeName> {
		/// <summary>
		/// Gets the single instance
		/// </summary>
		public static readonly DmdTypeNameEqualityComparer Instance = new DmdTypeNameEqualityComparer();
		DmdTypeNameEqualityComparer() { }

		/// <summary>
		/// Equals()
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool Equals(DmdTypeName x, DmdTypeName y) =>
			x.Name == y.Name &&
			x.Namespace == y.Namespace &&
			x.Extra == y.Extra;

		/// <summary>
		/// GetHashCode()
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(DmdTypeName obj) =>
			StringComparer.Ordinal.GetHashCode(obj.Namespace ?? string.Empty) ^
			StringComparer.Ordinal.GetHashCode(obj.Name ?? string.Empty) ^
			StringComparer.Ordinal.GetHashCode(obj.Extra ?? string.Empty);
	}
}
