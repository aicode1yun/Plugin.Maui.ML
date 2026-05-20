#if ANDROID
using Microsoft.ML.OnnxRuntime.Tensors;
using Plugin.Maui.ML;

namespace Plugin.Maui.ML.Platforms.Android;

/// <summary>
///     MLKit inference backend (stub).
/// </summary>
/// <remarks>
///     <para>
///         The MLKit backend (TensorFlow Lite) is not included in the core Plugin.Maui.ML NuGet package
///         to ensure Android 16 KB page-size compliance.
///     </para>
///     <para>
///         The default Android inference path is <strong>ONNX Runtime with NNAPI acceleration</strong>,
///         which is fully 16 KB page-size compliant and recommended for Android 16 (API 35+) apps.
///     </para>
///     <para>
///         If you require TensorFlow Lite/MLKit inference, please:
///         1. Keep using the legacy Xamarin.TensorFlow.Lite package directly, OR
///         2. Migrate to LiteRT 1.4.0+ (Google's official TensorFlow Lite successor with 16 KB compliance)
///     </para>
///     <para>
///         For migration guidance, see: <see href="https://ai.google.dev/edge/litert/migration"/>
///     </para>
/// </remarks>
public sealed class MLKitInfer : IMLInfer
{
    /// <inheritdoc/>
    public MLBackend Backend => throw new NotSupportedException(GetErrorMessage());

    /// <inheritdoc/>
    public bool IsModelLoaded => throw new NotSupportedException(GetErrorMessage());

    /// <inheritdoc/>
    public void Dispose() => throw new NotSupportedException(GetErrorMessage());

    /// <inheritdoc/>
    public Task LoadModelAsync(string modelPath, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(GetErrorMessage());

    /// <inheritdoc/>
    public Task LoadModelAsync(Stream modelStream, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(GetErrorMessage());

    /// <inheritdoc/>
    public Task LoadModelFromAssetAsync(string assetName, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(GetErrorMessage());

    /// <inheritdoc/>
    public Task<Dictionary<string, Tensor<float>>> RunInferenceAsync(
        Dictionary<string, Tensor<float>> inputs,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(GetErrorMessage());

    /// <inheritdoc/>
    public Task<Dictionary<string, Tensor<float>>> RunInferenceLongInputsAsync(
        Dictionary<string, Tensor<long>> inputs,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(GetErrorMessage());

    /// <inheritdoc/>
    public Dictionary<string, MLNodeMetadata> GetInputMetadata() =>
        throw new NotSupportedException(GetErrorMessage());

    /// <inheritdoc/>
    public Dictionary<string, MLNodeMetadata> GetOutputMetadata() =>
        throw new NotSupportedException(GetErrorMessage());

    /// <inheritdoc/>
    public void UnloadModel() =>
        throw new NotSupportedException(GetErrorMessage());

    private static string GetErrorMessage() =>
        """
        MLKit (TensorFlow Lite) backend is not included in Plugin.Maui.ML to ensure Android 16 KB 
        page-size compliance (required for Android 16+).
        
        Recommended: Use the default ONNX Runtime backend with NNAPI acceleration via:
            AddMauiML() or MLPlugin.Default
        
        Alternative: If you specifically need TFLite:
          • Continue using Xamarin.TensorFlow.Lite directly, OR
          • Migrate to LiteRT 1.4.0+ (Google's official successor)
        
        See: https://ai.google.dev/edge/litert/migration
        """;
}
#endif
