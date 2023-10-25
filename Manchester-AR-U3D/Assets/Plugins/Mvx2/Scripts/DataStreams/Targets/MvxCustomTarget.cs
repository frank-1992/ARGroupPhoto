using UnityEngine;

namespace MVXUnity
{
    [AddComponentMenu("Mvx2/Targets/Custom Target")]
    public class MvxCustomTarget : MvxTarget
    {
        #region data

        [Tooltip("E.g. A276D191-541F-442F-B2CF-6FD7116EFFEE")]
        [SerializeField] public string targetFilterGUID;
        [SerializeField] public MvxParam[] targetFilterParams = null;

        #endregion

        #region graph targets

        public override Mvx2API.GraphNode GetGraphNode()
        {
            Mvx2API.SingleFilterGraphNode singleFilterGraphNode = new Mvx2API.SingleFilterGraphNode(MVCommon.Guid.FromHexString(targetFilterGUID));
            foreach (var targetFilterParam in targetFilterParams)
                singleFilterGraphNode.SetFilterParameterValue(targetFilterParam.key, targetFilterParam.val);
            return singleFilterGraphNode;
        }

        #endregion
    }
}
