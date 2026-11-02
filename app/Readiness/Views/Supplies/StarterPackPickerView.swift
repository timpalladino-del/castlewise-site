import SwiftUI
import SwiftData

struct StarterPackPickerView: View {
    @Environment(\.modelContext) private var context
    @Environment(\.dismiss) private var dismiss
    @Query(sort: \StorageLocation.sortOrder) private var locations: [StorageLocation]

    @State private var selected: StarterPack?
    @State private var location: StorageLocation?
    private let library = StarterPackLibrary.shared

    var body: some View {
        NavigationStack {
            List {
                Section {
                    Text("A starter list adds items with today's date and an estimated best-by. Edit quantities afterwards to match what you actually have.")
                        .font(.footnote).foregroundStyle(.secondary)
                }
                Section("Lists") {
                    ForEach(library.packs) { pack in
                        Button {
                            selected = pack
                        } label: {
                            HStack {
                                VStack(alignment: .leading, spacing: 3) {
                                    Text(pack.name).foregroundStyle(.primary)
                                    Text(pack.summary).font(.caption).foregroundStyle(.secondary)
                                    Text("\(pack.items.count) items · \(pack.source)").font(.caption2).foregroundStyle(.tertiary)
                                }
                                Spacer()
                                if selected?.key == pack.key { Image(systemName: "checkmark.circle.fill").foregroundStyle(Color.accentColor) }
                            }
                        }
                    }
                }
                Section("Put them in") {
                    Picker("Location", selection: $location) {
                        Text("Not set").tag(Optional<StorageLocation>.none)
                        ForEach(locations) { loc in Text(loc.name).tag(Optional(loc)) }
                    }
                }
            }
            .navigationTitle("Starter packs")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .cancellationAction) { Button("Cancel") { dismiss() } }
                ToolbarItem(placement: .confirmationAction) {
                    Button("Add") {
                        if let pack = selected {
                            SeedData.apply(pack: pack, to: location, in: context)
                        }
                        dismiss()
                    }
                    .disabled(selected == nil)
                }
            }
            .onAppear { if location == nil { location = locations.first } }
        }
    }
}
