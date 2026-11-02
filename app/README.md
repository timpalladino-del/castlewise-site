# Lantern

The thing you reach for when the power dies. A calm supplies-and-gear tracker for ordinary households.

iPhone app for ordinary households that keep extra food, water, medicine and gear for a bad week.
Separate product from CastleWise; shares the model (local-first, no account, one-time price, honest
reference library, camera assist), not the branding. The product plan is in `docs/plan.md`.

Bundle identifier: `io.kismetstudio.lantern`. Visual identity is still to be designed.

## Status: MVP scaffold, written without a compiler

This code was written in a Linux environment with no Xcode. It follows standard SwiftUI, SwiftData and
VisionKit APIs for iOS 17, but it has not been compiled. Expect a handful of small fixes on first build.
The Xcode project uses Xcode 16's synchronized folders, so new files dropped into `Lantern/` or
`LanternTests/` are picked up automatically.

## Open it

1. Xcode 16 or later. Open `Lantern.xcodeproj`.
2. Select the Lantern target, set your team under Signing & Capabilities.
3. Run on a device for scanning. The simulator cannot run the VisionKit scanner; the app shows a
   manual barcode field there instead.
4. Press Cmd-U to run the unit tests (date parser, shelf-life library, readiness math).

## What is built (Phase 1 of the plan)

- **Supplies inventory**: name, brand, category, quantity, unit, location, purchase date, best-by date
  with its source (printed, estimated, by hand), opened and deep-storage flags, water fill date, notes,
  photo, barcode. Swipe: Used 1, Checked, Delete. Used-up items go to the shopping list.
- **Scanning**: VisionKit reads barcodes and printed text together, on-device. Printed dates are parsed by
  `DateLabelParser` (ISO, US slash, month names, compact codes). Barcodes are remembered per device
  (`ProductMemory`) so the second scan is one tap. Optional Open Food Facts lookup for names.
- **Shelf-life library**: 94 curated entries with sources in `shelf_life.json`, covering pantry staples,
  deep storage, water, batteries, fuel, medicine and hygiene. "Estimate from shelf life" fills a best-by
  date from the purchase, fill or opened date. Entries flagged `requiresPrintedDate` (medicine, formula)
  never get a soft estimate.
- **Alerts**: one weekly digest by default; per-item reminders only for medicine, baby items, water
  rotation and equipment checks. All local notifications.
- **Equipment**: 17 kinds with default check schedules from NFPA, CPSC and manufacturer guidance in
  `maintenance_intervals.json`. Mark done, add custom checks, keep a notes history, "replace by" dates
  for alarms, extinguishers and propane cylinders.
- **Readiness view**: household size and target days; food and water days-of-supply using Red Cross and
  CDC rates; gap list with one-tap add to the shopping list.
- **Starter packs**: Ready.gov basic kit, two-week pantry for two, power outage basics.
- **Export**: CSV for supplies and equipment, free.

## Not yet built

Kits and go-bags as containers, household sharing (CloudKit), seasonal hazard checklists, documents
vault, medication tracker, Outage Mode, camera "is this still good" check, scenario planner, widgets,
onboarding flow, app icon, Pro purchase. See the plan for phases.

## Data

- `Lantern/Library/Resources/shelf_life.json`: hand-curated. To add the full USDA FoodKeeper table
  (public domain), download it from data.gov and run `Scripts/import_foodkeeper.py`.
- `maintenance_intervals.json`: equipment kinds and default tasks.
- `starter_packs.json`: starter lists.

## Licensing notes

USDA, CDC, FEMA and Ready.gov text is public domain. FoodKeeper is CC0. Open Food Facts product data is
ODbL: attribution is shown in the scanner and settings; only the barcode is sent, and nothing is
redistributed. Red Cross, NFPA, LDS and manufacturer figures are paraphrased and cited, not copied.
