using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using static System.Math;

namespace BinaryFog.NameParser {
	internal sealed class DedicatedTaskScheduler : TaskScheduler, IDisposable {
		[ThreadStatic] private static bool _threadIsWorking;

		private readonly CancellationTokenSource _cts
			= new CancellationTokenSource();

		private readonly ConcurrentQueue<Task> _tasks
			= new ConcurrentQueue<Task>();

		private volatile int _dequeueCount;
		private readonly LinkedList<Task> _dequeues
			= new LinkedList<Task>();

		private readonly Thread[] _threads;

		private readonly ManualResetEventSlim _workReadyEvent
			= new ManualResetEventSlim(false);

		public DedicatedTaskScheduler(int dop) {
			dop = Min(Max(1, dop),Environment.ProcessorCount);
			_threads = new Thread[dop];
			for (var i = 0 ; i < dop ; ++i)
				(_threads[i] = new Thread(WorkerAction) {IsBackground = true}).Start();
			MaximumConcurrencyLevel = dop;

			AssemblyLoadContext.Default.Unloading += ctx => Dispose();
		}

		public DedicatedTaskScheduler()
			: this(Environment.ProcessorCount-1) {
		}

		protected override void QueueTask(Task task) {
			_tasks.Enqueue(task);

			_workReadyEvent.Set();
		}

		private bool IsDequeued(Task task) {
			if (_dequeueCount == 0)
				return false;
			lock (_dequeues) {
				var success = _dequeues.Remove(task);
				if (success) --_dequeueCount;
				return success;
			}
		}

		private void WorkerAction() {
			//++_threadsWorking;
			try {
				var currentThread = Thread.CurrentThread;
				for (; ;) {
					_workReadyEvent.Wait(_cts.Token);
					_workReadyEvent.Reset();
					currentThread.IsBackground = false;
					_threadIsWorking = true;
					while (_tasks.TryDequeue(out var task))
						if (!IsDequeued(task))
							try {
								if (!TryExecuteTask(task))
									_tasks.Enqueue(task);
							}
							catch {
								/* throw it away */
							}
					_threadIsWorking = false;
					currentThread.IsBackground = true;
				}
			}
			catch {
				/* just exit */
			}
			finally {
				_threadIsWorking = false;
			}
		}


		protected override bool TryExecuteTaskInline(Task task, bool prevQueued) {
			if (!_threadIsWorking)
				return false;

			if (!prevQueued)
				return TryExecuteTask(task);

			if (!TryDequeue(task))
				return false;

			if (TryExecuteTask(task))
				return true;

			QueueTask(task);

			return false;
		}

		// Attempt to remove a previously scheduled task from the scheduler. 
		protected override bool TryDequeue(Task task) {
			if (!_tasks.Contains(task))
				return false;


			lock (_dequeues) {
				++_dequeueCount;
				_dequeues.AddLast(task);
			}

			return true;
		}

		public override int MaximumConcurrencyLevel { get; }

		// Gets an enumerable of the tasks currently scheduled on this scheduler. 
		protected override IEnumerable<Task> GetScheduledTasks() {
			lock (_dequeues)
				return _tasks
					.Except(_dequeues)
					.ToImmutableArray();
		}

		public void Dispose() {
			_cts.Cancel();
			_workReadyEvent.Dispose();

			while (_tasks.TryDequeue(out var _)) {
				/* clear */
			}

			for (var i = 0 ; i < MaximumConcurrencyLevel ; ++i)
				_threads[i] = null;
		}
	}
}