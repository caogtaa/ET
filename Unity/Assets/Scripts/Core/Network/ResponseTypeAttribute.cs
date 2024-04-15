using System;

namespace ET
{
    // TODO: GT: ResponseType为什么要保存string name而不是Type？客户端代码混淆后咋办？
    public class ResponseTypeAttribute: BaseAttribute
    {
        public string Type { get; }

        public ResponseTypeAttribute(string type)
        {
            this.Type = type;
        }
    }
}