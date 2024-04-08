using MemoryPack;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace ET
{
    public struct EntryEvent1
    {
    }   
    
    public struct EntryEvent2
    {
    } 
    
    public struct EntryEvent3
    {
    }
    
    public static class Entry
    {
        public static void Init()
        {
            
        }
        
        public static void Start()
        {
            StartAsync().Coroutine();
        }
        
        private static async ETTask StartAsync()
        {
            // 根据平台不同初始化时间精度
            // 这里特指将Windows平台的时间精度设置为1ms（以为默认粒度较大）
            // 另外注意只要一个process设置了精度，系统就会让所有process都使用设置的最小值（最高精度），这么做会提升整体的系统负载
            //     https://learn.microsoft.com/en-us/windows/win32/api/timeapi/nf-timeapi-timebeginperiod
            WinPeriod.Init();

            // 注册Mongo type
            MongoRegister.Init();
            // 注册Entity序列化器
            EntitySerializeRegister.Init();
            
            // 这些是无需热重载的通用组件
            World.Instance.AddSingleton<IdGenerater>();
            World.Instance.AddSingleton<OpcodeType>();
            World.Instance.AddSingleton<ObjectPool>();
            World.Instance.AddSingleton<MessageQueue>();
            World.Instance.AddSingleton<NetServices>();
            World.Instance.AddSingleton<NavmeshComponent>();
            World.Instance.AddSingleton<LogMsg>();
            
            // 创建需要reload的code singleton
            // GT: 此处会将所有[Code]标注的类型实例化，并作为全局单例
            // 目前有[Code]标注的类型有:
            //      ET.Server.HttpDispatcher
            //      ET.EntitySystemSingleton
            //      ET.MessageDispatcher
            //      ET.EventSystem
            //      ET.LSEntitySystemSingleton, (LS for LockStep)
            //      ET.AIDispatcherComponent
            //      ET.ConsoleDispatcher
            //      ET.MessageSessionDispatcher
            //      ET.NumericWatcherComponent
            //      ET.Client.UIEventComponent
            // 在热重载时也会这么干一次
            CodeTypes.Instance.CreateCode();
            
            // 异步加载所有[Config]标注的配置类，反序列化对应配置，并设为全局单例(这里值的是AIConfig这种配置类设为单例)
            await World.Instance.AddSingleton<ConfigLoader>().LoadAsync();

            await FiberManager.Instance.Create(SchedulerType.Main, ConstFiberId.Main, 0, SceneType.Main, "");
        }
    }
}