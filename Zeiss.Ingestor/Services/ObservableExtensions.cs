namespace System.Reactive.Linq
{
	using System.Diagnostics.CodeAnalysis;
	using System.Reactive.Concurrency;
	using System.Threading;
	using System.Threading.Tasks;

	[SuppressMessage("Style", "VSTHRD200:Use Async suffix for async methods", Justification = "In RX naming conventions we use Async also to denote methods that accept a Task-based (asynchronous) selector delegate.")]
	public static class ObservableExtensions
	{
		// INFO: The SelectParallelAsync and SelectManyParallelAsync methods in this class codify an 
		// implementation of fan-out parallelism in RX pipelines. The implementation is somewhat opinionated 
		// and designed for scenarios where the following are true:
		// - Selector is an async function (returns a Task)
		// - Selector may perform significant work before, as well as after, its first await
		// - Both parts of selector should run asynchronously (on the provided scheduler)
		// - Unbounded parallelism is desired (i.e. work is not 100% CPU-bound, which is most likely the case
		//   if selector is an async function
		// This pattern works around a known issue in RX whereby, if we just use StartAsync, any part of the
		// selector before its first await will be run synchronously.
		// https://github.com/dotnet/reactive/issues/457

		public static IObservable<TResult> SelectParallelAsync<TSource, TResult>(
			this IObservable<TSource> source,
			Func<TSource, CancellationToken, Task<TResult>> selector,
			IScheduler scheduler)
		{
			return source
				.Select(sourceItem =>
					Observable.Defer(() =>
						Observable.StartAsync(cancellationToken => selector(sourceItem, cancellationToken), scheduler))
					.SubscribeOn(scheduler)
				)
				.Merge();
		}

		public static IObservable<TResult> SelectParallelAsync<TSource, TResult>(
			this IObservable<TSource> source,
			Func<TSource, Task<TResult>> selector,
			IScheduler scheduler)
		{
			return source
				.Select(sourceItem =>
					Observable.Defer(() =>
						Observable.StartAsync(() => selector(sourceItem), scheduler))
					.SubscribeOn(scheduler)
				)
				.Merge();
		}

		public static IObservable<TResult> SelectManyParallelAsync<TSource, TResult>(
			this IObservable<TSource> source,
			Func<TSource, CancellationToken, Task<IObservable<TResult>>> selector,
			IScheduler scheduler)
		{
			return source
				.Select(sourceItem =>
					Observable.Defer(() =>
						Observable.StartAsync(cancellationToken => selector(sourceItem, cancellationToken), scheduler)
						.Merge())
					.SubscribeOn(scheduler)
				)
				.Merge();
		}

		public static IObservable<TResult> SelectManyParallelAsync<TSource, TResult>(
			this IObservable<TSource> source,
			Func<TSource, Task<IObservable<TResult>>> selector,
			IScheduler scheduler)
		{
			return source
				.Select(sourceItem =>
					Observable.Defer(() =>
						Observable.StartAsync(() => selector(sourceItem), scheduler)
						.Merge())
					.SubscribeOn(scheduler)
				)
				.Merge();
		}

		public static IObservable<Unit> SelectParallelAsync<TSource>(
			this IObservable<TSource> source,
			Func<TSource, CancellationToken, Task> selector,
			IScheduler scheduler)
		{
			return source
				.Select(sourceItem =>
					Observable.Defer(() =>
						Observable.StartAsync(cancellationToken => selector(sourceItem, cancellationToken), scheduler))
					.SubscribeOn(scheduler)
				)
				.Merge();
		}

		public static IObservable<Unit> SelectParallelAsync<TSource>(
			this IObservable<TSource> source,
			Func<TSource, Task> selector,
			IScheduler scheduler)
		{
			return source
				.Select(sourceItem =>
					Observable.Defer(() =>
						Observable.StartAsync(() => selector(sourceItem), scheduler))
					.SubscribeOn(scheduler)
				)
				.Merge();
		}

		public static IObservable<TResult> SelectParallel<TSource, TResult>(
			this IObservable<TSource> source,
			Func<TSource, TResult> selector,
			IScheduler scheduler)
		{
			return source
				.Select(sourceItem => Observable.Start(() => selector(sourceItem), scheduler))
				.Merge();
		}

		public static IObservable<Unit> SelectParallel<TSource>(
			this IObservable<TSource> source,
			Action<TSource> selector,
			IScheduler scheduler)
		{
			return source
				.Select(sourceItem => Observable.Start(() => selector(sourceItem), scheduler))
				.Merge();
		}
	}
}