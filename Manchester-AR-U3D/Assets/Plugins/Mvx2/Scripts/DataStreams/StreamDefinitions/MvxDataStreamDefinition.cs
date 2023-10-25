using UnityEngine;
using UnityEngine.Events;

namespace MVXUnity
{
    public abstract class MvxDataStreamDefinition : ScriptableObject
    {
        #region events

        [System.Serializable] public class DefinitionChangedEvent : UnityEvent {}
        [SerializeField, HideInInspector] public DefinitionChangedEvent onDefinitionChanged = new DefinitionChangedEvent();

        #endregion

        #region graph node

        public abstract Mvx2API.GraphNode GetSourceGraphNode();

        #endregion
    }
}
