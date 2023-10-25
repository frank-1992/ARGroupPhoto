using UnityEngine;

namespace MVXUnity
{
    [CreateAssetMenu(fileName = "CustomDataStreamDefinition", menuName = "Mvx2/Data Stream Definitions/Custom Data Stream Definition")]
    public class MvxCustomDataStreamDefinition : MvxDataStreamDefinition
    {
        #region data

        [Tooltip("E.g. A276D191-541F-442F-B2CF-6FD7116EFFEE")]
        [SerializeField] public string sourceFilterGUID;
        [SerializeField] public MvxParam[] sourceFilterParams = null;

        public void Apply()
        {
            onDefinitionChanged.Invoke();
        }

        #endregion

        #region graph node

        public override Mvx2API.GraphNode GetSourceGraphNode()
        {
            Mvx2API.SingleFilterGraphNode singleFilterGraphNode = new Mvx2API.SingleFilterGraphNode(MVCommon.Guid.FromHexString(sourceFilterGUID));
            foreach (var sourceFilterParam in sourceFilterParams)
                singleFilterGraphNode.SetFilterParameterValue(sourceFilterParam.key, sourceFilterParam.val);
            return singleFilterGraphNode;
        }

        #endregion
    }
}
