using UnityEngine;
using System;

namespace ZeroVector.Common.Reorderable {

	public class ReorderableAttribute : PropertyAttribute {

		public bool add;
		public bool remove;
		public bool draggable;
		public bool singleLine;
		public bool paginate;
		public bool sortable;
		public int pageSize;
		public string elementNameProperty;
		public string elementNameOverride;
		public string elementIconPath;
		public Type surrogateType;
		public string surrogateProperty;

		public ReorderableAttribute(string elementNameProperty = null, string elementNameOverride = null,
			string elementIconPath = null, bool add = true, bool remove = true, bool draggable = true) {
			//
			this.add = add;
			this.remove = remove;
			this.draggable = draggable;
			this.elementNameProperty = elementNameProperty;
			this.elementNameOverride = elementNameOverride;
			this.elementIconPath = elementIconPath;

			sortable = true;
		}
	}
}
