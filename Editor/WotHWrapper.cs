using Nomnom.UnityProjectPatcher.Editor;
using Nomnom.UnityProjectPatcher.Editor.Steps;

namespace Skydorm.WotHProjectPatcher.Editor {
    [UPPatcher("com.skydorm.unity-woth-project-patcher")]
    public static class WotHWrapper 
    {
        public static void GetSteps(StepPipeline stepPipeline) 
        {
            stepPipeline.SetInputSystem(InputSystemType.Both);

            stepPipeline.InsertLast(new WotHClearUpFiles());
            stepPipeline.InsertLast(new WotHAddressables());
            stepPipeline.InsertLast(new WotHAssetCopyStep());
            stepPipeline.InsertLast(new WotHAddressPathChange());
            stepPipeline.InsertLast(new WotHShaderPatchStep());
            stepPipeline.InsertLast(new WotHSourcePatchStep());
            stepPipeline.InsertLast(new WotHFebucciStep());
        }
    }
}