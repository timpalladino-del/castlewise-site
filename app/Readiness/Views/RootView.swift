import SwiftUI
import SwiftData

struct RootView: View {
    @Environment(\.modelContext) private var context
    @Environment(\.scenePhase) private var scenePhase
    @Query private var items: [SupplyItem]
    @Query private var tasks: [MaintenanceTask]

    @AppStorage(AppSettings.notificationsEnabled) private var notificationsEnabled = false
    @AppStorage(AppSettings.digestWeekday) private var digestWeekday = 1
    @AppStorage(AppSettings.digestHour) private var digestHour = 9

    var body: some View {
        TabView {
            HomeView()
                .tabItem { Label("Home", systemImage: "house.fill") }
            SuppliesListView()
                .tabItem { Label("Supplies", systemImage: "shippingbox.fill") }
            EquipmentListView()
                .tabItem { Label("Equipment", systemImage: "wrench.and.screwdriver.fill") }
            ReadinessView()
                .tabItem { Label("Ready", systemImage: "checkmark.shield.fill") }
            SettingsView()
                .tabItem { Label("Settings", systemImage: "gearshape.fill") }
        }
        .task {
            SeedData.ensureDefaults(in: context)
        }
        .onChange(of: scenePhase) { _, phase in
            guard phase == .background, notificationsEnabled else { return }
            let snapshotItems = items
            let snapshotTasks = tasks
            Task { @MainActor in
                await NotificationScheduler.shared.reschedule(items: snapshotItems, tasks: snapshotTasks,
                                                               digestWeekday: digestWeekday, digestHour: digestHour)
            }
        }
    }
}
