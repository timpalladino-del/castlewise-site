import Foundation
import SwiftData

/// Gear that needs periodic attention: generator, extinguisher, alarms, power station.
@Model
final class Equipment {
    var uuid: UUID = UUID()
    var name: String = ""
    var kindRaw: String = "other"
    var brand: String = ""
    var model: String = ""
    var serial: String = ""
    var purchaseDate: Date?
    /// Manufacture date drives "replace after N years" items like alarms and extinguishers.
    var manufactureDate: Date?
    var fuelType: String = ""
    var batteryType: String = ""
    var batteryCount: Int = 0
    var notes: String = ""

    @Attribute(.externalStorage)
    var photoData: Data?

    var location: StorageLocation?

    @Relationship(deleteRule: .cascade, inverse: \MaintenanceTask.equipment)
    var tasks: [MaintenanceTask] = []

    @Relationship(deleteRule: .cascade, inverse: \MaintenanceLog.equipment)
    var logs: [MaintenanceLog] = []

    var createdAt: Date = Date()

    init(name: String, kind: String = "other", location: StorageLocation? = nil) {
        self.uuid = UUID()
        self.name = name
        self.kindRaw = kind
        self.location = location
        self.createdAt = Date()
    }

    /// The task due soonest, if any.
    var nextTask: MaintenanceTask? {
        tasks.min { $0.nextDue < $1.nextDue }
    }

    /// End-of-life date when the kind has a lifetime (alarms, extinguishers, cylinders).
    func replaceBy(lifetimeYears: Int?, calendar: Calendar = .current) -> Date? {
        guard let years = lifetimeYears, let base = manufactureDate ?? purchaseDate else { return nil }
        return calendar.date(byAdding: .year, value: years, to: base)
    }

    var sortedLogs: [MaintenanceLog] {
        logs.sorted { $0.date > $1.date }
    }
}

/// A recurring chore on a piece of equipment. Due date = last done (or anchor) + interval.
@Model
final class MaintenanceTask {
    var uuid: UUID = UUID()
    var title: String = ""
    var intervalDays: Int = 30
    var anchorDate: Date = Date()
    var lastDoneAt: Date?
    var note: String = ""
    var source: String = ""
    var isEnabled: Bool = true

    var equipment: Equipment?

    init(title: String, intervalDays: Int, note: String = "", source: String = "", anchorDate: Date = Date()) {
        self.uuid = UUID()
        self.title = title
        self.intervalDays = intervalDays
        self.note = note
        self.source = source
        self.anchorDate = anchorDate
    }

    var nextDue: Date {
        let base = lastDoneAt ?? anchorDate
        return Calendar.current.date(byAdding: .day, value: intervalDays, to: base) ?? base
    }

    func daysUntilDue(from now: Date = Date(), calendar: Calendar = .current) -> Int {
        let start = calendar.startOfDay(for: now)
        let end = calendar.startOfDay(for: nextDue)
        return calendar.dateComponents([.day], from: start, to: end).day ?? 0
    }

    func isDue(on now: Date = Date()) -> Bool {
        isEnabled && daysUntilDue(from: now) <= 0
    }
}

/// "Ran generator 20 min, changed oil." The notes history.
@Model
final class MaintenanceLog {
    var uuid: UUID = UUID()
    var date: Date = Date()
    var taskTitle: String = ""
    var note: String = ""

    var equipment: Equipment?

    init(date: Date = Date(), taskTitle: String, note: String = "") {
        self.uuid = UUID()
        self.date = date
        self.taskTitle = taskTitle
        self.note = note
    }
}
