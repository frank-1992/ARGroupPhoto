using UnityEngine;

namespace MVXUnity
{
    [AddComponentMenu("Mvx2/Targets/File Target")]
    public class MvxFileTarget : MvxTarget
    {
        #region data

        [SerializeField] public string absoluteFilePath;

        #endregion

        #region graph targets

        public override Mvx2API.GraphNode GetGraphNode()
        {
            return new Mvx2BasicIO.Mvx2FileWriterGraphNode(absoluteFilePath);
        }

        #endregion
    }
}
