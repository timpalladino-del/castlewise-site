import SwiftUI
import SwiftData

struct SuppliesListView: View {
    enum SortMode: String, CaseIterable, Identifiable {
        case date = "By date", name = "By name", location = "By location"
        var id: String { rawValue }
    }

    @Environment(\.modelContext) private var context
    @Query(sort: \SupplyItem.name) private var items: [SupplyItem]
    @Query(sort: \StorageLocation.sortOrder) private var locations: [StorageLocation]

    @State private var searchText = ""
    @State private var category: SupplyCategory?
    @State private var sortMode: SortMode = .date
    @State private var showScanner = false
    @State private var showAdd = false
    @State private var showPacks = false
    @State private var showShopping = false
    @State private var showOut = true

    private var now: Date { Date() }

    private var filtered: [SupplyItem] {
        var list = items
        if let category { list = list.filter { $0.category == category } }
        if !showOut { list = list.filter { $0.quantity > 0 } }
        let q = searchText.trimmingCharacters(in: .whitespaces).lowercased()
        if !q.isEmpty {
            list = list.filter { $0.name.lowercased().contains(q) || $0.brand.lowercased().contains(q) || ($0.location?.name.lowercased().contains(q) ?? false) }
        }
        switch sortMode {
        case .name:
            return list.sorted { $0.name.localizedCaseInsensitiveCompare($1.name) == .orderedAscending }
        case .date, .location:
            return list.sorted { a, b in
                let sa = a.status(on: now), sb = b.status(on: now)
                if sa.sortRank != sb.sortRank { return sa.sortRank < sb.sortRank }
                let da = a.daysUntilExpiration(from: now) ?? Int.max
                let db = b.daysUntilExpiration(from: now) ?? Int.max
                if da != db { return da < db }
                return a.name < b.name
            }
        }
    }

    private var groupedByLocation: [(String, [SupplyItem])] {
        let groups = Dictionary(grouping: filtered) { $0.location?.name ?? "No location" }
        let order = locations.map(\.name) + ["No location"]
        return order.compactMap { name in
            guard let g = groups[name], !g.isEmpty else { return nil }
            return (name, g)
        }
    }

    var body: some View {
        NavigationStack {
            Group {
                if items.isEmpty {
                    emptyState
                } else {
                    list
                }
            }
            .navigationTitle("Supplies")
            .searchable(text: $searchText, prompt: "Search supplies")
            .toolbar {
                ToolbarItemGroup(placement: .topBarTrailing) {
                    Button { showScanner = true } label: { Image(systemName: "barcode.viewfinder") }
                    Menu {
                        Button { showAdd = true } label: { Label("Add by hand", systemImage: "plus") }
                        Button { showPacks = true } label: { Label("Add a starter pack", systemImage: "list.bullet.rectangle") }
                        Button { showShopping = true } label: { Label("Shopping list", systemImage: "cart") }
                        Divider()
                        Picker("Sort", selection: $sortMode) {
                            ForEach(SortMode.allCases) { Text($0.rawValue).tag($0) }
                        }
                        Toggle("Show used-up items", isOn: $showOut)
                    } label: { Image(systemName: "ellipsis.circle") }
                }
                ToolbarItem(placement: .topBarLeading) {
                    Menu {
                        Button("All categories") { category = nil }
                        ForEach(SupplyCategory.allCases) { c in
                            Button { category = c } label: { Label(c.title, systemImage: c.symbol) }
                        }
                    } label: {
                        Label(category?.title ?? "All", systemImage: category?.symbol ?? "line.3.horizontal.decrease.circle")
                    }
                }
            }
            .sheet(isPresented: $showScanner) { ScanSheet() }
            .sheet(isPresented: $showAdd) { ItemEditView() }
            .sheet(isPresented: $showPacks) { StarterPackPickerView() }
            .sheet(isPresented: $showShopping) { ShoppingListView() }
        }
    }

    private var emptyState: some View {
        VStack(spacing: 16) {
            EmptyStateView(symbol: "shippingbox", title: "Nothing here yet",
                           message: "Scan a barcode, add an item by hand, or start from a ready-made list.")
            HStack(spacing: 12) {
                Button { showScanner = true } label: { Label("Scan", systemImage: "barcode.viewfinder") }
                    .buttonStyle(.borderedProminent)
                Button { showPacks = true } label: { Label("Starter pack", systemImage: "list.bullet.rectangle") }
                    .buttonStyle(.bordered)
            }
        }
    }

    private var list: some View {
        List {
            if sortMode == .location {
                ForEach(groupedByLocation, id: \.0) { name, group in
                    Section(name) {
                        ForEach(group) { item in row(item) }
                    }
                }
            } else {
                ForEach(filtered) { item in row(item) }
            }
        }
    }

    private func row(_ item: SupplyItem) -> some View {
        NavigationLink { ItemDetailView(item: item) } label: { ItemRow(item: item) }
            .swipeActions(edge: .trailing, allowsFullSwipe: true) {
                Button { useOne(item) } label: { Label("Used 1", systemImage: "minus.circle") }
                    .tint(.blue)
                Button(role: .destructive) { context.delete(item); try? context.save() } label: { Label("Delete", systemImage: "trash") }
            }
            .swipeActions(edge: .leading) {
                Button { checked(item) } label: { Label("Checked", systemImage: "checkmark.circle") }
                    .tint(.green)
            }
    }

    private func useOne(_ item: SupplyItem) {
        item.quantity = max(0, item.quantity - 1)
        item.touch()
        if item.quantity == 0 {
            context.insert(ShoppingItem(name: item.name, quantity: 1, unit: item.unit, reason: "Used up"))
        }
        try? context.save()
    }

    /// "I looked at it, it's fine." Food past its best-by gets a 90-day quiet period. Medicine does not.
    private func checked(_ item: SupplyItem) {
        item.lastCheckedAt = Date()
        if case .past = item.status(), item.category.usesSoftDateLanguage {
            item.snoozedUntil = Calendar.current.date(byAdding: .day, value: 90, to: Date())
        }
        item.touch()
        try? context.save()
    }
}
