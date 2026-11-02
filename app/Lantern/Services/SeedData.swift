import Foundation
import SwiftData

/// First-launch defaults: locations and a household row. Nothing else; the user's data is theirs.
enum SeedData {
    @MainActor
    static func ensureDefaults(in context: ModelContext) {
        let locations = (try? context.fetch(FetchDescriptor<StorageLocation>())) ?? []
        if locations.isEmpty {
            for (index, pair) in StorageLocation.defaults.enumerated() {
                context.insert(StorageLocation(name: pair.0, symbol: pair.1, sortOrder: index))
            }
        }
        let households = (try? context.fetch(FetchDescriptor<Household>())) ?? []
        if households.isEmpty {
            context.insert(Household())
        }
        try? context.save()
    }

    /// Adds a starter pack's lines to a location, estimating dates from today.
    @MainActor
    static func apply(pack: StarterPack, to location: StorageLocation?, in context: ModelContext, library: ShelfLifeLibrary = .shared) {
        let today = Date()
        for line in pack.items {
            let item = SupplyItem(name: line.name, category: SupplyCategory.from(line.category), quantity: line.quantity, unit: line.unit, location: location)
            item.purchaseDate = today
            item.servingsPerUnit = line.servingsPerUnit
            item.litersPerUnit = line.litersPerUnit
            if let key = line.shelfLifeKey, let entry = library.entry(for: key) {
                item.shelfLifeKey = key
                if !entry.needsPrintedDate, let est = entry.estimate(from: today, opened: false, deepStorage: false) {
                    item.expirationDate = est
                    item.dateSource = .estimated
                }
            }
            context.insert(item)
        }
        try? context.save()
    }
}
