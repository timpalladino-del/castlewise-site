import SwiftUI
import SwiftData

@main
struct ReadinessApp: App {
    let container: ModelContainer

    init() {
        do {
            container = try AppSchema.container()
        } catch {
            fatalError("Could not create the data store: \(error)")
        }
    }

    var body: some Scene {
        WindowGroup {
            RootView()
        }
        .modelContainer(container)
    }
}
