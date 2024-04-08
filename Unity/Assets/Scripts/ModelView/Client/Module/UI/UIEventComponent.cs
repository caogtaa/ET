using System;
using System.Collections.Generic;

namespace ET.Client
{
	/// <summary>
	/// 管理所有UI GameObject
	/// </summary>
	[Code]
	public class UIEventComponent: Singleton<UIEventComponent>, ISingletonAwake
	{
		public Dictionary<string, AUIEvent> UIEvents { get; } = new();
		
		/// <summary>
		/// 实例化所有[UIEvent]标注的类型，保存到UIEvents
		/// attr.UIType是string类型，自定义的字符串。在Demo里这个字符串直接耦合了本地的prefab名
		/// </summary>
        public void Awake()
        {
            var uiEvents = CodeTypes.Instance.GetTypes(typeof (UIEventAttribute));
            foreach (Type type in uiEvents)
            {
                object[] attrs = type.GetCustomAttributes(typeof (UIEventAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                UIEventAttribute uiEventAttribute = attrs[0] as UIEventAttribute;
                AUIEvent aUIEvent = Activator.CreateInstance(type) as AUIEvent;
                this.UIEvents.Add(uiEventAttribute.UIType, aUIEvent);
            }
        }
	}
}