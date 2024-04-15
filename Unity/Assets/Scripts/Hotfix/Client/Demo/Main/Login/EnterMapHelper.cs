using System;


namespace ET.Client
{
    public static partial class EnterMapHelper
    {
        public static async ETTask EnterMapAsync(Scene root)
        {
            try
            {
                // GT: 思考一个问题，在这里你怎么知道root现在具备ClientSenderComponent组件？本质上是存在过程耦合
                // 按照作者对可插拔有的偏执，这些组件并不直接长在Model身上，而是每次都通过GetComponent获取，这样设计的问题
                // 1. 层级混乱。业务概念和底层技术概念全揉到一起了，架构上没有分层
                // 2. 从数据角度来看，Component本身是把一些字段组合到一起放到root而已。如果把字段打碎了分别存储，那么这套Component的动态特性就和TypeScript很接近
                //  本质上也是记录了状态数据，当状态数量(Components)变多时，也会变得难以管理。目前已经出现多个生产和释放时机不配对的组件
                // 3. System是一系列无状态的静态方法，Extension本质上是语法糖。这种设计更像是没有OOP的C了
                G2C_EnterMap g2CEnterMap = await root.GetComponent<ClientSenderComponent>().Call(C2G_EnterMap.Create()) as G2C_EnterMap;
                
                // 等待场景切换完成
                await root.GetComponent<ObjectWait>().Wait<Wait_SceneChangeFinish>();
                
                EventSystem.Instance.Publish(root, new EnterMapFinish());
            }
            catch (Exception e)
            {
                Log.Error(e);
            }	
        }
        
        public static async ETTask Match(Fiber fiber)
        {
            try
            {
                G2C_Match g2CEnterMap = await fiber.Root.GetComponent<ClientSenderComponent>().Call(C2G_Match.Create()) as G2C_Match;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }	
        }
    }
}