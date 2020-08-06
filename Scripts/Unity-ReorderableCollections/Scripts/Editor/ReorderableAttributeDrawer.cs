using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ZeroVector.Common.Reorderable {
    using Internal;

    namespace Editor {
        [CustomPropertyDrawer(typeof(ReorderableAttribute))]
        [CustomPropertyDrawer(typeof(Internal.BaseReorderableCollection), true)]
        public class ReorderableAttributeDrawer : PropertyDrawer {
            private const string DefaultBackingListName = "items";

            private static readonly Dictionary<int, ReorderableCollection> Lists =
                new Dictionary<int, ReorderableCollection>();

            public override bool CanCacheInspectorGUI(SerializedProperty property) {
                return false;
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
                var collection = GetDrawableCollection(property, attribute as ReorderableAttribute);
                return collection?.GetHeight() ?? EditorGUIUtility.singleLineHeight * 2;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                var collection = GetDrawableCollection(property, attribute as ReorderableAttribute);

                if (collection != null) {
                    // fix nested drawers
                    var mode = EditorGUIUtility.hierarchyMode;
                    EditorGUIUtility.hierarchyMode = true;
                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;

                    collection.DoUIList(EditorGUI.IndentedRect(position), label);

                    EditorGUI.indentLevel = indent;
                    EditorGUIUtility.hierarchyMode = mode;
                }
                else {
                    GUI.Label(position, "Error: collection must extend BaseReorderableCollection and\n" +
                                        "must contain a drawable, serializable list named \"items\".",
                        EditorStyles.label);
                }
            }

            private static int GetListId(SerializedProperty property) {
                if (property == null) return 0;
                var h1 = property.serializedObject.targetObject.GetHashCode();
                var h2 = property.propertyPath.GetHashCode();

                return ((h1 << 5) + h1) ^ h2;
            }

            public static ReorderableCollection GetDrawableCollection(SerializedProperty property,
                ReorderableAttribute attrib = null, int id = -1, string backingListName = DefaultBackingListName) {
                //
                if (property == null) {
                    return null;
                }

                if (id == -1) {
                    id = GetListId(property);
                }

                if (attrib == null) {
                    attrib = new ReorderableAttribute();
                }


                ReorderableCollection collection = null;
                var backingList = property.FindPropertyRelative(backingListName);

                var obj = GetTargetObjectOfProperty(property);

                if (backingList == null || !backingList.isArray || !(obj is BaseReorderableCollection)) return null;

                if (!Lists.TryGetValue(id, out collection)) {
                    var icon = !string.IsNullOrEmpty(attrib.elementIconPath)
                        ? AssetDatabase.GetCachedIcon(attrib.elementIconPath)
                        : null;

                    var displayType = attrib.singleLine
                        ? ReorderableCollection.ElementDisplayType.SingleLine
                        : ReorderableCollection.ElementDisplayType.Auto;

                    collection = new ReorderableCollection(backingList, attrib.add, attrib.remove, attrib.draggable,
                        displayType,
                        attrib.elementNameProperty, attrib.elementNameOverride, icon) {
                        Paginate = attrib.paginate, PageSize = attrib.pageSize, sortable = attrib.sortable
                    };

                    // handle surrogate if any
                    if (attrib.surrogateType != null) {
                        var callback = new SurrogateCallback(attrib.surrogateProperty);
                        collection.surrogate =
                            new ReorderableCollection.Surrogate(attrib.surrogateType, callback.SetReference);
                    }

                    Lists.Add(id, collection);
                }
                else {
                    collection.List = backingList;
                }

                return collection;
            }

            private struct SurrogateCallback {
                private readonly string property;

                internal SurrogateCallback(string property) {
                    this.property = property;
                }

                internal void SetReference(SerializedProperty element, Object objectReference,
                    ReorderableCollection collection) {
                    var prop = !string.IsNullOrEmpty(property) ? element.FindPropertyRelative(property) : null;

                    if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference) {
                        prop.objectReferenceValue = objectReference;
                    }
                }
            }


            /// <summary>
            /// Gets the object the property represents.
            /// </summary>
            /// <param name="prop"></param>
            /// <returns></returns>
            public static object GetTargetObjectOfProperty(SerializedProperty prop) {
                if (prop == null) return null;

                var path = prop.propertyPath.Replace(".Array.data[", "[");
                object obj = prop.serializedObject.targetObject;
                var elements = path.Split('.');
                foreach (var element in elements) {
                    if (element.Contains("[")) {
                        var elementName = element.Substring(0, element.IndexOf("["));
                        var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "")
                            .Replace("]", ""));
                        obj = GetValue_Imp(obj, elementName, index);
                    }
                    else {
                        obj = GetValue_Imp(obj, element);
                    }
                }

                return obj;
            }

            private static object GetValue_Imp(object source, string name) {
                if (source == null)
                    return null;
                var type = source.GetType();

                while (type != null) {
                    var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (f != null)
                        return f.GetValue(source);

                    var p = type.GetProperty(name,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (p != null)
                        return p.GetValue(source, null);

                    type = type.BaseType;
                }

                return null;
            }

            private static object GetValue_Imp(object source, string name, int index) {
                var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
                if (enumerable == null) return null;
                var enm = enumerable.GetEnumerator();
                //while (index-- >= 0)
                //    enm.MoveNext();
                //return enm.Current;

                for (int i = 0; i <= index; i++) {
                    if (!enm.MoveNext()) return null;
                }

                return enm.Current;
            }
        }
    }
}