using System.Collections.Generic;
using System.Reflection;
using System;

namespace ET
{
    /// <summary>
    /// 设计目的:
    /// 1. 启动阶段保存全量FQN->Type反射，运行阶段通过此类直接查询，不用再走反射
    /// 2. 搜集所有ET系统的标注，反向映射到Type，运行时可以查询
    /// 3. CreateCode里实例化所有[Code]标注的类型，并设为全局单例
    /// </summary>
    public class CodeTypes: Singleton<CodeTypes>, ISingletonAwake<Assembly[]>
    {
        // 保存FQN到类型的映射
        private readonly Dictionary<string, Type> allTypes = new();
        
        // 保存标注 -> 宿主类型的映射
        private readonly UnOrderMultiMapSet<Type, Type> types = new();
        
        /// <summary>
        /// 扫描所有assembly里面的类型，提取类型的BaseAttribute标注
        /// 保存标注 -> 类型的映射。容器类型是 Dictionary<Type, HashSet<Type>>
        ///     其中key是BaseAttribute或者其派生类，value是业务类型的集合
        /// 猜测主要目的是启动阶段跑一次全量的类型反射，后面找类型就不通过反射来查了
        /// </summary>
        /// <param name="assemblies"></param>
        public void Awake(Assembly[] assemblies)
        {
            Dictionary<string, Type> addTypes = AssemblyHelper.GetAssemblyTypes(assemblies);
            foreach ((string fullName, Type type) in addTypes)
            {
                this.allTypes[fullName] = type;
                
                if (type.IsAbstract)
                {
                    continue;
                }
                
                // 记录所有的有BaseAttribute标记的的类型
                // 比如EntitySystem是继承BaseAttribute的，也会包含进来
                object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);

                foreach (object o in objects)
                {
                    this.types.Add(o.GetType(), type);
                }
            }
        }
        
        /// <summary>
        /// 获取有指定标注的所有类型
        /// 比如这个标注可以是EntitySystem、Event
        /// </summary>
        /// <param name="systemAttributeType"></param>
        /// <returns></returns>
        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!this.types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return this.types[systemAttributeType];
        }
        
        /// <summary>
        /// 返回的是所有的FQN -> Type映射表
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }
        
        /// <summary>
        /// 通过FQN获取类型
        /// 在Awake里一次性dump出来了，再次获取直接内存查找不需要再string -> type进行反射
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public Type GetType(string typeName)
        {
            return this.allTypes[typeName];
        }
        
        /// <summary>
        /// 实例化所有[Code]标注的类，并加入全局单例
        /// </summary>
        public void CreateCode()
        {
            var hashSet = this.GetTypes(typeof (CodeAttribute));
            foreach (Type type in hashSet)
            {
                object obj = Activator.CreateInstance(type);
                ((ISingletonAwake)obj).Awake();
                World.Instance.AddSingleton((ASingleton)obj);
            }
        }
    }
}