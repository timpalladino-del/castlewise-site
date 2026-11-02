import Foundation
import SwiftData

/// One row. Who lives here and how many days you are aiming for.
@Model
final class Household {
    var adults: Int = 2
    var children: Int = 0
    var pets: Int = 0
    /// Days of supply you want on hand. Red Cross: 3 to evacuate, 14 at home.
    var targetDays: Int = 14
    var region: String = ""
    var createdAt: Date = Date()

    init(adults: Int = 2, children: Int = 0, pets: Int = 0, targetDays: Int = 14) {
        self.adults = adults
        self.children = children
        self.pets = pets
        self.targetDays = targetDays
        self.createdAt = Date()
    }

    var people: Int { max(1, adults + children) }
}
