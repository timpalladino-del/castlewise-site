import SwiftUI

/// Calm date language for food; firm language for medicine and baby items.
struct StatusBadge: View {
    let item: SupplyItem
    var now: Date = Date()

    var body: some View {
        let status = item.status(on: now)
        Text(label(for: status))
            .font(.caption.weight(.medium))
            .padding(.horizontal, 8)
            .padding(.vertical, 4)
            .background(color(for: status).opacity(0.15), in: Capsule())
            .foregroundStyle(color(for: status))
    }

    private func label(for status: ExpiryStatus) -> String {
        switch status {
        case .noDate:
            return "No date"
        case .fine:
            if let exp = item.expirationDate {
                return exp.formatted(.dateTime.month(.abbreviated).year())
            }
            return "OK"
        case .soon(let days):
            if days == 0 { return item.category.usesSoftDateLanguage ? "Best by today" : "Expires today" }
            return "\(days)d left"
        case .past(let days):
            if item.category.usesSoftDateLanguage {
                return days >= 365 ? "Past best-by" : "Past best-by \(days)d"
            }
            return "Expired \(days)d ago"
        case .snoozed:
            return "Checked"
        }
    }

    private func color(for status: ExpiryStatus) -> Color {
        switch status {
        case .noDate: return .secondary
        case .fine: return .secondary
        case .soon: return .orange
        case .past: return item.category.usesSoftDateLanguage ? .orange : .red
        case .snoozed: return .green
        }
    }
}

struct ItemRow: View {
    let item: SupplyItem

    var body: some View {
        HStack(spacing: 12) {
            Image(systemName: item.category.symbol)
                .frame(width: 28)
                .foregroundStyle(.secondary)
            VStack(alignment: .leading, spacing: 2) {
                Text(item.name)
                    .font(.body)
                    .strikethrough(item.isOut)
                HStack(spacing: 6) {
                    Text(quantityText)
                    if let loc = item.location {
                        Text("·").foregroundStyle(.tertiary)
                        Text(loc.name)
                    }
                }
                .font(.caption)
                .foregroundStyle(.secondary)
            }
            Spacer()
            StatusBadge(item: item)
        }
        .contentShape(Rectangle())
    }

    private var quantityText: String {
        let q = item.quantity
        let qs = q == q.rounded() ? String(Int(q)) : String(format: "%.1f", q)
        return item.isOut ? "Out" : "\(qs) \(item.unit)"
    }
}
