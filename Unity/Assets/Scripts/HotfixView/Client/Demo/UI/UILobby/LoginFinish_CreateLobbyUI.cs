namespace ET.Client
{
	[Event(SceneType.Demo)]
	public class LoginFinish_CreateLobbyUI: AEvent<Scene, LoginFinish>
	{
		protected override async ETTask Run(Scene scene, LoginFinish args)
		{
			// GT: UI的转场呢？
			await UIHelper.Create(scene, UIType.UILobby, UILayer.Mid);
		}
	}
}
