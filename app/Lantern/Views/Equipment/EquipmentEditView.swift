import SwiftUI
import SwiftData

struct EquipmentEditView: View {
    @Environment(\.modelContext) private var context
    @Environment(\.dismiss) private var dismiss
    @Query(sort: \StorageLocation.sortOrder) private var locations: [StorageLocation]

    private let existing: Equipment?
    private let library = MaintenanceLibrary.shared

    @State private var name: String
    @State private var kind: String
    @State private var brand: String
    @State private var model: String
    @State private var serial: String
    @State private var location: StorageLocation?
    @State private var hasPurchaseDate: Bool
    @State private var purchaseDate: Date
    @State private var hasManufactureDate: Bool
    @State private var manufactureDate: Date
    @State private var fuelType: String
    @State private var batteryType: String
    @State private var batteryCount: Int
    @State private var notes: String
    @State private var photoData: Data?
    @State private var addDefaultTasks = true

    init(equipment: Equipment? = nil) {
        existing = equipment
        let now = Date()
        _name = State(initialValue: equipment?.name ?? "")
        _kind = State(initialValue: equipment?.kindRaw ?? "generator")
        _brand = State(initialValue: equipment?.brand ?? "")
        _model = State(initialValue: equipment?.model ?? "")
        _serial = State(initialValue: equipment?.serial ?? "")
        _location = State(initialValue: equipment?.location)
        _hasPurchaseDate = State(initialValue: equipment?.purchaseDate != nil)
        _purchaseDate = State(initialValue: equipment?.purchaseDate ?? now)
        _hasManufactureDate = State(initialValue: equipment?.manufactureDate != nil)
        _manufactureDate = State(initialValue: equipment?.manufactureDate ?? now)
        _fuelType = State(initialValue: equipment?.fuelType ?? "")
        _batteryType = State(initialValue: equipment?.batteryType ?? "")
        _batteryCount = State(initialValue: equipment?.batteryCount ?? 0)
        _notes = State(initialValue: equipment?.notes ?? "")
        _photoData = State(initialValue: equipment?.photoData)
    }

    private var kindInfo: EquipmentKind? { library.kind(kind) }

    var body: some View {
        NavigationStack {
            Form {
                Section {
                    TextField("Name (e.g. Garage generator)", text: $name)
                    Picker("Kind", selection: $kind) {
                        ForEach(library.kinds) { k in Label(k.name, systemImage: k.symbol).tag(k.kind) }
                    }
                    .onChange(of: kind) { _, new in
                        if name.isEmpty, let k = library.kind(new) { name = k.name }
                    }
                    Picker("Location", selection: $location) {
                        Text("Not set").tag(Optional<StorageLocation>.none)
                        ForEach(locations) { loc in Text(loc.name).tag(Optional(loc)) }
                    }
                }

                Section("Details") {
                    TextField("Brand", text: $brand)
                    TextField("Model", text: $model)
                    TextField("Serial number", text: $serial)
                    TextField("Fuel (gas, propane, none)", text: $fuelType)
                    TextField("Battery type (AA, 9V, CR123)", text: $batteryType)
                    Stepper("Batteries needed: \(batteryCount)", value: $batteryCount, in: 0...48)
                }

                Section("Dates") {
                    Toggle("Purchase date", isOn: $hasPurchaseDate)
                    if hasPurchaseDate { DatePicker("Bought", selection: $purchaseDate, displayedComponents: .date) }
                    Toggle("Manufacture date", isOn: $hasManufactureDate)
                    if hasManufactureDate { DatePicker("Made", selection: $manufactureDate, displayedComponents: .date) }
                    if let years = kindInfo?.lifetimeYears {
                        Text("\(kindInfo?.name ?? "This") should be replaced \(years) years after the manufacture date. It is usually printed on the back.")
                            .font(.footnote).foregroundStyle(.secondary)
                    }
                }

                if existing == nil, let k = kindInfo, !k.tasks.isEmpty {
                    Section("Check schedule") {
                        Toggle("Add the standard checks", isOn: $addDefaultTasks)
                        ForEach(k.tasks, id: \.title) { t in
                            VStack(alignment: .leading, spacing: 2) {
                                Text(t.title).font(.subheadline)
                                Text("Every \(ShelfLifeEntry.humanDays(t.intervalDays))\(t.source.map { " · \($0)" } ?? "")").font(.caption).foregroundStyle(.secondary)
                            }
                        }
                    }
                }

                Section("Notes") {
                    TextField("Oil type, fuel stabilizer, where the key is…", text: $notes, axis: .vertical).lineLimit(2...6)
                    PhotoPickerButton(photoData: $photoData)
                }
            }
            .navigationTitle(existing == nil ? "New equipment" : "Edit equipment")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .cancellationAction) { Button("Cancel") { dismiss() } }
                ToolbarItem(placement: .confirmationAction) {
                    Button("Save") { save() }.disabled(name.trimmingCharacters(in: .whitespaces).isEmpty)
                }
            }
        }
    }

    private func save() {
        let e = existing ?? Equipment(name: name, kind: kind)
        e.name = name.trimmingCharacters(in: .whitespaces)
        e.kindRaw = kind
        e.brand = brand
        e.model = model
        e.serial = serial
        e.location = location
        e.purchaseDate = hasPurchaseDate ? purchaseDate : nil
        e.manufactureDate = hasManufactureDate ? manufactureDate : nil
        e.fuelType = fuelType
        e.batteryType = batteryType
        e.batteryCount = batteryCount
        e.notes = notes
        e.photoData = photoData
        if existing == nil {
            context.insert(e)
            if addDefaultTasks, let k = kindInfo {
                for t in k.tasks {
                    let task = MaintenanceTask(title: t.title, intervalDays: t.intervalDays, note: t.note ?? "", source: t.source ?? "", anchorDate: Date())
                    task.equipment = e
                    context.insert(task)
                }
            }
        }
        try? context.save()
        dismiss()
    }
}
