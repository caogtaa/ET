using System;

namespace ET
{
    public interface ISystemType
    {
        // 组件类
        Type Type();
        
        // 系统类，如AwakeSystem/UpdateSystem
        Type SystemType();
        
        // Update/LateUpdate/Load
        int GetInstanceQueueIndex();
    }
}