using Microsoft.Extensions.DependencyInjection.Extensions;
#if ANDROID
using Plugin.Maui.ML.TFLite.Platforms.Android;
#endif

namespace Plugin.Maui.ML.TFLite;

/// <summary>
///     Dependency injection helpers for the TFLite/MLKit inference backend.
/// </summary>
/// <remarks>
///     <b>Android 16 KB page-size note</b>: Adding this package to an Android app project
///     will include <c>libtensorflowlite_jni.so</c> (from <c>Xamarin.TensorFlow.Lite</c>),
///     which has 0x1000 ELF alignment and does not yet pass Android 16 KB page-size validation
///     on API 35+ devices. Only use this package when your app requires TFLite model execution.
///     For the default Android path (ONNX Runtime + NNAPI) use <c>Plugin.Maui.ML</c> directly.
/// </remarks>
public static class MLKitExtensions
{
    /// <summary>
    ///     Registers <see cref="IMLInfer" /> as a singleton backed by the TFLite
    ///     <c>MLKitInfer</c> implementation on Android.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="PlatformNotSupportedException">
    ///     Thrown on non-Android platforms because TFLite is Android-only.
    /// </exception>
    public static IServiceCollection AddMauiMLKit(this IServiceCollection services)
    {
#if ANDROID
        services.TryAddSingleton<IMLInfer>(_ => new MLKitInfer());
        return services;
#else
        throw new PlatformNotSupportedException(
            "The TFLite/MLKit backend (Plugin.Maui.ML.TFLite) is only available on Android.");
#endif
    }

    /// <summary>
    ///     Registers <see cref="IMLInfer" /> as a transient service backed by the TFLite
    ///     <c>MLKitInfer</c> implementation on Android.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="PlatformNotSupportedException">
    ///     Thrown on non-Android platforms because TFLite is Android-only.
    /// </exception>
    public static IServiceCollection AddMauiMLKitTransient(this IServiceCollection services)
    {
#if ANDROID
        services.TryAddTransient<IMLInfer>(_ => new MLKitInfer());
        return services;
#else
        throw new PlatformNotSupportedException(
            "The TFLite/MLKit backend (Plugin.Maui.ML.TFLite) is only available on Android.");
#endif
    }
}