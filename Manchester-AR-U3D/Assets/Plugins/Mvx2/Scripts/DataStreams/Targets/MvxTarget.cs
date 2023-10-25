using UnityEngine;

namespace MVXUnity
{
    public abstract class MvxTarget : MonoBehaviour
    {
        #region graph targets

        public abstract Mvx2API.GraphNode GetGraphNode();

        public static Mvx2API.GraphNode[] TransformMvxTargetsToGraphNodes(MvxTarget[] targets)
        {
            if (targets == null)
                return null;

            Mvx2API.GraphNode[] graphTargets = new Mvx2API.GraphNode[targets.Length];
            for (int i = 0; i < targets.Length; i++)
                graphTargets[i] = targets[i].GetGraphNode();

            return graphTargets;
        }

        #endregion
    }
}
