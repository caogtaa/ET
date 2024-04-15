using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    [Invoke]
    public class GetAllConfigBytes: AInvokeHandler<ConfigLoader.GetAllConfigBytesTrait, ETTask<Dictionary<Type, byte[]>>>
    {
        public override async ETTask<Dictionary<Type, byte[]>> Handle(ConfigLoader.GetAllConfigBytesTrait args)
        {
            Dictionary<Type, byte[]> output = new Dictionary<Type, byte[]>();
            List<string> startConfigs = new List<string>()
            {
                "StartMachineConfigCategory", 
                "StartProcessConfigCategory", 
                "StartSceneConfigCategory", 
                "StartZoneConfigCategory",
            };
            HashSet<Type> configTypes = CodeTypes.Instance.GetTypes(typeof (ConfigAttribute));
            foreach (Type configType in configTypes)
            {
                string configFilePath;
                if (startConfigs.Contains(configType.Name))
                {
                    configFilePath = $"../Config/Excel/s/{Options.Instance.StartConfig}/{configType.Name}.bytes";    
                }
                else
                {
                    configFilePath = $"../Config/Excel/s/{configType.Name}.bytes";
                }
                output[configType] = File.ReadAllBytes(configFilePath);
            }
            
            // GT: 因为没有出现其他异步逻辑，这里为了不报错await调用一个空任务
            await ETTask.CompletedTask;
            return output;
        }
    }
    
    [Invoke]
    public class GetOneConfigBytes: AInvokeHandler<ConfigLoader.GetOneConfigBytesTrait, byte[]>
    {
        public override byte[] Handle(ConfigLoader.GetOneConfigBytesTrait args)
        {
            byte[] configBytes = File.ReadAllBytes($"../Config/Excel/s/{args.ConfigName}.bytes");
            return configBytes;
        }
    }
}