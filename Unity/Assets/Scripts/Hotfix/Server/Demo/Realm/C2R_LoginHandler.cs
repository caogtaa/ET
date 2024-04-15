using System;
using System.Net;
using System.Collections.Generic;

namespace ET.Server
{
	[MessageSessionHandler(SceneType.Realm)]
	[FriendOf(typeof(AccountInfo))]
	public class C2R_LoginHandler : MessageSessionHandler<C2R_Login, R2C_Login>
	{
		protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response) {
#if !ABCD
			await ETTask.CompletedTask;
#else
			if (string.IsNullOrWhiteSpace(request.Account) || string.IsNullOrWhiteSpace(request.Password)) {
				response.Error = ErrorCode.ERR_LoginInfoEmpty;
				CloseSession(session).Coroutine();
				return;
			}
			
			// 根据账户名hash加协程锁
			using (await session.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.LoginAccount, request.Account.GetLongHashCode()))
			{
				DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());

				// 获取所有同名账户
				List<AccountInfo> accountInfos = await dbComponent.Query<AccountInfo>(
					accountInfo => string.Equals(accountInfo.Account, request.Account));
				if (accountInfos.Count <= 0) {
					// 没有账户时自动注册
					var accountInfosComp = session.GetComponent<AccountInfosComponent>() ?? session.AddComponent<AccountInfosComponent>();
					var newAccount = accountInfosComp.AddChild<AccountInfo>();
					newAccount.Account = request.Account;
					newAccount.Password = request.Password;
					await dbComponent.Save(newAccount);
				}
				else {
					// 有账户时判断密码是否相等
					var accountInfo = accountInfos[0];
					if (!string.Equals(accountInfo.Password, request.Password)) {
						response.Error = ErrorCode.ERR_LoginPasswordError;
						CloseSession(session).Coroutine();
						return;
					}
				}
			}

			// 随机分配一个Gate
			StartSceneConfig config = RealmGateAddressHelper.GetGate(session.Zone(), request.Account);
			Log.Debug($"gate address: {config}");
			
			// 向gate请求一个key,客户端可以拿着这个key连接gate
			R2G_GetLoginKey r2GGetLoginKey = R2G_GetLoginKey.Create();
			r2GGetLoginKey.Account = request.Account;
			G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await session.Fiber().Root.GetComponent<MessageSender>().Call(
				config.ActorId, r2GGetLoginKey);

			response.Address = config.InnerIPPort.ToString();
			response.Key = g2RGetLoginKey.Key;
			response.GateId = g2RGetLoginKey.GateId;
			
			CloseSession(session).Coroutine();
#endif
		}

		private async ETTask CloseSession(Session session)
		{
			await session.Root().GetComponent<TimerComponent>().WaitAsync(1000);
			session.Dispose();
		}
	}
}
