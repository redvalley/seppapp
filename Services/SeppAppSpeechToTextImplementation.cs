using CommunityToolkit.Maui.Media;

namespace SeppApp.Services;

public abstract class SeppAppSpeechToTextImplementation : ISpeechToText
{
	readonly WeakEventManager recognitionResultUpdatedWeakEventManager = new();
	readonly WeakEventManager recognitionResultCompletedWeakEventManager = new();
	readonly WeakEventManager speechToTextStateChangedWeakEventManager = new();

	/// <inheritdoc />
	public event EventHandler<SpeechToTextRecognitionResultUpdatedEventArgs> RecognitionResultUpdated
	{
		add => recognitionResultUpdatedWeakEventManager.AddEventHandler(value);
		remove => recognitionResultUpdatedWeakEventManager.RemoveEventHandler(value);
	}

	/// <inheritdoc />
	public event EventHandler<SpeechToTextRecognitionResultCompletedEventArgs> RecognitionResultCompleted
	{
		add => recognitionResultCompletedWeakEventManager.AddEventHandler(value);
		remove => recognitionResultCompletedWeakEventManager.RemoveEventHandler(value);
	}

	/// <inheritdoc />
	public event EventHandler<SpeechToTextStateChangedEventArgs> StateChanged
	{
		add => speechToTextStateChangedWeakEventManager.AddEventHandler(value);
		remove => speechToTextStateChangedWeakEventManager.RemoveEventHandler(value);
	}

	/// <inheritdoc/>
	public async Task StartListenAsync(SpeechToTextOptions options, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var isPermissionGranted = await IsSpeechPermissionAuthorized(cancellationToken).ConfigureAwait(false);
		if (!isPermissionGranted)
		{
			throw new PermissionException($"{nameof(Permissions)}.{nameof(Permissions.Microphone)} Not Granted");
		}

		await InternalStartListeningAsync(options, cancellationToken).ConfigureAwait(false);
	}

	protected abstract Task InternalStartListeningAsync(SpeechToTextOptions options, CancellationToken cancellationToken);

	protected abstract Task<bool> IsSpeechPermissionAuthorized(CancellationToken cancellationToken);

	protected abstract Task InternalStopListeningAsync(CancellationToken cancellationToken);

	/// <inheritdoc/>
	public Task StopListenAsync(CancellationToken cancellationToken = default) => InternalStopListeningAsync(cancellationToken);

	public abstract Task<bool> RequestPermissions(CancellationToken cancellationToken = new CancellationToken());

	public abstract SpeechToTextState CurrentState { get; }


	protected void OnRecognitionResultUpdated(string recognitionResult)
	{
		recognitionResultUpdatedWeakEventManager.HandleEvent(this, new SpeechToTextRecognitionResultUpdatedEventArgs(recognitionResult), nameof(RecognitionResultUpdated));
	}

	protected void OnRecognitionResultCompleted(SpeechToTextResult recognitionResult)
	{

		recognitionResultCompletedWeakEventManager.HandleEvent(this, new SpeechToTextRecognitionResultCompletedEventArgs(recognitionResult), nameof(RecognitionResultCompleted));
	}

	protected void OnSpeechToTextStateChanged(SpeechToTextState speechToTextState)
	{
		speechToTextStateChangedWeakEventManager.HandleEvent(this, new SpeechToTextStateChangedEventArgs(speechToTextState), nameof(StateChanged));
	}
	
	

#if !MACCATALYST && !IOS
	/// <inheritdoc/>
	public async Task<bool> RequestPermissions(CancellationToken cancellationToken = default)
	{
		var status = await Permissions.RequestAsync<Permissions.Microphone>().WaitAsync(cancellationToken).ConfigureAwait(false);
		return status is PermissionStatus.Granted;
	}

	static async Task<bool> IsSpeechPermissionAuthorized(CancellationToken cancellationToken)
	{
		var status = await Permissions.CheckStatusAsync<Permissions.Microphone>().WaitAsync(cancellationToken).ConfigureAwait(false);
		return status is PermissionStatus.Granted;
	}
#endif
	public abstract ValueTask DisposeAsync();
}