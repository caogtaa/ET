using System.Reflection;

namespace ET
{
    /// <summary>
    /// 简单封装了一下反射方式使用方法
    /// 最多支持3个参数的方法调用
    /// 注意：这里要手动指定正确的assembly，没有在domain里搜索
    /// 注意：typeName要求带namespace，需要是FQN？
    /// </summary>
    public class StaticMethod : IStaticMethod
    {
        private readonly MethodInfo methodInfo;

        private readonly object[] param;

        public StaticMethod(Assembly assembly, string typeName, string methodName)
        {
            this.methodInfo = assembly.GetType(typeName).GetMethod(methodName);
            this.param = new object[this.methodInfo.GetParameters().Length];
        }

        public override void Run()
        {
            this.methodInfo.Invoke(null, param);
        }

        public override void Run(object a)
        {
            this.param[0] = a;
            this.methodInfo.Invoke(null, param);
        }

        public override void Run(object a, object b)
        {
            this.param[0] = a;
            this.param[1] = b;
            this.methodInfo.Invoke(null, param);
        }

        public override void Run(object a, object b, object c)
        {
            this.param[0] = a;
            this.param[1] = b;
            this.param[2] = c;
            this.methodInfo.Invoke(null, param);
        }
    }
}

