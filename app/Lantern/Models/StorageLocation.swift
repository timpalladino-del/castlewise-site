import Foundation
import SwiftData

/// A physical place supplies live: pantry, basement shelf, car, go-bag.
/// "If you can't walk to an item from its record, the inventory isn't useful."
@Model
final class StorageLocation {
    var uuid: UUID = UUID()
    var name: String = ""
    var symbol: String = "shippingbox.fill"
    var sortOrder: Int = 0
    var createdAt: Date = Date()

    @Relationship(deleteRule: .nullify, inverse: \SupplyItem.location)
    var items: [SupplyItem] = []

    @Relationship(deleteRule: .nullify, inverse: \Equipment.location)
    var equipment: [Equipment] = []

    init(name: String, symbol: String = "shippingbox.fill", sortOrder: Int = 0) {
        self.uuid = UUID()
        self.name = name
        self.symbol = symbol
        self.sortOrder = sortOrder
        self.createdAt = Date()
    }

    static let defaults: [(String, String)] = [
        ("Kitchen pantry", "cabinet.fill"),
        ("Basement or garage shelf", "shippingbox.fill"),
        ("Go-bag", "backpack.fill"),
        ("Car", "car.fill"),
        ("Medicine cabinet", "cross.case.fill")
    ]
}
