# Changelog

All notable changes to Plugin.Maui.ML are documented here.

---

## [Unreleased]

### Breaking Changes (Android)

#### Removed: `Microsoft.ML.OnnxRuntime.Extensions` from core Package

The `Microsoft.ML.OnnxRuntime.Extensions` (v0.14.0) package has been removed from the core
`Plugin.Maui.ML` NuGet package.

**Why:** This package was never used in code and only served to pull in non-compliant native
libraries into the Android build. It contributed two 64-bit .so files with 0x1000 ELF alignment:

- `libonnxruntime_extensions4j_jni.so`
- `libortextensions.so`

These libraries fail Android 16 KB page-size validation required for apps targeting API 35+
(Android 15+).

**Migration:** No action required. If your app uses ORT Extensions custom ops directly, add
`Microsoft.ML.OnnxRuntime.Extensions` as a direct dependency of your app project.

---

#### Removed: TensorFlow Lite / MLKit backend from core package

The `Xamarin.TensorFlow.Lite` (v2.16.1.7) package and related MLKit inference support has
been removed from `Plugin.Maui.ML` for Android 16 KB page-size compliance.

**Why:** TensorFlow Lite ships with `libtensorflowlite_jni.so` built with 0x1000 ELF alignment,
which fails Android 16+ validation. The default Android inference path (`OnnxRuntimeInfer` with
NNAPI acceleration) is fully 16 KB compliant and is the recommended path for Android 16+.

**Migration:**

- **Default ONNX Runtime + NNAPI (recommended):** No changes needed.
  ```csharp
  // Works out of the box — fully 16 KB compliant
  builder.Services.AddMauiML();
  ```

- **If you explicitly need TensorFlow Lite/MLKit:** Two options:

  1. **Use Xamarin.TensorFlow.Lite directly** (retains non-compliant natives):
     ```csharp
     // Add to your .csproj
     <PackageReference Include="Xamarin.TensorFlow.Lite" Version="2.16.1.7" />
     
     // In code, instantiate MLKitInfer will throw NotSupportedException with guidance
     ```

  2. **Migrate to LiteRT 1.4.0+ (recommended):** Google's official TensorFlow Lite successor
     with native 16 KB page-size compliance. API is compatible with TFLite for easy migration.
     See: https://ai.google.dev/edge/litert/migration

---

### Android Packaged Natives — Before and After

#### Before (Plugin.Maui.ML with ORT Extensions + TFLite)

```
lib/arm64-v8a/
  libonnxruntime.so                    ✅ 16 KB aligned
  libonnxruntime_extensions4j_jni.so   ❌ 0x1000 aligned — XA0141
  libortextensions.so                  ❌ 0x1000 aligned — XA0141
  libtensorflowlite_jni.so             ❌ 0x1000 aligned — XA0141
```

#### After (Plugin.Maui.ML with ONNX + NNAPI — default, recommended)

```
lib/arm64-v8a/
  libonnxruntime.so                    ✅ 16 KB aligned
  (All non-compliant natives are absent)
```

#### After (with explicit Xamarin.TensorFlow.Lite dependency — opt-in, not recommended)

```
lib/arm64-v8a/
  libonnxruntime.so                    ✅ 16 KB aligned
  libtensorflowlite_jni.so             ❌ 0x1000 aligned (upstream issue)
  (ORT Extensions natives are absent)
```

---

### Validation Summary

- ✅ **Default ONNX + NNAPI path:** Fully 16 KB page-size compliant for Android 16 (API 35+)
- ✅ **Builds:** All CI tests and builds succeed
- ✅ **Existing API:** No breaking changes to `AddMauiML()`, `MLPlugin.Default`, or `OnnxRuntimeInfer` behavior
- ⚠️ **MLKit backend:** Throws `NotSupportedException` with clear migration guidance when used without TensorFlow Lite package
