namespace ET.Server {
    public enum AccountType {
        General = 0,
        BlackList = 1,
    }
    
    // [ChildOf(typeof(AccountInfosComponent))]
    [ChildOf(typeof(Session))]
    public class AccountInfo : Entity, IAwake {
        public string Account;
        public string Password;
        public long CreateTime;
        public int AccountType;
    }
}
