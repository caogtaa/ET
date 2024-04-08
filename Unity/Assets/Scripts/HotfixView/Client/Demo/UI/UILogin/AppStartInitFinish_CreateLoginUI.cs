namespace ET.Client
{
	[Event(SceneType.Demo)]
	public class AppStartInitFinish_CreateLoginUI: AEvent<Scene, AppStartInitFinish>
	{
		protected override async ETTask Run(Scene root, AppStartInitFinish args)
		{
			// UIComponent::Create()
			await UIHelper.Create(root, UIType.UILogin, UILayer.Mid);
			
			// 创建两个电脑实体
			var computers = root.GetComponent<ComputersComponent>();
			var computer1 = computers.AddChild<Computer>();
			var computer2 = computers.AddChild<Computer>();
			
			// 电脑1添加机箱和显示器组件
			computer1.AddComponent<PCCaseComponent>();
			computer1.AddComponent<MonitorComponent, int>(30);
			
			// 电脑开机
			computer1.Open();
			
			// 修改亮度
			computer1.GetComponent<MonitorComponent>().ChangeBrightness(5);

			await root.GetComponent<TimerComponent>().WaitAsync(3000);
			
			computer1?.Dispose();
		}
	}
}
