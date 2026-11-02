import XCTest
import SwiftData
@testable import Lantern

@MainActor
final class ReadinessCalculatorTests: XCTestCase {
    private var container: ModelContainer!
    private var context: ModelContext!

    override func setUpWithError() throws {
        container = try AppSchema.container(inMemory: true)
        context = ModelContext(container)
    }

    private func food(_ name: String, qty: Double, servings: Double, expires: Date? = nil) -> SupplyItem {
        let i = SupplyItem(name: name, category: .food, quantity: qty, unit: "can")
        i.servingsPerUnit = servings
        i.expirationDate = expires
        context.insert(i)
        return i
    }

    private func water(_ name: String, gallons: Double) -> SupplyItem {
        let i = SupplyItem(name: name, category: .water, quantity: gallons, unit: "gal")
        context.insert(i)
        return i
    }

    func testDaysOfSupplyForTwoPeople() {
        let household = Household(adults: 2, children: 0, pets: 0, targetDays: 14)
        _ = food("Beans", qty: 12, servings: 2)          // 24 servings → 4 days for 2 people
        _ = water("Water", gallons: 8)                   // 8 gal → 4 days for 2 people
        let r = ReadinessCalculator.compute(items: fetch(), tasks: [], household: household)
        XCTAssertEqual(r.foodDays, 4, accuracy: 0.01)
        XCTAssertEqual(r.waterDays, 4, accuracy: 0.01)
        XCTAssertEqual(r.gaps.map(\.id), ["food", "water", "medicine"])
    }

    func testExpiredAndEmptyStockIsExcluded() {
        let household = Household(adults: 1, targetDays: 3)
        let past = Calendar.current.date(byAdding: .day, value: -10, to: Date())
        _ = food("Old soup", qty: 10, servings: 2, expires: past)
        _ = food("Empty", qty: 0, servings: 2)
        let r = ReadinessCalculator.compute(items: fetch(), tasks: [], household: household)
        XCTAssertEqual(r.foodServings, 0)
    }

    func testNoGapsWhenCovered() {
        let household = Household(adults: 1, children: 0, pets: 0, targetDays: 3)
        _ = food("Rice", qty: 1, servings: 10)
        _ = water("Water", gallons: 3)
        let med = SupplyItem(name: "First aid kit", category: .medicine)
        context.insert(med)
        let r = ReadinessCalculator.compute(items: fetch(), tasks: [], household: household)
        XCTAssertTrue(r.gaps.isEmpty, "\(r.gaps)")
    }

    func testPetsAddWater() {
        let noPets = ReadinessCalculator.compute(items: [water("W", gallons: 10)], tasks: [], household: Household(adults: 1, pets: 0))
        let pets = ReadinessCalculator.compute(items: [water("W2", gallons: 10)], tasks: [], household: Household(adults: 1, pets: 2))
        XCTAssertGreaterThan(noPets.waterDays, pets.waterDays)
    }

    func testWaterUnitConversion() {
        let i = SupplyItem(name: "Bottles", category: .water, quantity: 2, unit: "L")
        XCTAssertEqual(i.totalLiters, 2, accuracy: 0.001)
        let g = SupplyItem(name: "Jug", category: .water, quantity: 1, unit: "gal")
        XCTAssertEqual(g.totalLiters, 3.785, accuracy: 0.001)
    }

    func testStatusLanguageThresholds() {
        let soon = SupplyItem(name: "Soup", category: .food)
        soon.expirationDate = Calendar.current.date(byAdding: .day, value: 10, to: Date())
        if case .soon(let d) = soon.status() { XCTAssertEqual(d, 10) } else { XCTFail() }

        let deep = SupplyItem(name: "Rice", category: .food)
        deep.isDeepStorage = true
        deep.expirationDate = Calendar.current.date(byAdding: .day, value: 100, to: Date())
        if case .soon = deep.status() {} else { XCTFail("deep storage should warn 180 days out") }

        let past = SupplyItem(name: "Beans", category: .food)
        past.expirationDate = Calendar.current.date(byAdding: .day, value: -3, to: Date())
        if case .past(let d) = past.status() { XCTAssertEqual(d, 3) } else { XCTFail() }
    }

    func testMaintenanceDueMath() {
        let anchor = Calendar.current.date(byAdding: .day, value: -40, to: Date())!
        let t = MaintenanceTask(title: "Run", intervalDays: 30, anchorDate: anchor)
        XCTAssertTrue(t.isDue())
        XCTAssertEqual(t.daysUntilDue(), -10)
        t.lastDoneAt = Date()
        XCTAssertFalse(t.isDue())
        XCTAssertEqual(t.daysUntilDue(), 30)
    }

    private func fetch() -> [SupplyItem] {
        (try? context.fetch(FetchDescriptor<SupplyItem>())) ?? []
    }
}
