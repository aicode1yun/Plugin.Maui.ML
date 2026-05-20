# Changelog

All notable changes to Plugin.Maui.ML are documented here.

---

## [Unreleased]

### Breaking Changes (Android)

#### `Microsoft.ML.OnnxRuntime.Extensions` removed from `Plugin.Maui.ML`

The `Microsoft.ML.OnnxRuntime.Extensions` (v0.14.0) package has been removed from the core
`Plugin.Maui.ML` NuGet package.

**Why:** This package was never referenced by any code in the library — it was accidentally
included and caused two 64-bit native libraries with 0x1000 ELF segment alignment to be
packaged into every Android build:

- `libonnxruntime_extensions4j_jni.so`
- `libortextensions.so`

These libraries fail the Android 16 KB page-size validation required for apps targeting
API 35+ (Android 15).

**Migration:** No action required. If your app uses ORT Extensions custom ops directly,
add `Microsoft.ML.OnnxRuntime.Extensions` as a direct dependency of your app project.

---

#### `Xamarin.TensorFlow.Lite` removed from `Plugin.Maui.ML`; moved to new `Plugin.Maui.ML.TFLite` package

The `Xamarin.TensorFlow.Lite` (v2.16.1.7) package has been removed from the Android-conditional
dependencies of `Plugin.Maui.ML`.

**Why:** This package ships `libtensorflowlite_jni.so` with 0x1000 ELF alignment, which fails
the Android 16 KB page-size validation. It was only needed by the `MLKitInfer` backend
(TFLite Interpreter), which is **not** the default Android path. The default Android path
(`PlatformMLInfer` → `OnnxRuntimeInfer` with NNAPI) does not require TFLite.

**Migration:**

- **Default Android path (ONNX Runtime + NNAPI)** — no change needed. `AddMauiML()` or
  `MLPlugin.Default` continue to work exactly as before.

- **TFLite/MLKit path** — if you previously used `MLBackend.MLKit` or `new MLKitInfer()` on
  Android, you must now:

  1. Add the `Plugin.Maui.ML.TFLite` NuGet package to your Android app project.
  2. Replace `AddMauiML(MLBackend.MLKit)` with `AddMauiMLKit()`:

     ```csharp
     // Before
     builder.Services.AddMauiML(MLBackend.MLKit); // ← throws NotSupportedException now

     // After — requires Plugin.Maui.ML.TFLite package
     builder.Services.AddMauiMLKit();
     ```

  3. Instantiate `MLKitInfer` from the new namespace if used directly:

     ```csharp
     // Before
     using Plugin.Maui.ML.Platforms.Android;
     var infer = new MLKitInfer();

     // After — requires Plugin.Maui.ML.TFLite package
     using Plugin.Maui.ML.TFLite.Platforms.Android;
     var infer = new MLKitInfer();
     ```

> ⚠ **16 KB note for TFLite users:** `Xamarin.TensorFlow.Lite` 2.16.1.7 still ships a
> non-16KB-aligned `libtensorflowlite_jni.so`. This is a known upstream issue in the TFLite
> project. By isolating TFLite in `Plugin.Maui.ML.TFLite`, only apps that explicitly need
> TFLite runtime carry the non-compliant `.so` — the default ONNX+NNAPI path is fully
> 16 KB compliant.

---

### Android Packaged Natives — Before / After

#### Before

```
lib/arm64-v8a/
  libonnxruntime.so                    ✅ 16 KB aligned
  libonnxruntime_extensions4j_jni.so   ❌ 0x1000 aligned — XA0141
  libortextensions.so                  ❌ 0x1000 aligned — XA0141
  libtensorflowlite_jni.so             ❌ 0x1000 aligned — XA0141
```

#### After — Plugin.Maui.ML (default ONNX+NNAPI path)

```
lib/arm64-v8a/
  libonnxruntime.so                    ✅ 16 KB aligned
  (ORT Extensions and TFLite natives are absent)
```

#### After — Plugin.Maui.ML + Plugin.Maui.ML.TFLite (explicit TFLite opt-in)

```
lib/arm64-v8a/
  libonnxruntime.so                    ✅ 16 KB aligned
  libtensorflowlite_jni.so             ❌ 0x1000 aligned — XA0141 (upstream issue, opt-in)
  (ORT Extensions natives are absent)
```

---

### New Packages

#### `Plugin.Maui.ML.TFLite` (new)

Companion package providing the TFLite/MLKit inference backend for Android.

**Features:**
- `Plugin.Maui.ML.TFLite.Platforms.Android.MLKitInfer` — full TFLite Interpreter implementation
- `AddMauiMLKit()` and `AddMauiMLKitTransient()` DI extension methods
- `Xamarin.TensorFlow.Lite` dependency scoped to Android only

**Usage:**

```xml
<!-- In your .csproj, Android only -->
<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
  <PackageReference Include="Plugin.Maui.ML.TFLite" Version="1.0.0" />
</ItemGroup>
```

```csharp
// MauiProgram.cs
#if ANDROID
builder.Services.AddMauiMLKit(); // Registers MLKitInfer (TFLite) as IMLInfer
#else
builder.Services.AddMauiML();    // ONNX Runtime on other platforms
#endif
```
