import Foundation

/// One row of the bundled shelf-life table. Days are quality guidelines unless `requiresPrintedDate`.
struct ShelfLifeEntry: Codable, Identifiable, Hashable {
    let key: String
    let name: String
    let category: String
    let keywords: [String]
    let unopenedDays: Int?
    let openedDays: Int?
    let openedRefrigeratedDays: Int?
    let deepStorageDays: Int?
    let indefinite: Bool?
    let requiresPrintedDate: Bool?
    let note: String?
    let source: String
    let sourceURL: String?

    var id: String { key }
    var isIndefinite: Bool { indefinite ?? false }
    var needsPrintedDate: Bool { requiresPrintedDate ?? false }
    var supplyCategory: SupplyCategory { SupplyCategory.from(category) }

    /// Number of days to use for an estimate, given how the item is kept. Nil when indefinite or unknown.
    func days(opened: Bool, deepStorage: Bool) -> Int? {
        if isIndefinite { return nil }
        if deepStorage, let d = deepStorageDays { return d }
        if opened { return openedDays ?? openedRefrigeratedDays ?? unopenedDays }
        return unopenedDays
    }

    /// Estimated best-by date from a start date (purchase, fill, or opened date).
    func estimate(from start: Date, opened: Bool, deepStorage: Bool, calendar: Calendar = .current) -> Date? {
        guard let d = days(opened: opened, deepStorage: deepStorage) else { return nil }
        return calendar.date(byAdding: .day, value: d, to: start)
    }

    /// Short human summary like "2 years sealed, 1 year opened, 30 years deep storage".
    var summary: String {
        if isIndefinite { return "Keeps indefinitely" }
        var parts: [String] = []
        if let u = unopenedDays { parts.append("\(ShelfLifeEntry.humanDays(u)) sealed") }
        if let o = openedDays { parts.append("\(ShelfLifeEntry.humanDays(o)) opened") }
        if let r = openedRefrigeratedDays { parts.append("\(ShelfLifeEntry.humanDays(r)) opened in the fridge") }
        if let d = deepStorageDays { parts.append("\(ShelfLifeEntry.humanDays(d)) deep storage") }
        if parts.isEmpty { return needsPrintedDate ? "Use the printed date" : "No guideline" }
        return parts.joined(separator: ", ")
    }

    static func humanDays(_ days: Int) -> String {
        if days >= 365 {
            let years = Double(days) / 365.0
            let rounded = years.rounded()
            if abs(years - rounded) < 0.15 {
                return Int(rounded) == 1 ? "1 year" : "\(Int(rounded)) years"
            }
            return String(format: "%.1f years", years)
        }
        if days >= 60 {
            let months = Int((Double(days) / 30.0).rounded())
            return "\(months) months"
        }
        if days >= 14 {
            let weeks = Int((Double(days) / 7.0).rounded())
            return weeks == 1 ? "1 week" : "\(weeks) weeks"
        }
        return days == 1 ? "1 day" : "\(days) days"
    }
}

/// Loads and searches the bundled shelf-life table.
final class ShelfLifeLibrary {
    static let shared = ShelfLifeLibrary()

    struct File: Codable {
        let version: Int
        let note: String?
        let entries: [ShelfLifeEntry]
    }

    let entries: [ShelfLifeEntry]
    private let byKey: [String: ShelfLifeEntry]

    init(entries: [ShelfLifeEntry]) {
        self.entries = entries
        self.byKey = Dictionary(uniqueKeysWithValues: entries.map { ($0.key, $0) })
    }

    convenience init(data: Data) throws {
        let file = try JSONDecoder().decode(File.self, from: data)
        self.init(entries: file.entries)
    }

    convenience init(bundle: Bundle = .main) {
        if let url = bundle.url(forResource: "shelf_life", withExtension: "json"),
           let data = try? Data(contentsOf: url),
           let file = try? JSONDecoder().decode(File.self, from: data) {
            self.init(entries: file.entries)
        } else {
            self.init(entries: [])
        }
    }

    func entry(for key: String?) -> ShelfLifeEntry? {
        guard let key else { return nil }
        return byKey[key]
    }

    /// Ranked search over names and keywords. Empty query returns everything, grouped by name.
    func search(_ query: String, category: SupplyCategory? = nil) -> [ShelfLifeEntry] {
        let q = query.trimmingCharacters(in: .whitespacesAndNewlines).lowercased()
        let pool = category.map { c in entries.filter { $0.supplyCategory == c } } ?? entries
        if q.isEmpty { return pool.sorted { $0.name < $1.name } }
        let scored: [(ShelfLifeEntry, Int)] = pool.compactMap { e in
            let s = ShelfLifeLibrary.score(e, against: q)
            return s > 0 ? (e, s) : nil
        }
        return scored.sorted { a, b in a.1 != b.1 ? a.1 > b.1 : a.0.name < b.0.name }.map { $0.0 }
    }

    /// Best guess for a free-text product name, or nil when nothing matches.
    func bestMatch(for name: String, category: SupplyCategory? = nil) -> ShelfLifeEntry? {
        let results = search(name, category: category)
        guard let top = results.first else { return nil }
        return ShelfLifeLibrary.score(top, against: name.lowercased()) >= 3 ? top : nil
    }

    private static func score(_ e: ShelfLifeEntry, against q: String) -> Int {
        let name = e.name.lowercased()
        if name == q { return 10 }
        var best = 0
        for kw in e.keywords {
            let k = kw.lowercased()
            if k == q { best = max(best, 9); continue }
            if q.contains(k) && k.count >= 3 { best = max(best, 5 + min(k.count, 4)) }
            if k.contains(q) && q.count >= 3 { best = max(best, 4) }
        }
        if name.contains(q) && q.count >= 3 { best = max(best, 4) }
        // token overlap
        let qTokens = Set(q.split(separator: " ").map(String.init))
        let nTokens = Set(name.split(separator: " ").map(String.init))
        let overlap = qTokens.intersection(nTokens).filter { $0.count >= 3 }.count
        if overlap > 0 { best = max(best, 2 + overlap) }
        return best
    }
}
