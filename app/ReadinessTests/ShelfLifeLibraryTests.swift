import XCTest
@testable import Readiness

final class ShelfLifeLibraryTests: XCTestCase {
    private var library: ShelfLifeLibrary { ShelfLifeLibrary(bundle: Bundle(for: ShelfLifeLibrary.self)) }

    func testBundledTableLoads() {
        XCTAssertGreaterThan(library.entries.count, 50)
    }

    func testKeysAreUnique() {
        let keys = library.entries.map(\.key)
        XCTAssertEqual(keys.count, Set(keys).count)
    }

    func testEveryEntryHasASource() {
        for e in library.entries { XCTAssertFalse(e.source.isEmpty, e.key) }
    }

    func testSearchFindsRice() {
        let results = library.search("white rice")
        XCTAssertEqual(results.first?.key, "rice_white")
    }

    func testBestMatchOnProductName() {
        XCTAssertEqual(library.bestMatch(for: "Bush's Black Beans")?.key, "canned_low_acid")
        XCTAssertEqual(library.bestMatch(for: "Energizer AA Batteries")?.key, "batteries_alkaline")
        XCTAssertEqual(library.bestMatch(for: "Jif Peanut Butter")?.key, "peanut_butter")
    }

    func testBestMatchReturnsNilForNonsense() {
        XCTAssertNil(library.bestMatch(for: "zxqv"))
    }

    func testEstimateUsesDeepStorageWhenFlagged() {
        let rice = library.entry(for: "rice_white")!
        let start = Date(timeIntervalSince1970: 0)
        let sealed = rice.estimate(from: start, opened: false, deepStorage: false)!
        let deep = rice.estimate(from: start, opened: false, deepStorage: true)!
        XCTAssertGreaterThan(deep, sealed)
    }

    func testIndefiniteHasNoEstimate() {
        let honey = library.entry(for: "honey")!
        XCTAssertTrue(honey.isIndefinite)
        XCTAssertNil(honey.estimate(from: Date(), opened: false, deepStorage: false))
    }

    func testMedicineRequiresPrintedDate() {
        XCTAssertTrue(library.entry(for: "otc_medication")!.needsPrintedDate)
        XCTAssertTrue(library.entry(for: "infant_formula")!.needsPrintedDate)
    }

    func testHumanDays() {
        XCTAssertEqual(ShelfLifeEntry.humanDays(730), "2 years")
        XCTAssertEqual(ShelfLifeEntry.humanDays(180), "6 months")
        XCTAssertEqual(ShelfLifeEntry.humanDays(14), "2 weeks")
        XCTAssertEqual(ShelfLifeEntry.humanDays(1), "1 day")
    }

    func testMaintenanceLibraryLoads() {
        let m = MaintenanceLibrary(bundle: Bundle(for: ShelfLifeLibrary.self))
        XCTAssertNotNil(m.kind("generator"))
        XCTAssertEqual(m.kind("smokeAlarm")?.lifetimeYears, 10)
        XCTAssertFalse(m.kind("generator")!.tasks.isEmpty)
    }

    func testStarterPacksReferenceKnownKeys() {
        let packs = StarterPackLibrary(bundle: Bundle(for: ShelfLifeLibrary.self)).packs
        XCTAssertGreaterThanOrEqual(packs.count, 3)
        for p in packs {
            for i in p.items {
                if let k = i.shelfLifeKey { XCTAssertNotNil(library.entry(for: k), "\(p.key) → \(k)") }
            }
        }
    }
}
