import Foundation

/// Days-of-supply math against Red Cross and CDC targets. Calm numbers, no scores.
struct ReadinessReport: Equatable {
    struct Gap: Equatable, Identifiable {
        let id: String
        let title: String
        let detail: String
        let shoppingName: String
        let shoppingQuantity: Double
        let shoppingUnit: String
    }

    let targetDays: Int
    let people: Int
    let pets: Int

    let foodServings: Double
    let foodDays: Double
    let waterLiters: Double
    let waterDays: Double

    let medicineCount: Int
    let medicineNeedingAttention: Int
    let itemsNeedingAttention: Int
    let tasksDue: Int

    let gaps: [Gap]

    static let servingsPerPersonPerDay: Double = 3
    static let litersPerPersonPerDay: Double = 3.785   // 1 US gallon, CDC/Ready.gov
    static let litersPerPetPerDay: Double = 1.0

    var foodFraction: Double { targetDays == 0 ? 1 : min(1, foodDays / Double(targetDays)) }
    var waterFraction: Double { targetDays == 0 ? 1 : min(1, waterDays / Double(targetDays)) }
}

enum ReadinessCalculator {
    static func compute(items: [SupplyItem],
                        tasks: [MaintenanceTask],
                        household: Household,
                        now: Date = Date()) -> ReadinessReport {
        let people = household.people
        let pets = max(0, household.pets)
        let target = max(1, household.targetDays)

        // Only count stock that is not past its date and not used up.
        let usable = items.filter { item in
            guard item.quantity > 0 else { return false }
            if case .past = item.status(on: now) { return false }
            return true
        }

        let servings = usable.reduce(0.0) { $0 + $1.totalServings }
        let liters = usable.reduce(0.0) { $0 + $1.totalLiters }

        let servingsPerDay = Double(people) * ReadinessReport.servingsPerPersonPerDay
        let litersPerDay = Double(people) * ReadinessReport.litersPerPersonPerDay + Double(pets) * ReadinessReport.litersPerPetPerDay

        let foodDays = servingsPerDay > 0 ? servings / servingsPerDay : 0
        let waterDays = litersPerDay > 0 ? liters / litersPerDay : 0

        let medicine = items.filter { $0.category == .medicine }
        let medicineAttention = medicine.filter { $0.status(on: now).needsAttention }.count
        let attention = items.filter { $0.status(on: now).needsAttention }.count
        let due = tasks.filter { $0.isDue(on: now) }.count

        var gaps: [ReadinessReport.Gap] = []

        let foodShortServings = servingsPerDay * Double(target) - servings
        if foodShortServings > 0 {
            let cans = Int((foodShortServings / 2.0).rounded(.up))  // about 2 servings per can
            gaps.append(.init(
                id: "food",
                title: "Food: \(formatDays(foodDays)) of \(target) days",
                detail: "About \(cans) more cans, or the equivalent in rice, pasta, and beans.",
                shoppingName: "Shelf-stable food to close the gap",
                shoppingQuantity: Double(cans),
                shoppingUnit: CommonUnit.can.rawValue))
        }

        let waterShortLiters = litersPerDay * Double(target) - liters
        if waterShortLiters > 0 {
            let gallons = Int((waterShortLiters / 3.785).rounded(.up))
            gaps.append(.init(
                id: "water",
                title: "Water: \(formatDays(waterDays)) of \(target) days",
                detail: "About \(gallons) more gallons. One gallon per person per day, plus a little for pets.",
                shoppingName: "Water",
                shoppingQuantity: Double(gallons),
                shoppingUnit: CommonUnit.gal.rawValue))
        }

        if medicine.isEmpty {
            gaps.append(.init(
                id: "medicine",
                title: "No medicine or first aid logged",
                detail: "Add your first aid kit and a 7-day supply of any daily medication.",
                shoppingName: "First aid kit",
                shoppingQuantity: 1,
                shoppingUnit: CommonUnit.kit.rawValue))
        }

        return ReadinessReport(
            targetDays: target, people: people, pets: pets,
            foodServings: servings, foodDays: foodDays,
            waterLiters: liters, waterDays: waterDays,
            medicineCount: medicine.count, medicineNeedingAttention: medicineAttention,
            itemsNeedingAttention: attention, tasksDue: due,
            gaps: gaps)
    }

    static func formatDays(_ days: Double) -> String {
        if days >= 10 { return "\(Int(days.rounded())) days" }
        if days >= 1 { return String(format: "%.1f days", days) }
        if days <= 0 { return "0 days" }
        return "under a day"
    }
}
