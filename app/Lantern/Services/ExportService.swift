import Foundation

/// CSV export. Free, always. The binder people need this.
enum ExportService {
    static func suppliesCSV(_ items: [SupplyItem]) -> String {
        var rows: [String] = ["name,brand,category,quantity,unit,location,purchase_date,expiration_date,date_source,opened,deep_storage,fill_date,barcode,notes"]
        let df = ISO8601DateFormatter()
        df.formatOptions = [.withFullDate]
        for i in items.sorted(by: { ($0.location?.name ?? "") + $0.name < ($1.location?.name ?? "") + $1.name }) {
            rows.append([
                i.name, i.brand, i.category.rawValue, trim(i.quantity), i.unit, i.location?.name ?? "",
                i.purchaseDate.map { df.string(from: $0) } ?? "", i.expirationDate.map { df.string(from: $0) } ?? "", i.dateSource.rawValue,
                i.isOpened ? "yes" : "no", i.isDeepStorage ? "yes" : "no", i.fillDate.map { df.string(from: $0) } ?? "",
                i.barcode ?? "", i.notes
            ].map(escape).joined(separator: ","))
        }
        return rows.joined(separator: "\n")
    }

    static func equipmentCSV(_ equipment: [Equipment]) -> String {
        var rows: [String] = ["name,kind,brand,model,serial,location,purchase_date,manufacture_date,fuel,battery_type,battery_count,next_task,next_due,notes"]
        let df = ISO8601DateFormatter()
        df.formatOptions = [.withFullDate]
        for e in equipment.sorted(by: { $0.name < $1.name }) {
            let next = e.nextTask
            rows.append([
                e.name, e.kindRaw, e.brand, e.model, e.serial, e.location?.name ?? "",
                e.purchaseDate.map { df.string(from: $0) } ?? "", e.manufactureDate.map { df.string(from: $0) } ?? "",
                e.fuelType, e.batteryType, String(e.batteryCount),
                next?.title ?? "", next.map { df.string(from: $0.nextDue) } ?? "", e.notes
            ].map(escape).joined(separator: ","))
        }
        return rows.joined(separator: "\n")
    }

    /// Writes text to a temp file and returns its URL for ShareLink.
    static func writeTemp(_ text: String, filename: String) throws -> URL {
        let url = FileManager.default.temporaryDirectory.appendingPathComponent(filename)
        try text.data(using: .utf8)?.write(to: url, options: .atomic)
        return url
    }

    private static func escape(_ s: String) -> String {
        if s.contains(",") || s.contains("\"") || s.contains("\n") {
            return "\"" + s.replacingOccurrences(of: "\"", with: "\"\"") + "\""
        }
        return s
    }

    private static func trim(_ d: Double) -> String {
        d == d.rounded() ? String(Int(d)) : String(d)
    }
}
