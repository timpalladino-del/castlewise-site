import Foundation
import SwiftData

/// The payoff for logging: used-up supplies and readiness gaps land here.
@Model
final class ShoppingItem {
    var uuid: UUID = UUID()
    var name: String = ""
    var quantity: Double = 1
    var unit: String = CommonUnit.each.rawValue
    var reason: String = ""
    var isDone: Bool = false
    var createdAt: Date = Date()

    init(name: String, quantity: Double = 1, unit: String = CommonUnit.each.rawValue, reason: String = "") {
        self.uuid = UUID()
        self.name = name
        self.quantity = quantity
        self.unit = unit
        self.reason = reason
        self.createdAt = Date()
    }
}
