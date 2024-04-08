namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene root, string account, string password)
        {
            root.RemoveComponent<ClientSenderComponent>();
            
            // 初始化ClientSender组件，专用于发送客户端请求，Awake里啥也没做
            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            
            // LoginAsync里才开始创建ThreadPool的Fiber
            // TODO: GT: 如果登录失败，Fiber会残留在内部，下次重走登录流程会抛异常。应当清理Fiber或者复用旧的
            long playerId = await clientSenderComponent.LoginAsync(account, password);

            root.GetComponent<PlayerComponent>().MyId = playerId;
            
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}