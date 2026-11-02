import SwiftUI
import PhotosUI
import UIKit

/// Attach one photo from the library. Stored on-device as external data.
struct PhotoPickerButton: View {
    @Binding var photoData: Data?
    @State private var selection: PhotosPickerItem?

    var body: some View {
        HStack {
            if let data = photoData, let image = UIImage(data: data) {
                Image(uiImage: image)
                    .resizable()
                    .scaledToFill()
                    .frame(width: 56, height: 56)
                    .clipShape(RoundedRectangle(cornerRadius: 10))
                Spacer()
                Button("Remove", role: .destructive) { photoData = nil }
            } else {
                PhotosPicker(selection: $selection, matching: .images) {
                    Label("Add a photo", systemImage: "photo")
                }
            }
        }
        .onChange(of: selection) { _, newValue in
            guard let newValue else { return }
            Task {
                if let data = try? await newValue.loadTransferable(type: Data.self),
                   let image = UIImage(data: data),
                   let small = image.jpegData(compressionQuality: 0.6) {
                    photoData = small
                }
            }
        }
    }
}
