import Foundation

struct StarterPackItem: Codable, Hashable {
    let name: String
    let category: String
    let quantity: Double
    let unit: String
    let shelfLifeKey: String?
    let servingsPerUnit: Double?
    let litersPerUnit: Double?
}

struct StarterPack: Codable, Identifiable, Hashable {
    let key: String
    let name: String
    let summary: String
    let source: String
    let items: [StarterPackItem]

    var id: String { key }
}

/// One-tap starter lists so nobody faces a blank inventory.
final class StarterPackLibrary {
    static let shared = StarterPackLibrary()

    struct File: Codable {
        let version: Int
        let packs: [StarterPack]
    }

    let packs: [StarterPack]

    init(packs: [StarterPack]) { self.packs = packs }

    convenience init(bundle: Bundle = .main) {
        if let url = bundle.url(forResource: "starter_packs", withExtension: "json"),
           let data = try? Data(contentsOf: url),
           let file = try? JSONDecoder().decode(File.self, from: data) {
            self.init(packs: file.packs)
        } else {
            self.init(packs: [])
        }
    }
}
