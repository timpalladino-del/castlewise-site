import Foundation
import SwiftData

enum AppSchema {
    static let models: [any PersistentModel.Type] = [
        StorageLocation.self,
        SupplyItem.self,
        Equipment.self,
        MaintenanceTask.self,
        MaintenanceLog.self,
        Household.self,
        ProductMemory.self,
        ShoppingItem.self
    ]

    static func container(inMemory: Bool = false) throws -> ModelContainer {
        let schema = Schema(models)
        let config = ModelConfiguration(schema: schema, isStoredInMemoryOnly: inMemory)
        return try ModelContainer(for: schema, configurations: [config])
    }
}
