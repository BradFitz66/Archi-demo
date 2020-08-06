using System;

namespace ZeroVector.Common.Reorderable.Internal {
    
    /// <summary>
    /// Dummy class that all reorderable collections should inherit from in order to be drawn as reorderable in
    /// the Unity Inspector. Do not inherit from this, unless you're making a new generic reorderable collection.
    /// </summary>
    [Serializable]
    public abstract class BaseReorderableCollection { }
    
    
}