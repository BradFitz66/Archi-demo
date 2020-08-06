# Reorderable Collections (Lists AND Dictionaries!)

An extended version of Chris Foulston's excellent [Unity-Reorderable-List](https://github.com/cfoulston/Unity-Reorderable-List) project.

![image](https://user-images.githubusercontent.com/17273782/72189294-c817ce80-33fc-11ea-8c71-423fce6d2647.png)


## New Features
* **Built-in Dictionary support** with _full_ `Dictionary<TKey, TValue>` functionality. It also supports all the bells and whistles supported on the list drawers, such as pagination, sorting, and whatnot. Go wild.
* **Full List functionality**, too. Instead of implementing only a subset of the regular List functionality as the old "ReorderableArray" type did, the new ReorderableList now implements _all_ features, properties, and interfaces that a `List<T>` has.
* **Attributes no longer mandatory.** Any class that derives from the respective base classes will be drawn correctly in the Inspector. The `[Reorderable]` attribute can still be used to customise the a specific field's display settings.

* Changed internal naming to make it less confusing. `ReorderableArray`, the actual data structure backed by a _list_, has been rewritten from scratch and is now called `ReorderableList`, and the internal Editor-only class `ReorderableList` has been moved to `ReorderableCollection`, given that it can draw any reorderable collection. 


## Extant features
_(This list has been copy-pasted from the source repo.)_

* Drag and Drop references (like array inspector)
* Expandable items and list itself
* Multiple selection (ctrl/command, shift select)
* Draggable selection
* Context menu items (revert values, duplicate values, delete values)
* Custom attribute which allows automatic list generation for properties*
* Event delegates and custom styling
* Pagination
* Sorting (sort based on field, ascending and descending)
* Surrogates (Enable adding elements of a different type)


## Why use this one?
Because having dictionaries in the Inspector is useful. For lists, you can happily use the [parent repo](https://github.com/cfoulston/Unity-Reorderable-List), but dictionaries are a different beast entirely. The only available solution which I can sincerely recommend is Rotary Heart's brilliant [Serialised Dictionary](https://assetstore.unity.com/packages/tools/utilities/serialized-dictionary-lite-110992), which is based on the same base reorderable list repo but relies on a very special, complex property drawer instead, and that one's pretty difficult to customise, I can tell you that for a fact.

This, however, is a unified solution whicn relies on simple rather than complicated approaches, and which won't have you running two different "reorderable collection drawers" in different namespaces. Naturally, all listed options are free, so feel free to pick and choose whichever you like most.

## Usage
Unity is unable to serialise generic types, so it's always going to be necessary to create custom classes.  _Types not serialised by Unity can't be used for underlying collection types._ Also, both examples listed here can be found in the ScriptableObjectExample present in the repo.

### Lists?
Lists are easy. Simply make a new non-generic class that derives from `ReorderableList<T>`, just like you do with normal lists.

For the list in the screenshot above, we have:
```csharp
public MyList list;

[Serializable]
public class MyList : ReorderableList<MyObject> { }
```
where `MyObject` is a struct containing three fields -- a bool, a float, and a string.

### Dictionaries?
Dictionaries are still easy, though just a bit more complicated, because need to take more things into account. For the one in the screenshot above, we have the following:

```csharp
[Reorderable(paginate = true, pageSize = 0, elementNameProperty = "Value")]
public MyDict dict;


[Serializable]
public class MyDict : ReorderableDictionary<float, string, MyDict.KeyValuePair> {
    public override float DeduplicateKey(float duplicateKey) {
        return duplicateKey + 0.1f;
    }
    
    [Serializable]
    public new class KeyValuePair : ReorderableDictionary<float, string, KeyValuePair>.KeyValuePair { }
}
```
So, what's going on here?
* Dicts **must** inherit from `ReorderableDictionary<TKey, TValue, TContainer>` and must be flagged serialisable as normal.
* Dicts **must** also implement the `DeduplicateKey` method (takes and returns a `TKey`), which tells the dictionary how to mutate an existing key. _(Dictionaries must have unique keys, and it's smarter to force the implementation to do it instead of trying to come up with some "generic" way on the base level.)_ In this case, adding a new element will increment the last-added key by 0.1.
* You **must** also create a serialisable `TContainer` class that inherits from `ReorderableDictionary<TKey, TValue, TContainer>.KeyValuePair`. This class is used to draw the individual key-value pair elements. This is convoluted, yes, but allows you to create a custom property drawer for this class alone, and thus gives you full control over how your dictionaries look. You can put the special key-value pairs right in the base class to keep it slightly less cluttered.

Here are some examples of Inspector-drawn dicts obtained using custom property drawers for value and key-value pair classes:

![image](https://user-images.githubusercontent.com/17273782/72190260-5ee58a80-33ff-11ea-81ab-47a667ccc672.png)

(These are from something secret I'm working on...)


## Supporting other collections
You want to add a new collection type? That one's also pretty easy, though it takes a little work. To see how, just have a look at how `ReorderableDictionary` has been implemented here, which is, essentially, the following:
* Open your target collection class with the decompiler;
* Copy-paste it into your `ReorderableWhatever` file;
* Implement everything by pointing towards an internal object of the same type;
* Now wrap your internal object's data into a single `List<T>` named `items` that the extant reorderable drawer can work with. You can use Unity's `ISerializationCallbackReceiver` to make the magic happen.
* That's it, more or less!

## Downsides
The way this is implemented is smarter than some alternatives (e.g. not using `BaseReorderableCollection` which would have forced you, the user, to manually add `CustomPropertyDrawer` tags to editor scripts for every single custom collection you define. That'd be _very_ annoying.

However, an even smarter idea would have been to simply modify the `ReorderableCollection` drawer to be able to draw multiple lists of serialised properties instead of just one. That'd avoid the need for container classes for dictionaries that we observe now. However, I've been writing property drawers for three moths now, and preferred to NOT do that.
