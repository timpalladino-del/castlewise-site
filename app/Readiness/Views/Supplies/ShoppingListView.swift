import SwiftUI
import SwiftData

struct ShoppingListView: View {
    @Environment(\.modelContext) private var context
    @Environment(\.dismiss) private var dismiss
    @Query(sort: \ShoppingItem.createdAt) private var rows: [ShoppingItem]

    @State private var newName = ""
    @State private var buying: ShoppingItem?

    private var open: [ShoppingItem] { rows.filter { !$0.isDone } }
    private var done: [ShoppingItem] { rows.filter { $0.isDone } }

    var body: some View {
        NavigationStack {
            List {
                Section {
                    HStack {
                        TextField("Add something", text: $newName)
                            .onSubmit(add)
                        Button("Add", action: add).disabled(newName.trimmingCharacters(in: .whitespaces).isEmpty)
                    }
                }
                Section("To buy") {
                    if open.isEmpty { Text("Nothing to buy. Used-up supplies and readiness gaps land here.").foregroundStyle(.secondary) }
                    ForEach(open, id: \.uuid) { row in
                        HStack {
                            Button { row.isDone = true; try? context.save() } label: { Image(systemName: "circle") }
                                .buttonStyle(.plain)
                            VStack(alignment: .leading, spacing: 2) {
                                Text(row.name)
                                Text("\(quantityText(row))\(row.reason.isEmpty ? "" : " · \(row.reason)")").font(.caption).foregroundStyle(.secondary)
                            }
                            Spacer()
                            Button("Bought") { buying = row }.buttonStyle(.bordered).controlSize(.small)
                        }
                        .swipeActions {
                            Button(role: .destructive) { context.delete(row); try? context.save() } label: { Label("Delete", systemImage: "trash") }
                        }
                    }
                }
                if !done.isEmpty {
                    Section {
                        ForEach(done, id: \.uuid) { row in
                            HStack {
                                Button { row.isDone = false; try? context.save() } label: { Image(systemName: "checkmark.circle.fill").foregroundStyle(.green) }
                                    .buttonStyle(.plain)
                                Text(row.name).strikethrough().foregroundStyle(.secondary)
                            }
                        }
                        Button("Clear checked items") {
                            for r in done { context.delete(r) }
                            try? context.save()
                        }
                    } header: { Text("Checked") }
                }
            }
            .navigationTitle("Shopping list")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar { ToolbarItem(placement: .confirmationAction) { Button("Done") { dismiss() } } }
            .sheet(item: $buying) { row in
                ItemEditView(draft: draft(for: row)) { _ in
                    context.delete(row)
                    try? context.save()
                }
            }
        }
    }

    private func add() {
        let n = newName.trimmingCharacters(in: .whitespaces)
        guard !n.isEmpty else { return }
        context.insert(ShoppingItem(name: n))
        newName = ""
        try? context.save()
    }

    private func draft(for row: ShoppingItem) -> ItemDraft {
        var d = ItemDraft()
        d.name = row.name
        d.quantity = row.quantity
        d.unit = row.unit
        if let unit = CommonUnit(rawValue: row.unit), unit.litersPerUnit != nil { d.category = .water }
        return d
    }

    private func quantityText(_ row: ShoppingItem) -> String {
        let q = row.quantity
        let qs = q == q.rounded() ? String(Int(q)) : String(format: "%.1f", q)
        return "\(qs) \(row.unit)"
    }
}
