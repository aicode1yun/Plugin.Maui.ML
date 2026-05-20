#if ANDROID
using Java.Nio;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Plugin.Maui.ML;
using Xamarin.TensorFlow.Lite;
using Application = Android.App.Application;
using Object = Java.Lang.Object;

namespace Plugin.Maui.ML.TFLite.Platforms.Android;

/// <summary>
///     TensorFlow Lite inference backend for Android using <c>Xamarin.TensorFlow.Lite</c>.
/// </summary>
/// <remarks>
///     <para>
///         Loads <c>.tflite</c> models and runs inference via the TFLite <see cref="Interpreter"/>.
///         Register via DI using <c>AddMauiMLKit()</c> from the
///         <c>Plugin.Maui.ML.TFLite</c> package.
///     </para>
///     <para>
///         ⚠ <b>Android 16 KB page-size note</b>: <c>libtensorflowlite_jni.so</c> shipped by
///         <c>Xamarin.TensorFlow.Lite 2.16.1.7</c> has 0x1000 ELF alignment and will trigger an
///         XA0141 warning on Android API 35+ builds. This is a known upstream issue in the
///         TensorFlow Lite library. Until an aligned build is released, apps using this backend
///         cannot fully pass the 16 KB page-size validation.
///     </para>
/// </remarks>
public sealed class MLKitInfer : IMLInfer, IDisposable
{
    private readonly Lock _sync = new();
    private bool _disposed;
    private Interpreter? _interpreter;
    private byte[]? _modelBytes;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        UnloadModel();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public MLBackend Backend => MLBackend.MLKit;

    /// <inheritdoc/>
    public bool IsModelLoaded => _interpreter != null;

    /// <inheritdoc/>
    public async Task LoadModelAsync(string modelPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
            throw new ArgumentException("Model path cannot be null or empty", nameof(modelPath));
        if (!File.Exists(modelPath))
            throw new FileNotFoundException($"Model file not found: {modelPath}");
        var bytes = await File.ReadAllBytesAsync(modelPath, cancellationToken).ConfigureAwait(false);
        Initialize(bytes);
    }

    /// <inheritdoc/>
    public async Task LoadModelAsync(Stream modelStream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(modelStream);
        await using var ms = new MemoryStream();
        await modelStream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        Initialize(ms.ToArray());
    }

    /// <inheritdoc/>
    public async Task LoadModelFromAssetAsync(string assetName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(assetName))
            throw new ArgumentException("Asset name cannot be null or empty", nameof(assetName));
        try
        {
            await using var assetStream = Application.Context.Assets?.Open(assetName)
                                          ?? throw new FileNotFoundException(
                                              $"Asset '{assetName}' not found in Android assets.");
            await LoadModelAsync(assetStream, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load TFLite model asset '{assetName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public Task<Dictionary<string, Tensor<float>>> RunInferenceAsync(
        Dictionary<string, Tensor<float>> inputs,
        CancellationToken cancellationToken = default)
    {
        if (!IsModelLoaded) throw new InvalidOperationException("No TFLite model loaded. Call LoadModelAsync first.");
        if (inputs == null || inputs.Count == 0)
            throw new ArgumentException("Inputs cannot be null or empty", nameof(inputs));

        return Task.Run(() =>
        {
            lock (_sync)
            {
                if (_interpreter == null) throw new InvalidOperationException("Interpreter disposed.");

                var first = inputs.First();
                var inputTensor = first.Value;
                var inputIndex = 0;

                var flat = inputTensor.ToArray();
                var inputShape = inputTensor.Dimensions.ToArray();
                try
                {
                    _interpreter.ResizeInput(inputIndex, inputShape);
                }
                catch
                {
                    /* ignore if immutable */
                }

                _interpreter.AllocateTensors();

                var inputData = Object.FromArray(flat);

                var outputTensor = _interpreter.GetOutputTensor(0);
                if (outputTensor != null)
                {
                    var oshape = outputTensor.Shape();
                    var outCount = 1;
                    if (oshape != null)
                    {
                        foreach (var d in oshape) outCount *= d;
                        var outputArray = new float[outCount];

                        _interpreter.Run(inputData, outputArray);

                        var dense = new DenseTensor<float>(oshape.Select(i => i).ToArray());
                        var span = dense.Buffer.Span;
                        for (var i = 0; i < outputArray.Length; i++) span[i] = outputArray[i];

                        return new Dictionary<string, Tensor<float>> { ["output0"] = dense };
                    }
                }
            }

            throw new InvalidOperationException("Failed to run inference or retrieve output.");
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<Dictionary<string, Tensor<float>>> RunInferenceLongInputsAsync(
        Dictionary<string, Tensor<long>> inputs,
        CancellationToken cancellationToken = default)
    {
        if (inputs == null || inputs.Count == 0)
            throw new ArgumentException("Inputs cannot be null or empty", nameof(inputs));
        var floatInputs = new Dictionary<string, Tensor<float>>();
        foreach (var (k, v) in inputs)
        {
            var cast = new DenseTensor<float>(v.Dimensions.ToArray());
            var arr = v.ToArray();
            for (var i = 0; i < arr.Length; i++) cast.Buffer.Span[i] = arr[i];
            floatInputs[k] = cast;
        }

        return RunInferenceAsync(floatInputs, cancellationToken);
    }

    /// <inheritdoc/>
    public Dictionary<string, MLNodeMetadata> GetInputMetadata() =>
        throw new NotSupportedException("TFLite tensor metadata query is not yet exposed by this backend.");

    /// <inheritdoc/>
    public Dictionary<string, MLNodeMetadata> GetOutputMetadata() =>
        throw new NotSupportedException("TFLite tensor metadata query is not yet exposed by this backend.");

    /// <inheritdoc/>
    public void UnloadModel()
    {
        lock (_sync)
        {
            _interpreter?.Close();
            _interpreter?.Dispose();
            _interpreter = null;
            _modelBytes = null;
        }
    }

    private void Initialize(byte[] bytes)
    {
        lock (_sync)
        {
            UnloadModel();
            _modelBytes = bytes;
            var nativeOrder = ByteOrder.NativeOrder() ?? ByteOrder.BigEndian;
            var byteBuffer = ByteBuffer.AllocateDirect(bytes.Length).Order(nativeOrder!);
            byteBuffer.Put(bytes);
            byteBuffer.Rewind();
            _interpreter = new Interpreter(byteBuffer,
                (Interpreter.Options?)new Interpreter.Options().SetNumThreads(
                    Math.Max(1, Environment.ProcessorCount - 1)));
            _interpreter.AllocateTensors();
        }
    }
}
#endif
