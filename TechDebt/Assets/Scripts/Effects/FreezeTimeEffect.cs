namespace Effects
{
    public class FreezeTimeEffect: EffectBase
    {
        public override void OnFinish()
        {
            GameManager.Instance.NetworkPacketState = GameManager.GlobalNetworkPacketState.Running;
        }
    }
}