import SwiftUI
import SwiftData
import UIKit

struct EquipmentDetailView: View {
    @Environment(\.modelContext) private var context
    @Environment(\.dismiss) private var dismiss
    @Bindable var equipment: Equipment

    @State private var showEdit = false
    @State private var showAddTask = false
    @State private var showAddLog = false
    @State private var newTaskTitle = ""
    @State private var newTaskInterval = 30
    @State private var newLogNote = ""
    @State private var confirmDelete = false

    private let library = MaintenanceLibrary.shared

    private var sortedTasks: [MaintenanceTask] {
        equipment.tasks.sorted { $0.nextDue < $1.nextDue }
    }

    var body: some View {
        List {
            Section {
                HStack(spacing: 12) {
                    Image(systemName: library.symbol(for: equipment.kindRaw)).font(.title2).foregroundStyle(.secondary)
                    VStack(alignment: .leading, spacing: 3) {
                        Text(equipment.name).font(.title3.weight(.semibold))
                        Text([equipment.brand, equipment.model].filter { !$0.isEmpty }.joined(separator: " ")).foregroundStyle(.secondary)
                    }
                }
                if let data = equipment.photoData, let img = UIImage(data: data) {
                    Image(uiImage: img).resizable().scaledToFit().frame(maxHeight: 200).clipShape(RoundedRectangle(cornerRadius: 12))
                }
                if let loc = equipment.location { LabeledContent("Location", value: loc.name) }
                if !equipment.serial.isEmpty { LabeledContent("Serial", value: equipment.serial) }
                if !equipment.fuelType.isEmpty { LabeledContent("Fuel", value: equipment.fuelType) }
                if !equipment.batteryType.isEmpty { LabeledContent("Batteries", value: "\(equipment.batteryCount) × \(equipment.batteryType)") }
                if let replace = equipment.replaceBy(lifetimeYears: library.kind(equipment.kindRaw)?.lifetimeYears) {
                    LabeledContent("Replace by", value: replace.formatted(date: .abbreviated, time: .omitted))
                        .foregroundStyle(replace < Date() ? .orange : .primary)
                }
            }

            Section {
                if sortedTasks.isEmpty {
                    Text("No checks scheduled.").foregroundStyle(.secondary)
                }
                ForEach(sortedTasks, id: \.uuid) { task in
                    VStack(alignment: .leading, spacing: 4) {
                        HStack {
                            VStack(alignment: .leading, spacing: 2) {
                                Text(task.title)
                                Text(dueText(task)).font(.caption).foregroundStyle(task.isDue() ? .orange : .secondary)
                            }
                            Spacer()
                            Button("Done") { markDone(task) }.buttonStyle(.bordered).controlSize(.small)
                        }
                        if !task.note.isEmpty { Text(task.note).font(.caption).foregroundStyle(.secondary) }
                        if !task.source.isEmpty { Text("Source: \(task.source)").font(.caption2).foregroundStyle(.tertiary) }
                    }
                    .swipeActions {
                        Button(role: .destructive) { context.delete(task); try? context.save() } label: { Label("Delete", systemImage: "trash") }
                    }
                }
                Button { showAddTask = true } label: { Label("Add a check", systemImage: "plus") }
            } header: { Text("Checks") }

            Section {
                if equipment.logs.isEmpty { Text("Nothing logged yet.").foregroundStyle(.secondary) }
                ForEach(equipment.sortedLogs, id: \.uuid) { log in
                    VStack(alignment: .leading, spacing: 2) {
                        HStack {
                            Text(log.taskTitle.isEmpty ? "Note" : log.taskTitle).font(.subheadline)
                            Spacer()
                            Text(log.date.formatted(date: .abbreviated, time: .omitted)).font(.caption).foregroundStyle(.secondary)
                        }
                        if !log.note.isEmpty { Text(log.note).font(.caption).foregroundStyle(.secondary) }
                    }
                    .swipeActions {
                        Button(role: .destructive) { context.delete(log); try? context.save() } label: { Label("Delete", systemImage: "trash") }
                    }
                }
                Button { showAddLog = true } label: { Label("Add a note", systemImage: "square.and.pencil") }
            } header: { Text("History") }

            if !equipment.notes.isEmpty {
                Section("Notes") { Text(equipment.notes) }
            }

            Section { Button("Delete equipment", role: .destructive) { confirmDelete = true } }
        }
        .navigationTitle("")
        .navigationBarTitleDisplayMode(.inline)
        .toolbar { ToolbarItem(placement: .topBarTrailing) { Button("Edit") { showEdit = true } } }
        .sheet(isPresented: $showEdit) { EquipmentEditView(equipment: equipment) }
        .alert("New check", isPresented: $showAddTask) {
            TextField("What to do", text: $newTaskTitle)
            TextField("Every how many days", value: $newTaskInterval, format: .number)
            Button("Add") {
                let t = MaintenanceTask(title: newTaskTitle, intervalDays: max(1, newTaskInterval))
                t.equipment = equipment
                context.insert(t)
                try? context.save()
                newTaskTitle = ""
                newTaskInterval = 30
            }
            Button("Cancel", role: .cancel) {}
        }
        .alert("Add a note", isPresented: $showAddLog) {
            TextField("Ran 20 min, changed oil…", text: $newLogNote)
            Button("Save") {
                let log = MaintenanceLog(taskTitle: "", note: newLogNote)
                log.equipment = equipment
                context.insert(log)
                try? context.save()
                newLogNote = ""
            }
            Button("Cancel", role: .cancel) {}
        }
        .confirmationDialog("Delete \(equipment.name)?", isPresented: $confirmDelete, titleVisibility: .visible) {
            Button("Delete", role: .destructive) {
                context.delete(equipment)
                try? context.save()
                dismiss()
            }
        }
    }

    private func dueText(_ t: MaintenanceTask) -> String {
        let d = t.daysUntilDue()
        let last = t.lastDoneAt.map { " · last \($0.formatted(date: .abbreviated, time: .omitted))" } ?? ""
        if d < 0 { return "\(-d) days overdue\(last)" }
        if d == 0 { return "Due today\(last)" }
        return "Due \(t.nextDue.formatted(date: .abbreviated, time: .omitted))\(last)"
    }

    private func markDone(_ task: MaintenanceTask) {
        task.lastDoneAt = Date()
        let log = MaintenanceLog(taskTitle: task.title)
        log.equipment = equipment
        context.insert(log)
        try? context.save()
    }
}
