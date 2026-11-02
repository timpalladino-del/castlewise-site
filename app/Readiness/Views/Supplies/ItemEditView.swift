import SwiftUI
import SwiftData

/// Prefill for the editor when something was scanned or picked from a list.
struct ItemDraft {
    var name = ""
    var brand = ""
    var category: SupplyCategory = .food
    var quantity: Double = 1
    var unit: String = CommonUnit.each.rawValue
    var barcode: String?
    var expirationDate: Date?
    var dateSource: DateSource = .none
    var shelfLifeKey: String?
    var servingsPerUnit: Double?
    var litersPerUnit: Double?
    var location: StorageLocation?
}

struct ItemEditView: View {
    @Environment(\.modelContext) private var context
    @Environment(\.dismiss) private var dismiss
    @Query(sort: \StorageLocation.sortOrder) private var locations: [StorageLocation]

    private let existing: SupplyItem?
    private let onSaved: ((SupplyItem) -> Void)?
    private let library = ShelfLifeLibrary.shared

    @State private var name: String
    @State private var brand: String
    @State private var category: SupplyCategory
    @State private var quantity: Double
    @State private var unit: String
    @State private var barcode: String?
    @State private var location: StorageLocation?
    @State private var hasPurchaseDate: Bool
    @State private var purchaseDate: Date
    @State private var hasExpiration: Bool
    @State private var expirationDate: Date
    @State private var dateSource: DateSource
    @State private var isOpened: Bool
    @State private var openedDate: Date
    @State private var isDeepStorage: Bool
    @State private var hasFillDate: Bool
    @State private var fillDate: Date
    @State private var shelfLifeKey: String?
    @State private var servingsPerUnit: Double?
    @State private var litersPerUnit: Double?
    @State private var notes: String
    @State private var photoData: Data?
    @State private var showShelfLifePicker = false
    @State private var dismissedSuggestion = false

    init(item: SupplyItem? = nil, draft: ItemDraft = ItemDraft(), onSaved: ((SupplyItem) -> Void)? = nil) {
        self.existing = item
        self.onSaved = onSaved
        let now = Date()
        _name = State(initialValue: item?.name ?? draft.name)
        _brand = State(initialValue: item?.brand ?? draft.brand)
        _category = State(initialValue: item?.category ?? draft.category)
        _quantity = State(initialValue: item?.quantity ?? draft.quantity)
        _unit = State(initialValue: item?.unit ?? draft.unit)
        _barcode = State(initialValue: item?.barcode ?? draft.barcode)
        _location = State(initialValue: item?.location ?? draft.location)
        _hasPurchaseDate = State(initialValue: item?.purchaseDate != nil || item == nil)
        _purchaseDate = State(initialValue: item?.purchaseDate ?? now)
        let exp = item?.expirationDate ?? draft.expirationDate
        _hasExpiration = State(initialValue: exp != nil)
        _expirationDate = State(initialValue: exp ?? Calendar.current.date(byAdding: .year, value: 1, to: now) ?? now)
        _dateSource = State(initialValue: item?.dateSource ?? draft.dateSource)
        _isOpened = State(initialValue: item?.isOpened ?? false)
        _openedDate = State(initialValue: item?.openedDate ?? now)
        _isDeepStorage = State(initialValue: item?.isDeepStorage ?? false)
        _hasFillDate = State(initialValue: item?.fillDate != nil)
        _fillDate = State(initialValue: item?.fillDate ?? now)
        _shelfLifeKey = State(initialValue: item?.shelfLifeKey ?? draft.shelfLifeKey)
        _servingsPerUnit = State(initialValue: item?.servingsPerUnit ?? draft.servingsPerUnit)
        _litersPerUnit = State(initialValue: item?.litersPerUnit ?? draft.litersPerUnit)
        _notes = State(initialValue: item?.notes ?? "")
        _photoData = State(initialValue: item?.photoData)
    }

    private var selectedEntry: ShelfLifeEntry? { library.entry(for: shelfLifeKey) }

    private var suggestion: ShelfLifeEntry? {
        guard shelfLifeKey == nil, !dismissedSuggestion, name.count >= 3 else { return nil }
        return library.bestMatch(for: name)
    }

    var body: some View {
        NavigationStack {
            Form {
                Section {
                    TextField("Name", text: $name)
                    TextField("Brand (optional)", text: $brand)
                    Picker("Category", selection: $category) {
                        ForEach(SupplyCategory.allCases) { c in Label(c.title, systemImage: c.symbol).tag(c) }
                    }
                    if let s = suggestion {
                        VStack(alignment: .leading, spacing: 6) {
                            Text("Looks like **\(s.name)**. \(s.summary).").font(.footnote)
                            HStack {
                                Button("Use this") { apply(entry: s) }
                                    .buttonStyle(.borderedProminent).controlSize(.small)
                                Button("No") { dismissedSuggestion = true }
                                    .buttonStyle(.bordered).controlSize(.small)
                            }
                        }
                    }
                }

                Section("How much") {
                    HStack {
                        Text("Quantity")
                        Spacer()
                        TextField("1", value: $quantity, format: .number)
                            .keyboardType(.decimalPad)
                            .multilineTextAlignment(.trailing)
                            .frame(width: 90)
                    }
                    Picker("Unit", selection: $unit) {
                        ForEach(CommonUnit.allCases) { u in Text(u.rawValue).tag(u.rawValue) }
                    }
                    .onChange(of: unit) { _, new in
                        if category == .water, litersPerUnit == nil, let l = CommonUnit(rawValue: new)?.litersPerUnit { litersPerUnit = l }
                    }
                    Picker("Location", selection: $location) {
                        Text("Not set").tag(Optional<StorageLocation>.none)
                        ForEach(locations) { loc in Text(loc.name).tag(Optional(loc)) }
                    }
                    if category == .food {
                        HStack {
                            Text("Servings per \(unit)")
                            Spacer()
                            TextField("1", value: $servingsPerUnit, format: .number)
                                .keyboardType(.decimalPad).multilineTextAlignment(.trailing).frame(width: 90)
                        }
                    }
                    if category == .water {
                        HStack {
                            Text("Litres per \(unit)")
                            Spacer()
                            TextField("3.785", value: $litersPerUnit, format: .number)
                                .keyboardType(.decimalPad).multilineTextAlignment(.trailing).frame(width: 90)
                        }
                    }
                }

                Section("Shelf life") {
                    Button {
                        showShelfLifePicker = true
                    } label: {
                        HStack {
                            Text(selectedEntry?.name ?? "Match to the shelf-life table")
                            Spacer()
                            Image(systemName: "chevron.right").foregroundStyle(.tertiary)
                        }
                    }
                    .foregroundStyle(.primary)
                    if let e = selectedEntry {
                        Text(e.summary).font(.footnote).foregroundStyle(.secondary)
                        if e.needsPrintedDate {
                            Text("Use the date printed on the package for this one.").font(.footnote).foregroundStyle(.orange)
                        }
                    }
                    Toggle("Opened", isOn: $isOpened)
                    if isOpened {
                        DatePicker("Opened on", selection: $openedDate, displayedComponents: .date)
                    }
                    Toggle("Deep storage (sealed, cool, dry)", isOn: $isDeepStorage)
                }

                Section("Dates") {
                    Toggle("Bought on", isOn: $hasPurchaseDate)
                    if hasPurchaseDate {
                        DatePicker("Purchase date", selection: $purchaseDate, displayedComponents: .date)
                    }
                    if category == .water {
                        Toggle("Filled by me", isOn: $hasFillDate)
                        if hasFillDate {
                            DatePicker("Fill date", selection: $fillDate, displayedComponents: .date)
                            Text("CDC: rotate self-filled water every 6 months.").font(.footnote).foregroundStyle(.secondary)
                        }
                    }
                    Toggle(category.usesSoftDateLanguage ? "Best-by date" : "Expiration date", isOn: $hasExpiration)
                    if hasExpiration {
                        DatePicker("Date", selection: $expirationDate, displayedComponents: .date)
                        Picker("Where from", selection: $dateSource) {
                            Text("Printed").tag(DateSource.printed)
                            Text("Estimated").tag(DateSource.estimated)
                            Text("By hand").tag(DateSource.manual)
                        }
                        .pickerStyle(.segmented)
                    }
                    if selectedEntry != nil, !(selectedEntry?.isIndefinite ?? false) {
                        Button("Estimate from shelf life") { estimate() }
                    }
                    if selectedEntry?.isIndefinite == true {
                        Text("Keeps indefinitely. No date needed.").font(.footnote).foregroundStyle(.secondary)
                    }
                }

                Section("Notes") {
                    TextField("Anything worth remembering", text: $notes, axis: .vertical)
                        .lineLimit(2...6)
                    PhotoPickerButton(photoData: $photoData)
                }

                if let code = barcode {
                    Section("Barcode") {
                        Text(code).font(.system(.footnote, design: .monospaced)).foregroundStyle(.secondary)
                    }
                }
            }
            .navigationTitle(existing == nil ? "New item" : "Edit item")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .cancellationAction) { Button("Cancel") { dismiss() } }
                ToolbarItem(placement: .confirmationAction) {
                    Button("Save") { save() }.disabled(name.trimmingCharacters(in: .whitespaces).isEmpty)
                }
            }
            .sheet(isPresented: $showShelfLifePicker) {
                ShelfLifePickerView(category: category) { entry in apply(entry: entry) }
            }
            .onAppear {
                if existing == nil, shelfLifeKey == nil, !name.isEmpty, let m = library.bestMatch(for: name, category: category) {
                    apply(entry: m)
                }
            }
        }
    }

    private func apply(entry: ShelfLifeEntry) {
        shelfLifeKey = entry.key
        if category != entry.supplyCategory, existing == nil { category = entry.supplyCategory }
        dismissedSuggestion = true
        if !hasExpiration, !entry.needsPrintedDate { estimate() }
    }

    private func estimate() {
        guard let entry = selectedEntry else { return }
        let start: Date
        if category == .water, hasFillDate { start = fillDate }
        else if isOpened { start = openedDate }
        else if hasPurchaseDate { start = purchaseDate }
        else { start = Date() }
        if let est = entry.estimate(from: start, opened: isOpened, deepStorage: isDeepStorage) {
            expirationDate = est
            hasExpiration = true
            dateSource = .estimated
        }
    }

    private func save() {
        let item = existing ?? SupplyItem(name: name, category: category)
        item.name = name.trimmingCharacters(in: .whitespaces)
        item.brand = brand.trimmingCharacters(in: .whitespaces)
        item.category = category
        item.quantity = max(0, quantity)
        item.unit = unit
        item.barcode = barcode
        item.location = location
        item.purchaseDate = hasPurchaseDate ? purchaseDate : nil
        item.expirationDate = hasExpiration ? expirationDate : nil
        item.dateSource = hasExpiration ? (dateSource == .none ? .manual : dateSource) : .none
        item.isOpened = isOpened
        item.openedDate = isOpened ? openedDate : nil
        item.isDeepStorage = isDeepStorage
        item.fillDate = (category == .water && hasFillDate) ? fillDate : nil
        item.shelfLifeKey = shelfLifeKey
        item.servingsPerUnit = category == .food ? servingsPerUnit : nil
        item.litersPerUnit = category == .water ? (litersPerUnit ?? CommonUnit(rawValue: unit)?.litersPerUnit) : nil
        item.notes = notes
        item.photoData = photoData
        item.snoozedUntil = nil
        item.touch()
        if existing == nil { context.insert(item) }

        if let code = barcode, !code.isEmpty {
            let descriptor = FetchDescriptor<ProductMemory>(predicate: #Predicate { $0.barcode == code })
            let memory = (try? context.fetch(descriptor))?.first ?? {
                let m = ProductMemory(barcode: code, name: item.name)
                context.insert(m)
                return m
            }()
            memory.remember(from: item)
        }

        try? context.save()
        onSaved?(item)
        dismiss()
    }
}
