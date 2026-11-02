import SwiftUI

struct ShelfLifePickerView: View {
    @Environment(\.dismiss) private var dismiss
    var category: SupplyCategory?
    let onPick: (ShelfLifeEntry) -> Void

    @State private var query = ""
    @State private var allCategories = true
    private let library = ShelfLifeLibrary.shared

    private var results: [ShelfLifeEntry] {
        library.search(query, category: allCategories ? nil : category)
    }

    var body: some View {
        NavigationStack {
            List {
                if category != nil {
                    Toggle("Search every category", isOn: $allCategories)
                }
                ForEach(results) { entry in
                    Button {
                        onPick(entry)
                        dismiss()
                    } label: {
                        VStack(alignment: .leading, spacing: 3) {
                            Text(entry.name).foregroundStyle(.primary)
                            Text(entry.summary).font(.caption).foregroundStyle(.secondary)
                        }
                    }
                }
                if results.isEmpty {
                    Text("No match. Save without one and set the date by hand.").foregroundStyle(.secondary)
                }
            }
            .searchable(text: $query, prompt: "rice, batteries, tuna…")
            .navigationTitle("Shelf life")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar { ToolbarItem(placement: .cancellationAction) { Button("Cancel") { dismiss() } } }
        }
    }
}
