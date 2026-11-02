import SwiftUI
import SwiftData

struct ReadinessView: View {
    @Environment(\.modelContext) private var context
    @Query private var items: [SupplyItem]
    @Query private var tasks: [MaintenanceTask]
    @Query private var households: [Household]

    var body: some View {
        NavigationStack {
            Group {
                if let household = households.first {
                    content(household: household)
                } else {
                    ProgressView().task { SeedData.ensureDefaults(in: context) }
                }
            }
            .navigationTitle("Ready")
        }
    }

    @ViewBuilder
    private func content(household: Household) -> some View {
        let report = ReadinessCalculator.compute(items: items, tasks: tasks, household: household)
        HouseholdForm(household: household, report: report) { gap in
            context.insert(ShoppingItem(name: gap.shoppingName, quantity: gap.shoppingQuantity, unit: gap.shoppingUnit, reason: "Readiness gap"))
            try? context.save()
        }
    }
}

private struct HouseholdForm: View {
    @Bindable var household: Household
    let report: ReadinessReport
    let addGap: (ReadinessReport.Gap) -> Void
    @State private var addedGapIDs: Set<String> = []

    private let targets = [3, 7, 14, 30]

    var body: some View {
        List {
            Section("Who is covered") {
                Stepper("Adults: \(household.adults)", value: $household.adults, in: 0...20)
                Stepper("Children: \(household.children)", value: $household.children, in: 0...20)
                Stepper("Pets: \(household.pets)", value: $household.pets, in: 0...20)
                Picker("Days to cover", selection: $household.targetDays) {
                    ForEach(targets, id: \.self) { d in Text("\(d) days").tag(d) }
                }
                Text("Red Cross: 3 days if you may need to leave, 2 weeks to stay home. Pick what feels right and work toward it.")
                    .font(.footnote).foregroundStyle(.secondary)
            }

            Section("Where you stand") {
                ReadinessBar(title: "Food", value: ReadinessCalculator.formatDays(report.foodDays), fraction: report.foodFraction,
                             detail: "\(Int(report.foodServings.rounded())) servings on hand, 3 per person per day.")
                ReadinessBar(title: "Water", value: ReadinessCalculator.formatDays(report.waterDays), fraction: report.waterFraction,
                             detail: String(format: "%.0f litres on hand. One gallon per person per day.", report.waterLiters))
                LabeledContent("Medicine & first aid", value: report.medicineCount == 0 ? "Nothing logged" : "\(report.medicineCount) items")
                if report.itemsNeedingAttention > 0 {
                    LabeledContent("Items to use or check", value: "\(report.itemsNeedingAttention)")
                }
                if report.tasksDue > 0 {
                    LabeledContent("Equipment checks due", value: "\(report.tasksDue)")
                }
            }

            Section {
                if report.gaps.isEmpty {
                    Label("You are covered for \(report.targetDays) days of food and water.", systemImage: "checkmark.circle.fill")
                        .foregroundStyle(.green)
                }
                ForEach(report.gaps) { gap in
                    VStack(alignment: .leading, spacing: 6) {
                        Text(gap.title).font(.subheadline.weight(.medium))
                        Text(gap.detail).font(.caption).foregroundStyle(.secondary)
                        Button(addedGapIDs.contains(gap.id) ? "On the shopping list" : "Add to shopping list") {
                            addGap(gap)
                            addedGapIDs.insert(gap.id)
                        }
                        .buttonStyle(.bordered).controlSize(.small)
                        .disabled(addedGapIDs.contains(gap.id))
                    }
                }
            } header: { Text("Gaps") } footer: {
                Text("Food counts servings you entered per unit. Items past their date and used-up items are left out. Sources: Ready.gov, American Red Cross, CDC.")
            }
        }
    }
}

private struct ReadinessBar: View {
    let title: String
    let value: String
    let fraction: Double
    let detail: String

    var body: some View {
        VStack(alignment: .leading, spacing: 6) {
            HStack {
                Text(title).font(.subheadline.weight(.medium))
                Spacer()
                Text(value).font(.subheadline)
            }
            ProgressView(value: fraction)
            Text(detail).font(.caption).foregroundStyle(.secondary)
        }
        .padding(.vertical, 4)
    }
}
