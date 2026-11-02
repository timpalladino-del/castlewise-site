import Foundation

/// Top-level grouping for supplies. Stored as a raw string so the schema stays stable.
enum SupplyCategory: String, CaseIterable, Codable, Identifiable {
    case food, water, medicine, hygiene, power, tools, pet, baby, documents, cash, other

    var id: String { rawValue }

    var title: String {
        switch self {
        case .food: return "Food"
        case .water: return "Water"
        case .medicine: return "Medicine & first aid"
        case .hygiene: return "Hygiene"
        case .power: return "Power & light"
        case .tools: return "Tools & gear"
        case .pet: return "Pets"
        case .baby: return "Baby"
        case .documents: return "Documents"
        case .cash: return "Cash"
        case .other: return "Other"
        }
    }

    var symbol: String {
        switch self {
        case .food: return "fork.knife"
        case .water: return "drop.fill"
        case .medicine: return "cross.case.fill"
        case .hygiene: return "hands.sparkles.fill"
        case .power: return "bolt.fill"
        case .tools: return "wrench.and.screwdriver.fill"
        case .pet: return "pawprint.fill"
        case .baby: return "figure.and.child.holdinghands"
        case .documents: return "doc.text.fill"
        case .cash: return "banknote.fill"
        case .other: return "shippingbox.fill"
        }
    }

    /// Categories where "best by" is quality, not safety. Medicine and baby items are never softened.
    var usesSoftDateLanguage: Bool {
        switch self {
        case .food, .water, .hygiene, .power, .tools, .pet, .documents, .cash, .other: return true
        case .medicine, .baby: return false
        }
    }

    static func from(_ raw: String) -> SupplyCategory {
        SupplyCategory(rawValue: raw) ?? .other
    }
}

/// Where an item's expiration date came from. Drives how confidently the UI talks about it.
enum DateSource: String, Codable, CaseIterable {
    case printed      // read off the package (scanned or typed)
    case estimated    // from the shelf-life library and a purchase or fill date
    case manual       // user picked a date without saying where it came from
    case none         // no date on file

    var label: String {
        switch self {
        case .printed: return "Printed on package"
        case .estimated: return "Estimated from shelf life"
        case .manual: return "Entered by hand"
        case .none: return "No date"
        }
    }
}

/// Units the quick-add UI offers. Free text is still allowed.
enum CommonUnit: String, CaseIterable, Identifiable {
    case each, can, jar, box, bag, bottle, carton, pouch, kit, set, lb, oz, gal, L, bar, pack

    var id: String { rawValue }

    /// Litres per unit when the unit is a volume. Used for water math.
    var litersPerUnit: Double? {
        switch self {
        case .gal: return 3.785
        case .L: return 1.0
        case .oz: return 0.0296
        default: return nil
        }
    }
}
