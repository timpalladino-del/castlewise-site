import Foundation

/// UserDefaults keys. All local.
enum AppSettings {
    static let notificationsEnabled = "settings.notificationsEnabled"
    static let digestWeekday = "settings.digestWeekday"   // 1 = Sunday
    static let digestHour = "settings.digestHour"
    static let onlineLookupEnabled = "settings.onlineLookupEnabled"
    static let hasSeenWelcome = "settings.hasSeenWelcome"
}
