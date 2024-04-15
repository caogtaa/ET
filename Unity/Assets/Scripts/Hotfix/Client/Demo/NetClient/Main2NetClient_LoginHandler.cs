using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    [MessageHandler(SceneType.NetClient)]
    public class Main2NetClient_LoginHandler: MessageHandler<Scene, Main2NetClient_Login, NetClient2Main_Login>
    {
        /// <summary>
        /// GT: 业务边界完全没有隔离，在业务层里面需要关注各种底层概念，开发人员理解负担极重
        /// </summary>
        /// <param name="root"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        protected override async ETTask Run(Scene root, Main2NetClient_Login request, NetClient2Main_Login response)
        {
            string account = request.Account;
            string password = request.Password;
            // 创建一个ETModel层的Session
            root.RemoveComponent<RouterAddressComponent>();
            // 获取路由跟realmDispatcher地址
            // GT: 为什么取名叫RouterAddress? 不应该是EndPoint吗?
            RouterAddressComponent routerAddressComponent =
                    root.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
            
            // 请求所有router/realm endpoints，保存在routerAddressComponent.Info.Routers/Reamls内
            await routerAddressComponent.Init();
            root.AddComponent<NetComponent, AddressFamily, NetworkProtocol>(routerAddressComponent.RouterManagerIPAddress.AddressFamily, NetworkProtocol.UDP);
            root.GetComponent<FiberParentComponent>().ParentFiberId = request.OwnerFiberId;

            NetComponent netComponent = root.GetComponent<NetComponent>();
            
            // 对account取模后选择realm服务器地址 
            IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);
            
            // 教程里改版后的登录请求，返回Token
            Session session = await netComponent.CreateRouterSession(realmAddress, account, password);
            C2R_LoginAccount c2RLoginAccount = C2R_LoginAccount.Create();
            c2RLoginAccount.AccountName = request.Account;
            c2RLoginAccount.Password = request.Password;
            R2C_LoginAccount r2CLoginAccount = (R2C_LoginAccount)await session.Call(c2RLoginAccount);
            if (r2CLoginAccount.Error == ErrorCode.ERR_Success) {
                root.AddComponent<SessionComponent>().Session = session;
            }
            else {
                session?.Dispose();
            }

            response.Token = r2CLoginAccount.Token;
            response.Error = r2CLoginAccount.Error;

            // R2C_Login r2CLogin;
            // // 创建临时session,和realm服务器通讯
            // // GT: routerSession用account/password算一个hash
            // using (Session session = await netComponent.CreateRouterSession(realmAddress, account, password))
            // {
            //     C2R_Login c2RLogin = C2R_Login.Create();
            //     c2RLogin.Account = account;
            //     c2RLogin.Password = password;
            //     r2CLogin = (R2C_Login)await session.Call(c2RLogin);
            // }
            //
            // if (r2CLogin.Error != ErrorCode.ERR_Success) {
            //     response.Error = r2CLogin.Error;
            //     return;
            // }

            // // 创建一个gate Session,并且保存到SessionComponent中
            // Session gateSession = await netComponent.CreateRouterSession(NetworkHelper.ToIPEndPoint(r2CLogin.Address), account, password);
            // gateSession.AddComponent<ClientSessionErrorComponent>();
            // root.AddComponent<SessionComponent>().Session = gateSession;
            // C2G_LoginGate c2GLoginGate = C2G_LoginGate.Create();
            // c2GLoginGate.Key = r2CLogin.Key;
            // c2GLoginGate.GateId = r2CLogin.GateId;
            // G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(c2GLoginGate);
            //
            // Log.Debug("登陆gate成功!");
            //
            // response.PlayerId = g2CLoginGate.PlayerId;
        }
    }
}