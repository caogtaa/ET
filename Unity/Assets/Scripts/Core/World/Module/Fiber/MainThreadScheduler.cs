using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    internal class MainThreadScheduler: IScheduler
    {
        private readonly ConcurrentQueue<int> idQueue = new();
        private readonly ConcurrentQueue<int> addIds = new();       // fiberIds
        private readonly FiberManager fiberManager;                 // TODO: 这里循环引用了
        private readonly ThreadSynchronizationContext threadSynchronizationContext = new();

        public MainThreadScheduler(FiberManager fiberManager)
        {
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
            this.fiberManager = fiberManager;
        }

        public void Dispose()
        {
            this.addIds.Clear();
            this.idQueue.Clear();
        }

        /// <summary>
        /// 入口脚本Init::LateUpdate() -> FiberManager::LateUpdate() 驱动
        /// TODO: GT: 如果是在Unity主线程里跑，有必要每次Update/LateUpdate()都调用SynchronizationContext.SetSynchronizationContext()吗？
        /// </summary>
        public void Update()
        {
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
            this.threadSynchronizationContext.Update();
            
            // 每一帧都tick一下所有该scheduler下的fibers，顺便把失效的fiber移除
            int count = this.idQueue.Count;
            while (count-- > 0)
            {
                if (!this.idQueue.TryDequeue(out int id))
                {
                    continue;
                }

                Fiber fiber = this.fiberManager.Get(id);
                if (fiber == null)
                {
                    continue;
                }
                
                if (fiber.IsDisposed)
                {
                    continue;
                }
                
                // GT: Fiber.Instance是ThreadStatic变量，每个线程有自己的副本，所以在线程loop内修改是安全的
                // 目前看到的唯一用途是根据Instance是否为空选择不同类型的Logger，避免日志写入race乱掉
                Fiber.Instance = fiber;
                SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
                fiber.Update();
                Fiber.Instance = null;
                
                this.idQueue.Enqueue(id);
            }
            
            // Fiber调度完成，要还原成默认的上下文，否则unity的回调会找不到正确的上下文
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
        }

        /// <summary>
        /// 入口脚本Init::LateUpdate() -> FiberManager::LateUpdate() 驱动
        /// </summary>
        public void LateUpdate()
        {
            int count = this.idQueue.Count;
            while (count-- > 0)
            {
                if (!this.idQueue.TryDequeue(out int id))
                {
                    continue;
                }

                Fiber fiber = this.fiberManager.Get(id);
                if (fiber == null)
                {
                    continue;
                }

                if (fiber.IsDisposed)
                {
                    continue;
                }

                Fiber.Instance = fiber;
                SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
                fiber.LateUpdate();
                Fiber.Instance = null;
                
                this.idQueue.Enqueue(id);
            }

            while (this.addIds.Count > 0)
            {
                this.addIds.TryDequeue(out int result);
                this.idQueue.Enqueue(result);
            }
            
            // Fiber调度完成，要还原成默认的上下文，否则unity的回调会找不到正确的上下文
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
        }
        
        /// <summary>
        /// 添加一个fiberId到当前的线程调度器内
        /// 下一次LateUpdate()才会加入真正的驱动队列
        ///     之后的下一次Update()才会真正消费该fiber的消息
        /// </summary>
        /// <param name="fiberId"></param>
        public void Add(int fiberId = 0)
        {
            this.addIds.Enqueue(fiberId);
        }
    }
}