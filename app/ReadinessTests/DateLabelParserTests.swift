import XCTest
@testable import Readiness

final class DateLabelParserTests: XCTestCase {
    private var calendar: Calendar {
        var c = Calendar(identifier: .gregorian)
        c.timeZone = TimeZone(identifier: "UTC")!
        return c
    }

    private var today: Date {
        calendar.date(from: DateComponents(year: 2026, month: 11, day: 2))!
    }

    private func parser() -> DateLabelParser { DateLabelParser(calendar: calendar, today: today) }

    private func ymd(_ d: Date) -> (Int, Int, Int) {
        let c = calendar.dateComponents([.year, .month, .day], from: d)
        return (c.year!, c.month!, c.day!)
    }

    func testISODate() {
        let d = parser().bestDate(in: "BEST BY 2027-03-15 LOT 4471")
        XCTAssertNotNil(d)
        XCTAssertEqual(ymd(d!), (2027, 3, 15))
    }

    func testUSSlashFullYear() {
        let d = parser().bestDate(in: "EXP 03/15/2027")
        XCTAssertEqual(ymd(d!), (2027, 3, 15))
    }

    func testUSSlashTwoDigitYear() {
        let d = parser().bestDate(in: "BB 3/15/27")
        XCTAssertEqual(ymd(d!), (2027, 3, 15))
    }

    func testDayFirstWhenFirstNumberCannotBeMonth() {
        let d = parser().bestDate(in: "15/03/2027")
        XCTAssertEqual(ymd(d!), (2027, 3, 15))
    }

    func testMonthNameDayFirst() {
        let d = parser().bestDate(in: "BEST IF USED BY 15 MAR 2027")
        XCTAssertEqual(ymd(d!), (2027, 3, 15))
    }

    func testCompactMonthName() {
        let d = parser().bestDate(in: "15MAR27")
        XCTAssertEqual(ymd(d!), (2027, 3, 15))
    }

    func testMonthNameThenDayAndYear() {
        let d = parser().bestDate(in: "Best By: MAR 15, 2027")
        XCTAssertEqual(ymd(d!), (2027, 3, 15))
    }

    func testMonthYearOnlyResolvesToEndOfMonth() {
        let d = parser().bestDate(in: "BEST BY MAR 2027")
        XCTAssertEqual(ymd(d!), (2027, 3, 31))
    }

    func testNumericMonthYear() {
        let d = parser().bestDate(in: "BEST BY 06/2028")
        XCTAssertEqual(ymd(d!), (2028, 6, 30))
    }

    func testFullMonthName() {
        let d = parser().bestDate(in: "Use by September 2027")
        XCTAssertEqual(ymd(d!), (2027, 9, 30))
    }

    func testCompactDigitsYYYYMMDD() {
        let d = parser().bestDate(in: "20270315")
        XCTAssertEqual(ymd(d!), (2027, 3, 15))
    }

    func testRejectsWordsThatStartLikeMonths() {
        let results = parser().parse("MARKET PANTRY 2027 DECK")
        XCTAssertTrue(results.isEmpty, "MARKET should not read as March: \(results)")
    }

    func testIgnoresImplausibleYears() {
        let d = parser().bestDate(in: "LOT 1998-01-01")
        XCTAssertNil(d)
    }

    func testPastDateLowersConfidenceButStillParses() {
        let results = parser().parse("BEST BY 01/2025")
        XCTAssertFalse(results.isEmpty)
        XCTAssertLessThan(results[0].confidence, 0.6)
    }

    func testKeywordBoostsConfidence() {
        let withKeyword = parser().parse("BEST BY 2027-03-15").first!.confidence
        let without = parser().parse("2027-03-15").first!.confidence
        XCTAssertGreaterThan(withKeyword, without)
    }

    func testPicksHighestConfidenceAmongSeveral() {
        let results = parser().parse("LOT 20261102 EXP 2028-06-30")
        XCTAssertEqual(ymd(results.first!.date), (2028, 6, 30))
    }

    func testInvalidDayRejected() {
        XCTAssertNil(parser().bestDate(in: "EXP 02/30/2027"))
    }
}
