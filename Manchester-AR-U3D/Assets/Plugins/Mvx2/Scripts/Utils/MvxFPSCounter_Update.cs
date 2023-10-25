namespace MVXUnity
{
    public class MvxFPSCounter_Update : MvxFPSCounter
    {
        protected override void Update()
        {
            SnapFrame();
            base.Update();
        }
    }
}