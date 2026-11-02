import SwiftUI
import SwiftData
import VisionKit
import Vision

/// Camera wrapper. Barcodes and printed text, on-device, nothing recorded.
struct DataScannerRepresentable: UIViewControllerRepresentable {
    var onBarcode: (String) -> Void
    var onText: (String) -> Void

    static var isSupported: Bool {
        DataScannerViewController.isSupported && DataScannerViewController.isAvailable
    }

    func makeUIViewController(context: Context) -> DataScannerViewController {
        let types: Set<DataScannerViewController.RecognizedDataType> = [
            .barcode(symbologies: [.ean13, .ean8, .upce, .code128, .code39, .qr, .dataMatrix, .itf14]),
            .text()
        ]
        let vc = DataScannerViewController(
            recognizedDataTypes: types,
            qualityLevel: .balanced,
            recognizesMultipleItems: true,
            isHighFrameRateTrackingEnabled: false,
            isHighlightingEnabled: true
        )
        vc.delegate = context.coordinator
        return vc
    }

    func updateUIViewController(_ vc: DataScannerViewController, context: Context) {
        context.coordinator.parent = self
        if !vc.isScanning { try? vc.startScanning() }
    }

    static func dismantleUIViewController(_ vc: DataScannerViewController, coordinator: Coordinator) {
        vc.stopScanning()
    }

    func makeCoordinator() -> Coordinator { Coordinator(parent: self) }

    final class Coordinator: NSObject, DataScannerViewControllerDelegate {
        var parent: DataScannerRepresentable
        init(parent: DataScannerRepresentable) { self.parent = parent }

        func dataScanner(_ dataScanner: DataScannerViewController, didAdd addedItems: [RecognizedItem], allItems: [RecognizedItem]) {
            handle(addedItems)
        }

        func dataScanner(_ dataScanner: DataScannerViewController, didUpdate updatedItems: [RecognizedItem], allItems: [RecognizedItem]) {
            handle(updatedItems)
        }

        private func handle(_ items: [RecognizedItem]) {
            for item in items {
                switch item {
                case .barcode(let barcode):
                    if let value = barcode.payloadStringValue { parent.onBarcode(value) }
                case .text(let text):
                    parent.onText(text.transcription)
                @unknown default:
                    break
                }
            }
        }
    }
}

struct ScanSheet: View {
    @Environment(\.modelContext) private var context
    @Environment(\.dismiss) private var dismiss
    @Query private var items: [SupplyItem]
    @AppStorage(AppSettings.onlineLookupEnabled) private var onlineLookup = true

    @State private var barcode: String?
    @State private var productName: String?
    @State private var productBrand: String?
    @State private var memory: ProductMemory?
    @State private var isLookingUp = false
    @State private var bestDate: DateLabelParser.Candidate?
    @State private var manualBarcode = ""
    @State private var showEditor = false
    @State private var addedToExisting: String?

    private let parser = DateLabelParser()

    private var existingMatches: [SupplyItem] {
        guard let barcode else { return [] }
        return items.filter { $0.barcode == barcode }
    }

    var body: some View {
        NavigationStack {
            VStack(spacing: 0) {
                if DataScannerRepresentable.isSupported {
                    DataScannerRepresentable(onBarcode: handleBarcode, onText: handleText)
                        .ignoresSafeArea(edges: .top)
                } else {
                    VStack(spacing: 12) {
                        EmptyStateView(symbol: "camera.fill", title: "Camera scanning not available",
                                       message: "This device or simulator cannot scan. Type the barcode instead, or add the item by hand.")
                        HStack {
                            TextField("Barcode digits", text: $manualBarcode).keyboardType(.numberPad).textFieldStyle(.roundedBorder)
                            Button("Use") { handleBarcode(manualBarcode) }.disabled(manualBarcode.isEmpty)
                        }.padding(.horizontal)
                    }
                    .frame(maxHeight: .infinity)
                }
                panel
            }
            .navigationTitle("Scan")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .cancellationAction) { Button("Done") { dismiss() } }
            }
            .sheet(isPresented: $showEditor) {
                ItemEditView(draft: draft) { _ in reset() }
            }
        }
    }

    private var panel: some View {
        VStack(alignment: .leading, spacing: 10) {
            HStack(spacing: 8) {
                Chip(symbol: "barcode", text: barcode.map { productName ?? "Barcode \($0)" } ?? "Point at a barcode", active: barcode != nil)
                Chip(symbol: "calendar", text: bestDate.map { "Date: " + $0.date.formatted(date: .abbreviated, time: .omitted) } ?? "Point at the printed date", active: bestDate != nil)
            }
            if isLookingUp { ProgressView("Looking up product…").font(.caption) }
            if let added = addedToExisting {
                Label("Added 1 to \(added)", systemImage: "checkmark.circle.fill").foregroundStyle(.green).font(.subheadline)
            }
            HStack(spacing: 10) {
                if let first = existingMatches.first {
                    Button {
                        first.quantity += 1
                        first.touch()
                        if let d = bestDate?.date, first.expirationDate == nil || d > (first.expirationDate ?? d) {
                            first.expirationDate = d
                            first.dateSource = .printed
                        }
                        try? context.save()
                        addedToExisting = first.name
                        barcode = nil
                        bestDate = nil
                    } label: { Label("Add 1 to \(first.name)", systemImage: "plus.circle.fill") }
                    .buttonStyle(.borderedProminent)
                }
                if existingMatches.isEmpty {
                    Button { showEditor = true } label: { Label(barcode == nil && bestDate == nil ? "Add by hand" : "New item", systemImage: "square.and.pencil") }
                        .buttonStyle(.borderedProminent)
                } else {
                    Button { showEditor = true } label: { Label("New item", systemImage: "square.and.pencil") }
                        .buttonStyle(.bordered)
                }
                if barcode != nil || bestDate != nil {
                    Button("Rescan") { reset() }.buttonStyle(.bordered)
                }
            }
            Text(onlineLookup ? OpenFoodFactsClient.attribution : "Online barcode lookup is off. Names come from your own history.")
                .font(.caption2).foregroundStyle(.tertiary)
        }
        .padding()
        .background(Color(uiColor: .systemGroupedBackground))
    }

    private var draft: ItemDraft {
        var d = ItemDraft()
        d.barcode = barcode
        d.name = productName ?? ""
        d.brand = productBrand ?? ""
        if let m = memory {
            d.category = m.category
            d.unit = m.unit
            d.shelfLifeKey = m.shelfLifeKey
            d.servingsPerUnit = m.servingsPerUnit
            d.litersPerUnit = m.litersPerUnit
        }
        if let c = bestDate {
            d.expirationDate = c.date
            d.dateSource = .printed
        }
        return d
    }

    private func reset() {
        barcode = nil
        productName = nil
        productBrand = nil
        memory = nil
        bestDate = nil
        addedToExisting = nil
    }

    private func handleBarcode(_ code: String) {
        let clean = code.trimmingCharacters(in: .whitespacesAndNewlines)
        guard barcode == nil, !clean.isEmpty else { return }
        barcode = clean
        addedToExisting = nil

        let descriptor = FetchDescriptor<ProductMemory>(predicate: #Predicate { $0.barcode == clean })
        if let m = (try? context.fetch(descriptor))?.first {
            memory = m
            productName = m.name
            productBrand = m.brand
            return
        }
        if let existing = items.first(where: { $0.barcode == clean }) {
            productName = existing.name
            productBrand = existing.brand
            return
        }
        guard onlineLookup else { return }
        isLookingUp = true
        Task {
            let info = try? await OpenFoodFactsClient.shared.lookup(barcode: clean)
            await MainActor.run {
                isLookingUp = false
                if let info, barcode == clean {
                    productName = info.name
                    productBrand = info.brand
                }
            }
        }
    }

    private func handleText(_ text: String) {
        guard let candidate = parser.parse(text).first, candidate.confidence >= 0.55 else { return }
        if let current = bestDate, current.confidence >= candidate.confidence { return }
        bestDate = candidate
    }
}

private struct Chip: View {
    let symbol: String
    let text: String
    let active: Bool

    var body: some View {
        Label(text, systemImage: symbol)
            .font(.caption.weight(.medium))
            .lineLimit(1)
            .padding(.horizontal, 10)
            .padding(.vertical, 6)
            .background((active ? Color.accentColor : Color.secondary).opacity(0.15), in: Capsule())
            .foregroundStyle(active ? Color.accentColor : Color.secondary)
    }
}
