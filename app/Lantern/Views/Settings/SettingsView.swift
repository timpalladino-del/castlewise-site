import SwiftUI
import SwiftData

struct SettingsView: View {
    @Environment(\.modelContext) private var context
    @Query private var items: [SupplyItem]
    @Query private var tasks: [MaintenanceTask]
    @Query private var equipment: [Equipment]

    @AppStorage(AppSettings.notificationsEnabled) private var notificationsEnabled = false
    @AppStorage(AppSettings.digestWeekday) private var digestWeekday = 1
    @AppStorage(AppSettings.digestHour) private var digestHour = 9
    @AppStorage(AppSettings.onlineLookupEnabled) private var onlineLookup = true

    @State private var exportURL: URL?
    @State private var exportError: String?
    @State private var authDenied = false

    private let weekdays = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]

    var body: some View {
        NavigationStack {
            Form {
                Section {
                    Toggle("Weekly digest and due-date reminders", isOn: $notificationsEnabled)
                        .onChange(of: notificationsEnabled) { _, on in
                            Task { await toggleNotifications(on) }
                        }
                    if notificationsEnabled {
                        Picker("Digest day", selection: $digestWeekday) {
                            ForEach(1...7, id: \.self) { Text(weekdays[$0 - 1]).tag($0) }
                        }
                        Picker("Time", selection: $digestHour) {
                            ForEach(6...21, id: \.self) { h in Text(hourLabel(h)).tag(h) }
                        }
                        Button("Refresh reminders now") { Task { await reschedule() } }
                    }
                    if authDenied {
                        Text("Notifications are off in iOS Settings. Turn them on there to get the digest.").font(.footnote).foregroundStyle(.orange)
                    }
                } header: { Text("Reminders") } footer: {
                    Text("One digest a week by default. Separate reminders only for medicine, water rotation and equipment checks. Scheduled on this phone, no server.")
                }

                Section {
                    Toggle("Look up barcodes online", isOn: $onlineLookup)
                } header: { Text("Scanning") } footer: {
                    Text("When on, the barcode digits (and nothing else) are sent to Open Food Facts to fetch a product name. \(OpenFoodFactsClient.attribution)")
                }

                Section("Places") {
                    NavigationLink("Storage locations") { LocationsEditView() }
                }

                Section {
                    Button("Export supplies (CSV)") { export(ExportService.suppliesCSV(items), name: "supplies.csv") }
                    Button("Export equipment (CSV)") { export(ExportService.equipmentCSV(equipment), name: "equipment.csv") }
                    if let err = exportError { Text(err).font(.footnote).foregroundStyle(.red) }
                } header: { Text("Export") } footer: {
                    Text("Your data is yours. Print the CSV for the binder, or open it in Numbers or Excel.")
                }

                Section("About") {
                    LabeledContent("Version", value: Bundle.main.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String ?? "0.1")
                    NavigationLink("Sources and shelf-life table") { SourcesView() }
                    Text("Everything you enter stays on this phone. No account, no cloud, no analytics. Dates are guidelines from public sources and manufacturers, not medical or safety advice. For medicine, follow the printed date and your pharmacist.")
                        .font(.footnote).foregroundStyle(.secondary)
                }
            }
            .navigationTitle("Settings")
            .sheet(item: $exportURL) { url in
                ShareSheetView(url: url)
            }
        }
    }

    private func hourLabel(_ h: Int) -> String {
        var comps = DateComponents()
        comps.hour = h
        let d = Calendar.current.date(from: comps) ?? Date()
        return d.formatted(.dateTime.hour())
    }

    private func export(_ csv: String, name: String) {
        do {
            exportURL = try ExportService.writeTemp(csv, filename: name)
            exportError = nil
        } catch {
            exportError = "Could not write the file: \(error.localizedDescription)"
        }
    }

    private func toggleNotifications(_ on: Bool) async {
        if on {
            let granted = await NotificationScheduler.shared.requestAuthorization()
            authDenied = !granted
            if granted { await reschedule() } else { notificationsEnabled = false }
        } else {
            NotificationScheduler.shared.cancelAll()
        }
    }

    private func reschedule() async {
        await NotificationScheduler.shared.reschedule(items: items, tasks: tasks, digestWeekday: digestWeekday, digestHour: digestHour)
    }
}

extension URL: Identifiable {
    public var id: String { absoluteString }
}

private struct ShareSheetView: View {
    @Environment(\.dismiss) private var dismiss
    let url: URL

    var body: some View {
        NavigationStack {
            VStack(spacing: 20) {
                Image(systemName: "doc.text").font(.system(size: 40)).foregroundStyle(.secondary)
                Text(url.lastPathComponent).font(.headline)
                ShareLink(item: url) { Label("Share or save", systemImage: "square.and.arrow.up") }
                    .buttonStyle(.borderedProminent)
            }
            .padding()
            .toolbar { ToolbarItem(placement: .cancellationAction) { Button("Done") { dismiss() } } }
        }
        .presentationDetents([.medium])
    }
}

private struct SourcesView: View {
    private let library = ShelfLifeLibrary.shared

    var body: some View {
        List {
            Section("Where the numbers come from") {
                Text("USDA FoodKeeper (public domain), USDA FSIS shelf-stable and power-outage guidance, CDC emergency water guidance, Ready.gov and the American Red Cross kit lists, NFPA alarm and extinguisher intervals via state fire marshals, CPSC generator safety, and manufacturer specifications where noted. Deep-storage figures follow LDS Home Storage and BYU research and assume cool, dry, oxygen-free packaging.")
                    .font(.footnote)
                Text(OpenFoodFactsClient.attribution).font(.footnote)
            }
            Section("Shelf-life table (\(library.entries.count) entries)") {
                ForEach(library.entries.sorted { $0.name < $1.name }) { e in
                    VStack(alignment: .leading, spacing: 2) {
                        Text(e.name).font(.subheadline)
                        Text(e.summary).font(.caption).foregroundStyle(.secondary)
                        Text(e.source).font(.caption2).foregroundStyle(.tertiary)
                    }
                }
            }
        }
        .navigationTitle("Sources")
        .navigationBarTitleDisplayMode(.inline)
    }
}
