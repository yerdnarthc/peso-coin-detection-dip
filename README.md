# Peso Coin Detection and Denomination with Digital Image Processing (DIP)

A C# Windows Forms application targeting .NET Framework 4.8. It counts Philippine 5-centavo, 10-centavo, 25-centavo, 1-peso, and 5-peso coins using the instructor-provided DIP source and built-in .NET facilities. No external packages are required.

## Run

Open `CoinDenominationDIP.slnx` in a compatible Visual Studio version, or open `CoinDenominationDIP/CoinDenominationDIP.csproj` directly. Install the .NET Framework 4.8 targeting pack and .NET desktop development tools. Build and run the application.

1. Click **Load Sample**, or **Open Image** for a JPEG, PNG, or BMP.
2. Leave **Threshold** at **150** for the supplied image.
3. Click **Count Coins**.
4. Inspect the denomination labels and counts. Toggle **Show binary mask** to inspect segmentation.



The result table shows counts and subtotals. Unknown candidates are marked with orange boxes and excluded from the monetary total. Changing the threshold clears previous results; click **Count Coins** again to recompute.

## Algorithm

1. Use `HNUDIP.ImageProcess.Threshold` to create a black-foreground, white-background mask from average RGB intensity.
2. Label 8-connected dark regions and discard small specks.
3. Measure each region's bounding box, aspect ratio, and foreground coverage. Search enclosed background regions for a central hole using 4-connectivity.
4. Classify holed candidates of the expected size as 5 centavos. Classify other valid candidates by mean bounding-box dimension using the manual sample calibration.
5. Draw denomination labels and calculate totals in integer centavos.

The implementation is in `CoinDenominationDIP/CoinAnalyzer.cs`; form interaction is in `Form1.cs`. Both provided DIP files are compiled, but this pipeline only needs the threshold method. Their original source remains unchanged.

### Calibration and limitations

Size is normalized to the supplied image's width of 776 pixels. The classifier uses these ranges in equivalent reference pixels:

| Features | Mean bounding-box dimension | Result |
|---|---|---|
| Central hole | 50 to less than 64 px | 5 centavos |
| No central hole | 60 to less than 70 px | 10 centavos |
| No central hole | 70 to less than 83.25 px | 25 centavos |
| No central hole | 83.25 to less than 96.5 px | 1 peso |
| No central hole | 96.5 through 112 px | 5 pesos |

These ranges are initial sample-specific calibration, not universal physical coin sizes. The thresholded boundaries differ slightly from manual selections because shadows and background pixels are excluded. Thresholding must preserve the coin bodies and central holes. Shape checks reject substantially elongated, sparse, or border-clipped regions as unknown.

The application supports separated coins on a light background at the supplied framing and scale, including proportional resizes of the complete image. Cropping, different camera distances, overlapping coins, or different backgrounds can produce incorrect classifications and require a different calibration or segmentation approach. It does not read coin inscriptions. Images are limited to 12 million pixels to bound processing memory use.

## Verified reference result

The analyzer matches the manual inventory in [docs/coin-inventory.md](docs/coin-inventory.md):

| Denomination | Count | Subtotal |
|---|---:|---:|
| 5 centavos | 7 | ₱0.35 |
| 10 centavos | 11 | ₱1.10 |
| 25 centavos | 28 | ₱7.00 |
| 1 peso | 13 | ₱13.00 |
| 5 pesos | 5 | ₱25.00 |
| **Total** | **64** | **₱46.45** |

These totals are checked by the verification harness, never used as inputs to detection or classification.

## Build and verification

From a Visual Studio Developer PowerShell at the repository root:

```powershell
MSBuild .\CoinDenominationDIP\CoinDenominationDIP.csproj /p:Configuration=Debug /p:Platform=AnyCPU
MSBuild .\CoinDenominationDIP\CoinDenominationDIP.csproj /p:Configuration=Release /p:Platform=AnyCPU
.\tests\Run-Checks.ps1
```

The dependency-free harness checks reference counts and value, repeated processing, half-size and double-size reference images, synthetic holed and solid coins, noise and malformed regions, blank input, invalid thresholds, source preservation, and basic WinForms flows. It creates a temporary off-screen test form and writes diagnostic images (`analyzed.png`, `mask.png`, `ui.png`) under the ignored Debug output directory. Both Debug and Release builds and this harness passed during implementation.

## Third-Party / Provided Code

The following Digital Image Processing source files were provided for
academic use by the course instructor:

- `CoinDenominationDIP/Libraries/CITLIBFilters.cs`
- `CoinDenominationDIP/Libraries/basicDIPlib_v1.cs`

These files are not original work of the project authors.
Original copyright and attribution notices have been preserved.
