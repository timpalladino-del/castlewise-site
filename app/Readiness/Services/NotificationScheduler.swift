import Foundation
import UserNotifications

/// Local notifications only. Scheduled by iOS, so they fire with no network and no server.
/// Default is one weekly digest. Per-item pings are reserved for medicine, water rotation and equipment.
@MainActor
final class NotificationScheduler {
    static let shared = NotificationScheduler()

    static let digestIdentifier = "weekly-digest"

    func requestAuthorization() async -> Bool {
        let center = UNUserNotificationCenter.current()
        do {
            return try await center.requestAuthorization(options: [.alert, .badge, .sound])
        } catch {
            return false
        }
    }

    func authorizationStatus() async -> UNAuthorizationStatus {
        await UNUserNotificationCenter.current().notificationSettings().authorizationStatus
    }

    /// Rebuilds every scheduled notification from the current data. Call after edits.
    func reschedule(items: [SupplyItem],
                    tasks: [MaintenanceTask],
                    digestWeekday: Int,
                    digestHour: Int,
                    now: Date = Date()) async {
        let center = UNUserNotificationCenter.current()
        center.removeAllPendingNotificationRequests()

        // Weekly digest
        let soon = items.filter { item in
            switch item.status(on: now) {
            case .soon, .past: return true
            default: return false
            }
        }
        let dueTasks = tasks.filter { $0.isDue(on: now) || $0.daysUntilDue(from: now) <= 7 }

        let digest = UNMutableNotificationContent()
        digest.title = "Your week, supplies-wise"
        digest.body = digestBody(itemCount: soon.count, taskCount: dueTasks.count, firstItem: soon.first?.name)
        digest.sound = .default
        var comps = DateComponents()
        comps.weekday = digestWeekday
        comps.hour = digestHour
        comps.minute = 0
        let trigger = UNCalendarNotificationTrigger(dateMatching: comps, repeats: true)
        try? await center.add(UNNotificationRequest(identifier: NotificationScheduler.digestIdentifier, content: digest, trigger: trigger))

        // Per-item: medicine, baby items, and water rotation only.
        for item in items where [.medicine, .baby, .water].contains(item.category) {
            guard let exp = item.expirationDate, exp > now else { continue }
            let leadDays = item.category == .water ? 7 : 14
            guard let fire = Calendar.current.date(byAdding: .day, value: -leadDays, to: exp), fire > now else { continue }
            let content = UNMutableNotificationContent()
            content.title = item.category == .water ? "Time to rotate water" : "\(item.name) is due soon"
            content.body = item.category == .water
                ? "\(item.name) was filled or bought a while ago. Refill or replace it this week."
                : "Expires \(exp.formatted(date: .abbreviated, time: .omitted)). Check it and restock if needed."
            content.sound = .default
            let fireComps = Calendar.current.dateComponents([.year, .month, .day], from: fire)
            var at = fireComps
            at.hour = digestHour
            let t = UNCalendarNotificationTrigger(dateMatching: at, repeats: false)
            try? await center.add(UNNotificationRequest(identifier: "item-\(item.uuid.uuidString)", content: content, trigger: t))
        }

        // Equipment tasks: one notification on the due day.
        for task in tasks where task.isEnabled {
            let due = task.nextDue
            guard due > now else { continue }
            let content = UNMutableNotificationContent()
            content.title = task.equipment?.name ?? "Equipment check"
            content.body = task.title
            content.sound = .default
            var at = Calendar.current.dateComponents([.year, .month, .day], from: due)
            at.hour = digestHour
            let t = UNCalendarNotificationTrigger(dateMatching: at, repeats: false)
            try? await center.add(UNNotificationRequest(identifier: "task-\(task.uuid.uuidString)", content: content, trigger: t))
        }
    }

    func cancelAll() {
        UNUserNotificationCenter.current().removeAllPendingNotificationRequests()
    }

    private func digestBody(itemCount: Int, taskCount: Int, firstItem: String?) -> String {
        var parts: [String] = []
        if itemCount > 0 {
            if let first = firstItem, itemCount == 1 { parts.append("Use \(first) soon.") }
            else { parts.append("\(itemCount) items to use or check.") }
        }
        if taskCount > 0 { parts.append(taskCount == 1 ? "1 equipment task." : "\(taskCount) equipment tasks.") }
        if parts.isEmpty { return "Nothing expiring, nothing due. Enjoy the quiet." }
        return parts.joined(separator: " ")
    }
}
