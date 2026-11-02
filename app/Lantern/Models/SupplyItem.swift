import Foundation
import SwiftData

/// One line of inventory: "12 cans of black beans in the basement, best by March 2028."
@Model
final class SupplyItem {
    var uuid: UUID = UUID()
    var name: String = ""
    var brand: String = ""
    var categoryRaw: String = SupplyCategory.food.rawValue
    var quantity: Double = 1
    var unit: String = CommonUnit.each.rawValue
    var barcode: String?

    var purchaseDate: Date?
    var expirationDate: Date?
    var dateSourceRaw: String = DateSource.none.rawValue

    var isOpened: Bool = false
    var openedDate: Date?
    var isDeepStorage: Bool = false

    /// For water containers you fill yourself. Rotation is counted from here.
    var fillDate: Date?

    /// Key into the shelf-life library, when the item was matched to one.
    var shelfLifeKey: String?

    /// Readiness math. Food: servings per unit. Water: litres per unit.
    var servingsPerUnit: Double?
    var litersPerUnit: Double?

    var notes: String = ""
    var lastCheckedAt: Date?
    var snoozedUntil: Date?

    @Attribute(.externalStorage)
    var photoData: Data?

    var location: StorageLocation?

    var createdAt: Date = Date()
    var updatedAt: Date = Date()

    init(name: String,
         category: SupplyCategory = .food,
         quantity: Double = 1,
         unit: String = CommonUnit.each.rawValue,
         location: StorageLocation? = nil) {
        self.uuid = UUID()
        self.name = name
        self.categoryRaw = category.rawValue
        self.quantity = quantity
        self.unit = unit
        self.location = location
        self.createdAt = Date()
        self.updatedAt = Date()
    }

    // MARK: Derived

    var category: SupplyCategory {
        get { SupplyCategory.from(categoryRaw) }
        set { categoryRaw = newValue.rawValue }
    }

    var dateSource: DateSource {
        get { DateSource(rawValue: dateSourceRaw) ?? .none }
        set { dateSourceRaw = newValue.rawValue }
    }

    var isOut: Bool { quantity <= 0 }

    /// Whole days until the expiration date. Negative when past. Nil when no date.
    func daysUntilExpiration(from now: Date = Date(), calendar: Calendar = .current) -> Int? {
        guard let exp = expirationDate else { return nil }
        let start = calendar.startOfDay(for: now)
        let end = calendar.startOfDay(for: exp)
        return calendar.dateComponents([.day], from: start, to: end).day
    }

    func status(on now: Date = Date(), calendar: Calendar = .current) -> ExpiryStatus {
        if let snooze = snoozedUntil, snooze > now { return .snoozed(until: snooze) }
        guard let days = daysUntilExpiration(from: now, calendar: calendar) else { return .noDate }
        if days < 0 { return .past(days: -days) }
        if days <= soonThresholdDays { return .soon(days: days) }
        return .fine(days: days)
    }

    /// How far ahead "soon" starts for this item. Deep storage gets a long runway.
    var soonThresholdDays: Int {
        if isDeepStorage { return 180 }
        switch category {
        case .medicine, .baby: return 60
        case .water: return 30
        default: return 30
        }
    }

    /// Readiness helpers
    var totalServings: Double {
        guard category == .food else { return 0 }
        return max(0, quantity) * (servingsPerUnit ?? 1)
    }

    var totalLiters: Double {
        guard category == .water else { return 0 }
        let perUnit = litersPerUnit ?? CommonUnit(rawValue: unit)?.litersPerUnit ?? 0
        return max(0, quantity) * perUnit
    }

    func touch() { updatedAt = Date() }
}

/// What the UI says about an item's date. Language stays calm for food; firm for medicine.
enum ExpiryStatus: Equatable {
    case noDate
    case fine(days: Int)
    case soon(days: Int)
    case past(days: Int)
    case snoozed(until: Date)

    var sortRank: Int {
        switch self {
        case .past: return 0
        case .soon: return 1
        case .fine: return 2
        case .snoozed: return 3
        case .noDate: return 4
        }
    }

    var needsAttention: Bool {
        switch self {
        case .past, .soon: return true
        default: return false
        }
    }
}
