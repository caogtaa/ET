namespace ET.Client
{
    public static partial class SceneChangeHelper
    {
        // 场景切换协程
        public static async ETTask SceneChangeTo(Scene root, string sceneName, long sceneInstanceId)
        {
            root.RemoveComponent<AIComponent>();
            
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose(); // 删除之前的CurrentScene，创建新的
            Scene currentScene = CurrentSceneFactory.Create(sceneInstanceId, sceneName, currentScenesComponent);
            UnitComponent unitComponent = currentScene.AddComponent<UnitComponent>();
         
            // 可以订阅这个事件中创建Loading界面
            // GT: 这里实际上没有等待，内部coroutine了。如果要等待要用await PublishAsync
            // 所以目前的设计里，如果场景较大加载慢的时候，可能会业务会提前跑起来
            // GT: 加载场景和创建UI、实例化Unit并不冲突，因为UI和Unit都挂载在Global的组件下
            EventSystem.Instance.Publish(root, new SceneChangeStart());
            // await EventSystem.Instance.PublishAsync(root, new SceneChangeStart());
            
            // 等待CreateMyUnit的消息
            Wait_CreateMyUnit waitCreateMyUnit = await root.GetComponent<ObjectWait>().Wait<Wait_CreateMyUnit>();
            M2C_CreateMyUnit m2CCreateMyUnit = waitCreateMyUnit.Message;
            Unit unit = UnitFactory.Create(currentScene, m2CCreateMyUnit.Unit);
            unitComponent.Add(unit);
            root.RemoveComponent<AIComponent>();
            
            EventSystem.Instance.Publish(currentScene, new SceneChangeFinish());
            // 通知等待场景切换的协程
            root.GetComponent<ObjectWait>().Notify(new Wait_SceneChangeFinish());
        }
    }
}