#if ANDROID
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Plugin.Maui.ML.Platforms.Android;

/// <summary>
///     Stub implementation of the TFLite/MLKit inference backend.
///     The real implementation lives in the <c>Plugin.Maui.ML.TFLite</c> NuGet package.
/// </summary>
/// <remarks>
///     This stub exists so that code which references <see cref="MLKitInfer"/> (or
///     <see cref="MLBackend.MLKit"/>) compiles against the core package and produces
///     a clear, actionable error at runtime instead of a missing-type build error.
///
///     To use TFLite inference on Android:
///     <list type="number">
///       <item>Add the <c>Plugin.Maui.ML.TFLite</c> NuGet package to your app project.</item>
///       <item>Replace <c>AddMauiML(MLBackend.MLKit)</c> with <c>AddMauiMLKit()</c> in your
///         <c>MauiProgram.cs</c>.</item>
///     </list>
/// </remarks>
public sealed class MLKitInfer : IMLInfer, IDisposable
{
    private const string Guidance =
        "The MLKit/TFLite backend is provided by the 'Plugin.Maui.ML.TFLite' NuGet package. " +
        "Add that package to your app project and use 'AddMauiMLKit()' instead of " +
        "'AddMauiML(MLBackend.MLKit)'.";

    /// <inheritdoc/>
    public MLBackend Backend => MLBackend.MLKit;

    /// <inheritdoc/>
    public bool IsModelLoaded => false;

    /// <inheritdoc/>
    public void Dispose() { /* nothing to dispose */ }

    /// <inheritdoc/>
    public Task LoadModelAsync(string modelPath, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(Guidance);

    /// <inheritdoc/>
    public Task LoadModelAsync(Stream modelStream, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(Guidance);

    /// <inheritdoc/>
    public Task LoadModelFromAssetAsync(string assetName, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(Guidance);

    /// <inheritdoc/>
    public Task<Dictionary<string, Tensor<float>>> RunInferenceAsync(
        Dictionary<string, Tensor<float>> inputs,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(Guidance);

    /// <inheritdoc/>
    public Task<Dictionary<string, Tensor<float>>> RunInferenceLongInputsAsync(
        Dictionary<string, Tensor<long>> inputs,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(Guidance);

    /// <inheritdoc/>
    public Dictionary<string, MLNodeMetadata> GetInputMetadata() =>
        throw new NotSupportedException(Guidance);

    /// <inheritdoc/>
    public Dictionary<string, MLNodeMetadata> GetOutputMetadata() =>
        throw new NotSupportedException(Guidance);

    /// <inheritdoc/>
    public void UnloadModel() { /* nothing to unload */ }
}
#endif