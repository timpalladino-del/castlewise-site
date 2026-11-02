import Foundation

/// Finds best-by / expiration dates in OCR text from a package.
/// Pure Foundation so it can be unit tested without UI.
struct DateLabelParser {
    struct Candidate: Equatable {
        let date: Date
        let confidence: Double
        let matchedText: String
        let isMonthYearOnly: Bool
    }

    var calendar: Calendar
    var today: Date
    /// Years in the past beyond which a date is probably a lot number, not an expiration.
    var maxYearsPast: Int = 3
    var maxYearsAhead: Int = 40

    init(calendar: Calendar = .current, today: Date = Date()) {
        self.calendar = calendar
        self.today = today
    }

    private static let keywords = [
        "BEST IF USED BY", "BEST BEFORE", "BEST BY", "USE BY", "USE BEFORE", "SELL BY",
        "EXPIRES", "EXPIRY", "EXPIRATION", "EXP", "BBE", "BB", "BBD", "ENJOY BY", "FRESHEST BY"
    ]

    private static let monthNames: [String: Int] = [
        "JAN": 1, "FEB": 2, "MAR": 3, "APR": 4, "MAY": 5, "JUN": 6,
        "JUL": 7, "AUG": 8, "SEP": 9, "OCT": 10, "NOV": 11, "DEC": 12
    ]

    /// All plausible dates in the text, highest confidence first.
    func parse(_ raw: String) -> [Candidate] {
        let text = raw.uppercased()
            .replacingOccurrences(of: "\u{2010}", with: "-")
            .replacingOccurrences(of: "\u{2011}", with: "-")
            .replacingOccurrences(of: "\u{2013}", with: "-")
        let ns = text as NSString
        var used: [NSRange] = []
        var out: [Candidate] = []

        func take(_ range: NSRange) -> Bool {
            for u in used where NSIntersectionRange(u, range).length > 0 { return false }
            used.append(range)
            return true
        }

        func keywordBoost(before range: NSRange) -> Double {
            let start = max(0, range.location - 24)
            let prefix = ns.substring(with: NSRange(location: start, length: range.location - start))
            for k in DateLabelParser.keywords where prefix.contains(k) { return 0.15 }
            return 0
        }

        func add(_ y: Int, _ m: Int, _ d: Int?, range: NSRange, base: Double) {
            guard let date = makeDate(year: y, month: m, day: d) else { return }
            guard isPlausible(date) else { return }
            guard take(range) else { return }
            var conf = base + keywordBoost(before: range)
            if date < today { conf -= 0.3 }
            out.append(Candidate(date: date, confidence: min(1, max(0, conf)),
                                 matchedText: ns.substring(with: range), isMonthYearOnly: d == nil))
        }

        // 1. ISO: 2027-03-15, 2027/03/15, 2027.03.15
        for m in matches("\\b(20\\d{2})[-/.](\\d{1,2})[-/.](\\d{1,2})\\b", in: text) {
            let y = Int(ns.substring(with: m.range(at: 1)))!, mo = Int(ns.substring(with: m.range(at: 2)))!, d = Int(ns.substring(with: m.range(at: 3)))!
            add(y, mo, d, range: m.range, base: 0.85)
        }

        // 2. Month name forms: 15 MAR 2027, 15MAR27, MAR 15, 2027, MAR 2027, MARCH 2027
        for m in matches("\\b(\\d{1,2})\\s*([A-Z]{3,9})\\.?\\s*,?\\s*(\\d{4}|\\d{2})\\b", in: text) {
            guard let mo = month(from: ns.substring(with: m.range(at: 2))) else { continue }
            let d = Int(ns.substring(with: m.range(at: 1)))!
            let y = year(from: ns.substring(with: m.range(at: 3)))
            add(y, mo, d, range: m.range, base: 0.85)
        }
        for m in matches("\\b([A-Z]{3,9})\\.?\\s*(\\d{1,2}(?!\\d))?\\s*,?\\s*(\\d{4}|\\d{2})\\b", in: text) {
            guard let mo = month(from: ns.substring(with: m.range(at: 1))) else { continue }
            let dayRange = m.range(at: 2)
            let d: Int? = dayRange.location == NSNotFound ? nil : Int(ns.substring(with: dayRange))
            let y = year(from: ns.substring(with: m.range(at: 3)))
            add(y, mo, d, range: m.range, base: d == nil ? 0.65 : 0.85)
        }

        // 3. US numeric: 03/15/2027, 3/15/27, 03-15-2027. Also DD/MM when the first number can't be a month.
        for m in matches("\\b(\\d{1,2})[/.-](\\d{1,2})[/.-](\\d{4}|\\d{2})\\b", in: text) {
            let a = Int(ns.substring(with: m.range(at: 1)))!, b = Int(ns.substring(with: m.range(at: 2)))!
            let y = year(from: ns.substring(with: m.range(at: 3)))
            if a <= 12 { add(y, a, b, range: m.range, base: 0.8) }
            else if b <= 12 { add(y, b, a, range: m.range, base: 0.7) }
        }

        // 4. Month/year numeric: 03/2027, 3/27, 03-27
        for m in matches("\\b(\\d{1,2})[/.-](\\d{4}|\\d{2})\\b", in: text) {
            let mo = Int(ns.substring(with: m.range(at: 1)))!
            let y = year(from: ns.substring(with: m.range(at: 2)))
            add(y, mo, nil, range: m.range, base: 0.55)
        }

        // 5. Compact digits: 20270315 (YYYYMMDD), 03152027 (MMDDYYYY), 031527 (MMDDYY)
        for m in matches("\\b(\\d{8}|\\d{6})\\b", in: text) {
            let s = ns.substring(with: m.range)
            if s.count == 8 {
                if s.hasPrefix("20"), let y = Int(s.prefix(4)), let mo = Int(s.dropFirst(4).prefix(2)), let d = Int(s.suffix(2)) {
                    add(y, mo, d, range: m.range, base: 0.5)
                } else if let mo = Int(s.prefix(2)), let d = Int(s.dropFirst(2).prefix(2)), let y = Int(s.suffix(4)) {
                    add(y, mo, d, range: m.range, base: 0.45)
                }
            } else if let mo = Int(s.prefix(2)), let d = Int(s.dropFirst(2).prefix(2)), let yy = Int(s.suffix(2)) {
                add(2000 + yy, mo, d, range: m.range, base: 0.35)
            }
        }

        return out.sorted { a, b in
            if a.confidence != b.confidence { return a.confidence > b.confidence }
            return a.date < b.date
        }
    }

    /// The single most likely expiration date, or nil.
    func bestDate(in text: String, minimumConfidence: Double = 0.45) -> Date? {
        parse(text).first(where: { $0.confidence >= minimumConfidence })?.date
    }

    // MARK: Helpers

    private func matches(_ pattern: String, in text: String) -> [NSTextCheckingResult] {
        guard let re = try? NSRegularExpression(pattern: pattern) else { return [] }
        return re.matches(in: text, range: NSRange(location: 0, length: (text as NSString).length))
    }

    private func month(from token: String) -> Int? {
        let t = token.trimmingCharacters(in: CharacterSet(charactersIn: ". "))
        guard t.count >= 3 else { return nil }
        let prefix = String(t.prefix(3))
        guard let m = DateLabelParser.monthNames[prefix] else { return nil }
        // Reject words that merely start with a month abbreviation (e.g. "MARKET", "DECK", "SEPARATE").
        let full = ["JANUARY", "FEBRUARY", "MARCH", "APRIL", "MAY", "JUNE", "JULY", "AUGUST", "SEPTEMBER", "SEPT", "OCTOBER", "NOVEMBER", "DECEMBER"]
        if t.count > 3 && !full.contains(t) { return nil }
        return m
    }

    private func year(from token: String) -> Int {
        guard let v = Int(token) else { return 0 }
        return token.count == 2 ? 2000 + v : v
    }

    /// Month-only dates resolve to the last day of that month.
    private func makeDate(year: Int, month: Int, day: Int?) -> Date? {
        guard (1...12).contains(month), year >= 2000, year <= 2100 else { return nil }
        if let day {
            var comps = DateComponents()
            comps.calendar = calendar
            comps.year = year; comps.month = month; comps.day = day
            guard comps.isValidDate(in: calendar) else { return nil }
            return calendar.date(from: comps)
        } else {
            var comps = DateComponents()
            comps.year = year; comps.month = month; comps.day = 1
            guard let first = calendar.date(from: comps),
                  let range = calendar.range(of: .day, in: .month, for: first) else { return nil }
            comps.day = range.count
            return calendar.date(from: comps)
        }
    }

    private func isPlausible(_ date: Date) -> Bool {
        guard let lower = calendar.date(byAdding: .year, value: -maxYearsPast, to: today),
              let upper = calendar.date(byAdding: .year, value: maxYearsAhead, to: today) else { return true }
        return date >= lower && date <= upper
    }
}
