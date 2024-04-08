namespace ET.Client
{
    // 实体组件通用的System标记，必须指定类型
    // 必须用static partial标记类，因为ET会自动生成IAwake等接口的声明周期的分类
    [EntitySystemOf(typeof(Computer))]
    public static partial class ComputerSystem
    {
        [EntitySystem]
        private static void Awake(this Computer self)
        {
            Log.Info("Computer Awake");
        }

        [EntitySystem]
        private static void Update(this Computer self)
        {
            // Log.Info("Computer Update");
        }

        [EntitySystem]
        private static void Destroy(this Computer self)
        {
            Log.Info("Computer Destroy");
        }

        //自己编写的给外部调用的测试方法
        public static void Open(this Computer self)
        {
            Log.Info("Computer Open");
        }
    }
}

namespace ET.Client
{
    [EntitySystemOf(typeof(PCCaseComponent))]
    public static partial class PCCaseComponentSystem
    {
        [EntitySystem]
        private static void Awake(this PCCaseComponent self)
        {
            Log.Info("PCCaseComponent Awake");
        }
    }
}

namespace ET.Client
{
    [EntitySystemOf(typeof(MonitorComponent))]
    // 数据修改友好标记，允许修改指定类型上的数据
    [FriendOf(typeof(ET.Client.MonitorComponent))]
    public static partial class MonitorComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.MonitorComponent self, int brightness)
        {
            Log.Info("MonitorComponent Awake");
            
            // 修改亮度
            self.Brightness = brightness;
        }
        
        [EntitySystem]
        private static void Destroy(this ET.Client.MonitorComponent self)
        {
            Log.Info("MonitorComponent Destroy");
        }

        public static void ChangeBrightness(this MonitorComponent self, int value)
        {
            self.Brightness = value;
        }
    }
}