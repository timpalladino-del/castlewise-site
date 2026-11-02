import SwiftUI
import SwiftData
import UIKit

struct ItemDetailView: View {
    @Environment(\.modelContext) private var context
    @Environment(\.dismiss) private var dismiss
    @Bindable var item: SupplyItem
    @State private var showEdit = false
    @State private var confirmDelete = false

    private let library = ShelfLifeLibrary.shared

    var body: some View {
        List {
            Section {
                HStack {
                    VStack(alignment: .leading, spacing: 4) {
                        Text(item.name).font(.title2.weight(.semibold))
                        if !item.brand.isEmpty { Text(item.brand).foregroundStyle(.secondary) }
                    }
                    Spacer()
                    StatusBadge(item: item)
                }
                if let data = item.photoData, let image = UIImage(data: data) {
                    Image(uiImage: image).resizable().scaledToFit().frame(maxHeight: 220).clipShape(RoundedRectangle(cornerRadius: 12))
                }
            }

            Section("On hand") {
                Stepper(value: $item.quantity, in: 0...9999, step: 1) {
                    HStack {
                        Text("Quantity")
                        Spacer()
                        Text(quantityText).foregroundStyle(.secondary)
                    }
                }
                .onChange(of: item.quantity) { old, new in
                    item.touch()
                    if old > 0 && new == 0 {
                        context.insert(ShoppingItem(name: item.name, quantity: 1, unit: item.unit, reason: "Used up"))
                    }
                    try? context.save()
                }
                LabeledContent("Category", value: item.category.title)
                LabeledContent("Location", value: item.location?.name ?? "Not set")
                if item.isDeepStorage { LabeledContent("Storage", value: "Deep storage") }
                if item.isOpened { LabeledContent("Opened", value: item.openedDate?.formatted(date: .abbreviated, time: .omitted) ?? "Yes") }
            }

            Section("Dates") {
                if let exp = item.expirationDate {
                    LabeledContent(item.category.usesSoftDateLanguage ? "Best by" : "Expires", value: exp.formatted(date: .long, time: .omitted))
                    LabeledContent("Date source", value: item.dateSource.label)
                } else {
                    Text("No date on file").foregroundStyle(.secondary)
                }
                if let p = item.purchaseDate { LabeledContent("Bought", value: p.formatted(date: .abbreviated, time: .omitted)) }
                if let f = item.fillDate { LabeledContent("Filled", value: f.formatted(date: .abbreviated, time: .omitted)) }
                if let c = item.lastCheckedAt { LabeledContent("Last checked", value: c.formatted(date: .abbreviated, time: .omitted)) }
                if case .past = item.status(), item.category.usesSoftDateLanguage {
                    Text("Past its best-by date. Quality fades; safety usually does not. For cans: no rust, dents, or swelling means it is fine to use.")
                        .font(.footnote).foregroundStyle(.secondary)
                    Button("Checked it, still fine") {
                        item.lastCheckedAt = Date()
                        item.snoozedUntil = Calendar.current.date(byAdding: .day, value: 90, to: Date())
                        try? context.save()
                    }
                }
                if item.category == .medicine, item.expirationDate != nil {
                    Text("Medicine: use the printed date. The FDA does not recommend using expired medication.")
                        .font(.footnote).foregroundStyle(.secondary)
                }
            }

            if let entry = library.entry(for: item.shelfLifeKey) {
                Section("Shelf life") {
                    Text(entry.name).font(.subheadline.weight(.medium))
                    Text(entry.summary).font(.footnote)
                    if let note = entry.note { Text(note).font(.footnote).foregroundStyle(.secondary) }
                    Text("Source: \(entry.source)").font(.caption2).foregroundStyle(.tertiary)
                }
            }

            if !item.notes.isEmpty {
                Section("Notes") { Text(item.notes) }
            }

            if let code = item.barcode {
                Section("Barcode") { Text(code).font(.system(.footnote, design: .monospaced)) }
            }

            Section {
                Button("Delete item", role: .destructive) { confirmDelete = true }
            }
        }
        .navigationTitle("")
        .navigationBarTitleDisplayMode(.inline)
        .toolbar {
            ToolbarItem(placement: .topBarTrailing) { Button("Edit") { showEdit = true } }
        }
        .sheet(isPresented: $showEdit) { ItemEditView(item: item) }
        .confirmationDialog("Delete \(item.name)?", isPresented: $confirmDelete, titleVisibility: .visible) {
            Button("Delete", role: .destructive) {
                context.delete(item)
                try? context.save()
                dismiss()
            }
        }
    }

    private var quantityText: String {
        let q = item.quantity
        let qs = q == q.rounded() ? String(Int(q)) : String(format: "%.1f", q)
        return "\(qs) \(item.unit)"
    }
}
