import SwiftUI
import SwiftData

struct EquipmentListView: View {
    @Environment(\.modelContext) private var context
    @Query(sort: \Equipment.name) private var equipment: [Equipment]
    @State private var showAdd = false
    private let library = MaintenanceLibrary.shared

    private var sorted: [Equipment] {
        equipment.sorted { a, b in
            let da = a.nextTask?.daysUntilDue() ?? Int.max
            let db = b.nextTask?.daysUntilDue() ?? Int.max
            if da != db { return da < db }
            return a.name < b.name
        }
    }

    var body: some View {
        NavigationStack {
            Group {
                if equipment.isEmpty {
                    VStack(spacing: 16) {
                        EmptyStateView(symbol: "wrench.and.screwdriver", title: "No equipment yet",
                                       message: "Add the generator, smoke alarms, extinguisher, or power station. Each comes with a sensible check schedule you can edit.")
                        Button { showAdd = true } label: { Label("Add equipment", systemImage: "plus") }
                            .buttonStyle(.borderedProminent)
                    }
                } else {
                    List {
                        ForEach(sorted, id: \.uuid) { e in
                            NavigationLink { EquipmentDetailView(equipment: e) } label: { EquipmentRow(equipment: e) }
                                .swipeActions {
                                    Button(role: .destructive) { context.delete(e); try? context.save() } label: { Label("Delete", systemImage: "trash") }
                                }
                        }
                    }
                }
            }
            .navigationTitle("Equipment")
            .toolbar {
                ToolbarItem(placement: .topBarTrailing) {
                    Button { showAdd = true } label: { Image(systemName: "plus") }
                }
            }
            .sheet(isPresented: $showAdd) { EquipmentEditView() }
        }
    }
}

struct EquipmentRow: View {
    let equipment: Equipment
    private let library = MaintenanceLibrary.shared

    var body: some View {
        HStack(spacing: 12) {
            Image(systemName: library.symbol(for: equipment.kindRaw)).frame(width: 28).foregroundStyle(.secondary)
            VStack(alignment: .leading, spacing: 2) {
                Text(equipment.name)
                HStack(spacing: 6) {
                    if let loc = equipment.location?.name { Text(loc) }
                    if let next = equipment.nextTask {
                        if equipment.location != nil { Text("·").foregroundStyle(.tertiary) }
                        Text(nextText(next))
                    }
                }
                .font(.caption)
                .foregroundStyle(.secondary)
            }
            Spacer()
            if let next = equipment.nextTask {
                let d = next.daysUntilDue()
                Text(d <= 0 ? "Due" : "\(d)d")
                    .font(.caption.weight(.medium))
                    .padding(.horizontal, 8).padding(.vertical, 4)
                    .background((d <= 0 ? Color.orange : Color.secondary).opacity(0.15), in: Capsule())
                    .foregroundStyle(d <= 0 ? Color.orange : Color.secondary)
            }
        }
    }

    private func nextText(_ t: MaintenanceTask) -> String {
        let d = t.daysUntilDue()
        if d < 0 { return "\(t.title) · \(-d)d overdue" }
        if d == 0 { return "\(t.title) · today" }
        return "\(t.title) · in \(d)d"
    }
}
