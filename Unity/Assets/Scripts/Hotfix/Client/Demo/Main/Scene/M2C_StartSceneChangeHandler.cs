namespace ET.Client
{
	// 用于接收服务器下发的M2C消息，所有只有MessageType，没有ResponseType
	// M2C_StartSceneChange由C2G_EnterMap触发
	[MessageHandler(SceneType.Demo)]
	public class M2C_StartSceneChangeHandler : MessageHandler<Scene, M2C_StartSceneChange>
	{
		protected override async ETTask Run(Scene root, M2C_StartSceneChange message)
		{
			await SceneChangeHelper.SceneChangeTo(root, message.SceneName, message.SceneInstanceId);
		}
	}
}
