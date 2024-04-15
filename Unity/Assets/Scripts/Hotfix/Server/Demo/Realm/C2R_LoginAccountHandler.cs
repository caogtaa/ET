using System;
using System.Net;
using System.Collections.Generic;

namespace ET.Server
{
	[MessageSessionHandler(SceneType.Realm)]
	[FriendOf(typeof(AccountInfo))]
	public class C2R_LoginAccountHandler : MessageSessionHandler<C2R_LoginAccount, R2C_LoginAccount>
	{
		protected override async ETTask Run(Session session, C2R_LoginAccount request, R2C_LoginAccount response) {
			session.RemoveComponent<SessionAcceptTimeoutComponent>();
			
			// 防重入
			if (session.GetComponent<SessionLockingComponent>() != null) {
				response.Error = ErrorCode.ERR_RequestRepeatedly;
				session.Disconnect().Coroutine();
				return;
			}
			
			if (string.IsNullOrWhiteSpace(request.AccountName) || string.IsNullOrWhiteSpace(request.Password)) {
				response.Error = ErrorCode.ERR_LoginInfoIsNull;
				session.Disconnect().Coroutine();
				return;
			}

			// TODO: 判断Account/Password是否符合规则

			CoroutineLockComponent coroutineLockComponent = session.Root().GetComponent<CoroutineLockComponent>();
			// 防重入，防止同客户端多次并发请求
			using (session.AddComponent<SessionLockingComponent>()) {
				// 协程锁，防止多个客户端Account相同的不同请求
				using (await coroutineLockComponent.Wait(CoroutineLockType.LoginAccount, request.AccountName.GetLongHashCode())) {
					DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
					List<AccountInfo> accountInfos = await dbComponent.Query<AccountInfo>(
						accountInfo => string.Equals(accountInfo.Account, request.AccountName));

					AccountInfo account = null;
					if (accountInfos != null && accountInfos.Count > 0) {
						account = accountInfos[0];
						session.AddChild(account);
						if (account.AccountType == (int)AccountType.BlackList) {
							response.Error = ErrorCode.ERR_AccountInBlackListError;
							session.Disconnect().Coroutine();
							account?.Dispose();
							return;
						}
						
						if (!string.Equals(account.Password, request.Password)) {
							response.Error = ErrorCode.ERR_LoginPasswordError;
							session.Disconnect().Coroutine();
							account?.Dispose();
							return;
						}
					} else {
						account = session.AddChild<AccountInfo>();
						account.Account = request.AccountName;
						account.Password = request.Password;
						account.CreateTime = TimeInfo.Instance.ServerNow();
						account.AccountType = (int)AccountType.General;
						await dbComponent.Save(account);
					}
					
					
				}
			}
		}
		// GT: 以下是从C2R_LoginHandler里copy来的，忽略
		// protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response)
		// {
		// 	if (string.IsNullOrWhiteSpace(request.Account) || string.IsNullOrWhiteSpace(request.Password)) {
		// 		response.Error = ErrorCode.ERR_LoginInfoEmpty;
		// 		CloseSession(session).Coroutine();
		// 		return;
		// 	}
		// 	
		// 	// 根据账户名hash加协程锁
		// 	using (await session.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.LoginAccount, request.Account.GetLongHashCode()))
		// 	{
		// 		DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
		//
		// 		// 获取所有同名账户
		// 		List<AccountInfo> accountInfos = await dbComponent.Query<AccountInfo>(
		// 			accountInfo => string.Equals(accountInfo.Account, request.Account));
		// 		if (accountInfos.Count <= 0) {
		// 			// 没有账户时自动注册
		// 			var accountInfosComp = session.GetComponent<AccountInfosComponent>() ?? session.AddComponent<AccountInfosComponent>();
		// 			var newAccount = accountInfosComp.AddChild<AccountInfo>();
		// 			newAccount.Account = request.Account;
		// 			newAccount.Password = request.Password;
		// 			await dbComponent.Save(newAccount);
		// 		}
		// 		else {
		// 			// 有账户时判断密码是否相等
		// 			var accountInfo = accountInfos[0];
		// 			if (!string.Equals(accountInfo.Password, request.Password)) {
		// 				response.Error = ErrorCode.ERR_LoginPasswordError;
		// 				CloseSession(session).Coroutine();
		// 				return;
		// 			}
		// 		}
		// 	}
		//
		// 	// 随机分配一个Gate
		// 	StartSceneConfig config = RealmGateAddressHelper.GetGate(session.Zone(), request.Account);
		// 	Log.Debug($"gate address: {config}");
		// 	
		// 	// 向gate请求一个key,客户端可以拿着这个key连接gate
		// 	R2G_GetLoginKey r2GGetLoginKey = R2G_GetLoginKey.Create();
		// 	r2GGetLoginKey.Account = request.Account;
		// 	G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await session.Fiber().Root.GetComponent<MessageSender>().Call(
		// 		config.ActorId, r2GGetLoginKey);
		//
		// 	response.Address = config.InnerIPPort.ToString();
		// 	response.Key = g2RGetLoginKey.Key;
		// 	response.GateId = g2RGetLoginKey.GateId;
		// 	
		// 	CloseSession(session).Coroutine();
		// }
		//
		// private async ETTask CloseSession(Session session)
		// {
		// 	await session.Root().GetComponent<TimerComponent>().WaitAsync(1000);
		// 	session.Dispose();
		// }
	}

	public static class DisconnectHelper {
		public static async ETTask Disconnect(this Session self) {
			if (self == null || self.IsDisposed)
				return;

			long instanceId = self.InstanceId;
			TimerComponent timerComponent = self.Root().GetComponent<TimerComponent>();
			
			// GT: 为什么要等一秒才释放？直接释放不行么？直接释放的话，当前的response无法返回给客户端，ugly!!
			await timerComponent.WaitAsync(1000);
			if (self.InstanceId != instanceId) {
				// 说明session不是之前的session了，不进行释放
				return;
			}
			
			self.Dispose();
		}
	}
}
