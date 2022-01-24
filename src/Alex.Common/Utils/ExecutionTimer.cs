using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Alex.Common.Utils;

public record TimingResult(string Name, TimeSpan ElapsedTime);

public class TimingsReport : IEnumerable<TimingResult>
{
	private TimingResult[] _results;
	public TimingsReport(IEnumerable<TimingResult> timingResults)
	{
		_results = timingResults.ToArray();
	}

	/// <inheritdoc />
	public IEnumerator<TimingResult> GetEnumerator()
	{
		foreach (var result in _results)
			yield return result;
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

public class ExecutionTimer : ITimer
{
	public string Name { get; }
	public TimeSpan Elapsed => _stopwatch.Elapsed;
	
	private readonly ExecutionTimer _parent;

	private Stopwatch _stopwatch;
	
	#if DEBUG
	private List<TimingResult> _timingResults = new List<TimingResult>();
	#endif
	public ExecutionTimer(string name)
	{
		Name = name;
		_stopwatch = Stopwatch.StartNew();
	}

	protected ExecutionTimer(string name, ExecutionTimer parent) : this(name)
	{
		_parent = parent;
	}

	/// <inheritdoc />
	public ITimer Section(string name)
	{
#if DEBUG
		return new ExecutionTimer(name, this);
#else
		return this;
#endif
	}

	/// <inheritdoc />
	public TimingsReport GenerateReport()
	{
#if DEBUG
		return new TimingsReport(_timingResults);
#else
		return new TimingsReport(Array.Empty<TimingResult>());
#endif
	}

	private bool _stopped = false;
	/// <inheritdoc />
	public void Stop()
	{
		if (_stopped)
			return;

		_stopped = true;
#if DEBUG
		_stopwatch.Stop();
		_timingResults.Add(new TimingResult(Name, _stopwatch.Elapsed));
		_parent?.ReportTimings(this);
#endif
	}
#if DEBUG
	protected void ReportTimings(ExecutionTimer timer)
	{
		foreach (var result in timer._timingResults)
		{
			string actualName = $"{Name}.{result.Name}";
			_timingResults.Add(new TimingResult(actualName, result.ElapsedTime));
		}
	}
#endif
	
	/// <inheritdoc />
	public void Dispose()
	{
		Stop();
#if DEBUG
		_timingResults.Clear();
#endif
	}
}

public interface ITimer : IDisposable
{
	ITimer Section(string name);

	TimingsReport GenerateReport();

	void Stop();
}