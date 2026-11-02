import Foundation

struct MaintenanceTaskTemplate: Codable, Hashable {
    let title: String
    let intervalDays: Int
    let note: String?
    let source: String?
}

struct EquipmentKind: Codable, Identifiable, Hashable {
    let kind: String
    let name: String
    let symbol: String
    let lifetimeYears: Int?
    let tasks: [MaintenanceTaskTemplate]

    var id: String { kind }
}

/// Default maintenance schedules per kind of equipment, from NFPA, CPSC and manufacturer guidance.
final class MaintenanceLibrary {
    static let shared = MaintenanceLibrary()

    struct File: Codable {
        let version: Int
        let kinds: [EquipmentKind]
    }

    let kinds: [EquipmentKind]
    private let byKind: [String: EquipmentKind]

    init(kinds: [EquipmentKind]) {
        self.kinds = kinds
        self.byKind = Dictionary(uniqueKeysWithValues: kinds.map { ($0.kind, $0) })
    }

    convenience init(data: Data) throws {
        let file = try JSONDecoder().decode(File.self, from: data)
        self.init(kinds: file.kinds)
    }

    convenience init(bundle: Bundle = .main) {
        if let url = bundle.url(forResource: "maintenance_intervals", withExtension: "json"),
           let data = try? Data(contentsOf: url),
           let file = try? JSONDecoder().decode(File.self, from: data) {
            self.init(kinds: file.kinds)
        } else {
            self.init(kinds: [])
        }
    }

    func kind(_ raw: String) -> EquipmentKind? { byKind[raw] }

    func symbol(for raw: String) -> String { byKind[raw]?.symbol ?? "shippingbox.fill" }
    func displayName(for raw: String) -> String { byKind[raw]?.name ?? "Equipment" }
}
