using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinaryFog.NameParser {
	public static class TaskManager {
		public static readonly TaskScheduler TaskScheduler
			= new DedicatedTaskScheduler();

		public static readonly TaskFactory TaskFactory
			= new TaskFactory(TaskScheduler);


		public static IEnumerable<TResult> Schedule<T, TResult>(IEnumerable<T> stuff, Func<T, TResult> work) {
			return stuff
				.Select(item => TaskFactory.StartNew(() => work(item)))
				.ToArray().Select(t => t.Result);
		}
	}
}