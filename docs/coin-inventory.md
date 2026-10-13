# Coin Inventory

Based on my observations in the reference image, here are the manual reference counts for each denomination.

| Denomination    |        Count |             Value |
| --------------- | -----------: | ----------------: |
| 5 centavos      |            7 |            ₱0.35 |
| 10 centavos     |           11 |            ₱1.10 |
| 25 centavos     |           28 |            ₱7.00 |
| ₱1             |           13 |           ₱13.00 |
| ₱5             |            5 |           ₱25.00 |
| **Total** | **64** | **₱46.45** |

## Coin Location Approximation

- **5 Centavo**:	one 5 Centavo coin near the top-right edge.
- **10 Centavo**:	one 10 Centavo coin in the very first row of coins, then after the very first appearance of 5 Centavo coin where it's beside its right side.
- **25 Centavo**:	one 25 Centavo coin near the top-left edge.
- **₱1**:			one ₱1 coin also in the very first row of coins, it's the only ₱1 coin in that row.
- **₱5**:			one ₱5 coin can be found near the bottom-right edge.

## Coin Size Measurements

Measurements are taken manually in Paint from the original image without resizing. Each rectangular selection tightly encloses the coin's outer rim and excludes its shadow. Mean diameter is calculated as (width + height) / 2.

| Denomination | Location                                 |  Width | Height | Mean diameter |
| ------------ | ---------------------------------------- | -----: | -----: | ------------: |
| 5 centavos   | Near top-right edge                      |  58 px |  60 px |         59 px |
| 10 centavos  | First row, right of the first holed coin |  64 px |  65 px |       64.5 px |
| 25 centavos  | Near top-left edge                       |  75 px |  76 px |      75.5 px  |
| ₱1          | First row, only ₱1 coin                |  90 px |  92 px |         91 px |
| ₱5          | Near bottom-right edge                   | 101 px | 103 px |        102 px |

These are initial sample measurements for all five denominations, not classification thresholds. Additional observations are recorded below.

## Additional Size Observations

### 10-centavo consistency checks

The two additional coins were checked in Photopea by moving the same 64 × 65 px selection box over each coin in the original image. Both were reported to fit. These are approximate fit checks, not independently measured tight bounding boxes; they do not establish exact size equality or zero variation.

| Sample | Approximate location | Selection width | Selection height | Mean selection dimension | Method |
|---|---|---:|---:|---:|---|
| 10C-01 | First row, right of the first holed coin | 64 px | 65 px | 64.5 px | Initial manual measurement in Paint |
| 10C-02 | Only 10-centavo coin at the leftmost edge | 64 px | 65 px | 64.5 px | Reused selection box fit check in Photopea |
| 10C-03 | First of the two 10-centavo coins described as being in the middle | 64 px | 65 px | 64.5 px | Reused selection box fit check in Photopea |

| Observation | Result |
|---|---|
| Samples examined | 3: one initial measurement and two additional fit checks |
| Reused selection size | 64 × 65 px |
| Mean of selection width and height | 64.5 px |
| Interpretation | The additional coins appear consistent with the initial sample size; exact size variation remains unmeasured. |
| Classification status | No size threshold has been established. |

### 5-centavo consistency checks

Two additional 5-centavo coins were reported to fit the initial sample selection. The table assumes the same 58 × 60 px selection box and Photopea fit-check method used for these checks; new independent width and height measurements were not supplied.

| Sample | Approximate location | Selection width | Selection height | Mean selection dimension | Method |
|---|---|---:|---:|---:|---|
| 5C-01 | Near top-right edge | 58 px | 60 px | 59 px | Initial manual measurement in Paint |
| 5C-02 | Near the first 10-centavo sample in the top row | 58 px | 60 px | 59 px | Selection box fit check; dimensions and method assumed from the initial sample and preceding checks |
| 5C-03 | Last 5-centavo coin when scanning toward the bottom of the image | 58 px | 60 px | 59 px | Selection box fit check; dimensions and method assumed from the initial sample and preceding checks |

| Observation | Result |
|---|---|
| Samples examined | 3: one initial measurement and two additional fit checks |
| Assumed reused selection size | 58 × 60 px |
| Mean of selection width and height | 59 px |
| Interpretation | The additional coins were reported to fit the initial sample selection; exact size variation remains unmeasured. |
| Location note | The final sample was described as being in the most-bottom row; its exact position is not recorded. |
| Classification status | No size threshold has been established. |

### 25-centavo consistency checks

Two additional coins identified by the user as 25 centavos were checked in Photopea using the same 75 × 76 px selection box. These are approximate fit checks, not independent tight bounding-box measurements. Mean selection dimension is (width + height) / 2.

| Sample | Approximate location | Selection width | Selection height | Mean selection dimension | Method and observation |
|---|---|---:|---:|---:|---|
| 25C-01 | Near top-left edge | 75 px | 76 px | 75.5 px | Initial manual measurement in Paint |
| 25C-02 | First 25-centavo coin in the bottom row | 75 px | 76 px | 75.5 px | Photopea fit check; reported to fit almost exactly, with no headroom at the top edge |
| 25C-03 | In the cluster near the top-left edge, immediately right of the suspected reverse-face coin | 75 px | 76 px | 75.5 px | Photopea fit check; selected as the identifiable replacement for the unconfirmed reverse-face candidate and reported to fit |

| Observation | Result |
|---|---|
| Reference samples examined | 3: one initial measurement and two additional fit checks |
| Reused selection size | 75 × 76 px |
| Mean of selection width and height | 75.5 px |
| Interpretation | The additional reference coins appear consistent with the initial sample size; exact size variation remains unmeasured. |
| Top-edge fit | No headroom is needed, provided the selection includes the entire outer rim without clipping it. |
| Classification status | No size threshold has been established. |

### Unconfirmed reverse-face candidate

| Candidate | Approximate location | Visible face | Selection fit | Denomination status |
|---|---|---|---|---|
| U-01 | In the cluster near the top-left edge, immediately left of sample 25C-03 | Reverse face; described as upside down, without the visible 25 figure | Reported to fit the 75 × 76 px box | Suspected 25 centavos; not independently confirmed |

Candidate U-01 is excluded from the confirmed 25-centavo calibration samples. Fitting the same selection box is size evidence, not independent denomination confirmation. Retain it for later investigation; it is not a labeled accuracy reference until its denomination is independently verified. This observation does not change the recorded manual inventory totals.

### 1-peso consistency checks

Two additional coins identified by the user as 1 peso were reported to fit the same 90 × 92 px selection box. These are approximate fit checks, not independent tight bounding-box measurements. Mean selection dimension is (width + height) / 2.

| Sample | Approximate location | Selection width | Selection height | Mean selection dimension | Method and observation |
|---|---|---:|---:|---:|---|
| 1P-01 | First row, only 1-peso coin | 90 px | 92 px | 91 px | Initial manual measurement in Paint |
| 1P-02 | Only 1-peso coin along the rightmost edge | 90 px | 92 px | 91 px | Reused selection box fit check; reported to fit |
| 1P-03 | Lower-middle area, surrounded mostly by 25-centavo coins, with a 10-centavo coin below-right | 90 px | 92 px | 91 px | Reused selection box fit check; reported to fit |

| Observation | Result |
|---|---|
| Samples examined | 3: one initial measurement and two additional fit checks |
| Reused selection size | 90 × 92 px |
| Mean of selection width and height | 91 px |
| Interpretation | The additional coins appear consistent with the initial sample size; exact size variation remains unmeasured. |
| Classification status | No size threshold has been established. |

### 5-peso consistency checks

Two additional coins identified by the user as 5 pesos were reported to fit the same 101 × 103 px selection box. These are approximate fit checks, not independent tight bounding-box measurements. Mean selection dimension is (width + height) / 2.

| Sample | Approximate location | Selection width | Selection height | Mean selection dimension | Method and observation |
|---|---|---:|---:|---:|---|
| 5P-01 | Near bottom-right edge | 101 px | 103 px | 102 px | Initial manual measurement in Paint |
| 5P-02 | Directly above the initial sample 5P-01, along the rightmost edge | 101 px | 103 px | 102 px | Reused selection box fit check; reported to fit |
| 5P-03 | To the left of the initial sample 5P-01 in the bottommost row | 101 px | 103 px | 102 px | Reused selection box fit check; reported to fit |

| Observation | Result |
|---|---|
| Samples examined | 3: one initial measurement and two additional fit checks |
| Reused selection size | 101 × 103 px |
| Mean of selection width and height | 102 px |
| Interpretation | The additional coins appear consistent with the initial sample size; exact size variation remains unmeasured. |
| Classification status | No size threshold has been established. |
