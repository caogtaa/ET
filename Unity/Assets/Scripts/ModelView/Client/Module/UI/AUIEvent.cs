namespace ET.Client
{
    public abstract class AUIEvent: HandlerObject
    {
        // TODO: GT: 窗体管理设计得太简单了，没有GameFramework里得被覆盖、被激活等消息
        public abstract ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer);
        public abstract void OnRemove(UIComponent uiComponent);
    }
}