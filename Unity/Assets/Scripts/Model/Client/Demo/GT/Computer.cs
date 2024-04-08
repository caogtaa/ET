namespace ET.Client {
    // 该实体类属于指定的实体类，如果为唯一则添加typeof指定，任意父实体就不需要。
    [ChildOf(typeof(ComputersComponent))]
    // 无论是实体还是组件，都必须继承Entity
    // 而IAwake IUpdate IDestroy是生命周期函数，按需添加即可
    public class Computer : Entity, IAwake, IUpdate, IDestroy {
        
    }
}

namespace ET.Client
{
    [ComponentOf(typeof(Computer))]
    public class PCCaseComponent : Entity,IAwake
    {
    
    }
}

namespace ET.Client
{
    [ComponentOf(typeof(Computer))]
    // IAwake允许传入指定的参数，最多四个（可在源码里按需添加）
    public class MonitorComponent : Entity, IAwake<int>, IDestroy
    {
        public int Brightness;
    }
}

namespace ET.Client
{
    // 该组件属于场景根节点
    // 挂载电脑实体的集合组件
    // GT: 所有组件必须继承IAwake，即使没有对应的System实现Awake
    [ComponentOf(typeof(Scene))]
    public class ComputersComponent : Entity, IAwake
    {
    
    }
}
