# Coin counting implementation plan

Implement the agreed threshold -> connected components -> size and hole features -> denomination counts flow, using only supplied DIP code and .NET Framework 4.8.

1. Add a UI-independent `CoinAnalyzer.cs`: threshold via `HNUDIP.ImageProcess.Threshold`, label separate dark regions, reject noise, measure bounding dimensions and enclosed central holes, classify using sample-calibrated size ranges, and generate labeled output. Own and dispose output bitmaps explicitly.
2. Build a basic WinForms interface in `Form1`: original/result panels, sample/open/process controls, threshold input, denomination counts/subtotals, total and unknown count. Process an owned bitmap on a background task; disable conflicting controls while busy; release images on replacement and close.
3. Include both supplied source files, enable unsafe compilation for CITLIB, and copy the reference JPEG to output. Preserve supplied code unchanged.
4. Build Debug/Release. Run the analyzer on the reference image and compare all detected measurements and counts with the manual inventory. Exercise blank images, synthetic holed/solid shapes, invalid inputs, repeated processing, and UI loading/processing. Document measured results and scale limitations.

Scope: separated coins on a light background at the reference image scale (or a proportional resize of the whole reference). This is not a general classifier for differently framed photographs. Inventory totals are validation data, never algorithm inputs. No external packages or template matching.

