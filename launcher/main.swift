import Cocoa
import WebKit

class AppDelegate: NSObject, NSApplicationDelegate, NSWindowDelegate {
    var window: NSWindow!
    var webView: WKWebView!

    func applicationDidFinishLaunching(_ notification: Notification) {
        NSApp.setActivationPolicy(.regular)

        let screenRect = NSScreen.main?.visibleFrame ?? NSRect(x: 0, y: 0, width: 1280, height: 800)
        let windowWidth: CGFloat = min(1280, screenRect.width - 60)
        let windowHeight: CGFloat = min(800, screenRect.height - 60)
        let windowRect = NSRect(
            x: screenRect.midX - windowWidth / 2,
            y: screenRect.midY - windowHeight / 2,
            width: windowWidth,
            height: windowHeight
        )

        window = NSWindow(
            contentRect: windowRect,
            styleMask: [.titled, .closable, .miniaturizable, .resizable, .fullSizeContentView],
            backing: .buffered,
            defer: false
        )

        window.title = "11Hopem Moon Sweepers"
        window.titlebarAppearsTransparent = false
        window.titleVisibility = .visible
        window.backgroundColor = NSColor(red: 0.03, green: 0.04, blue: 0.08, alpha: 1.0)
        window.isReleasedWhenClosed = false
        window.delegate = self
        window.minSize = NSSize(width: 900, height: 560)

        // WebKit Configuration
        let config = WKWebViewConfiguration()
        let prefs = WKWebpagePreferences()
        prefs.allowsContentJavaScript = true
        config.defaultWebpagePreferences = prefs
        config.preferences.setValue(true, forKey: "developerExtrasEnabled")
        config.preferences.setValue(true, forKey: "allowFileAccessFromFileURLs")
        config.setValue(true, forKey: "allowUniversalAccessFromFileURLs")

        webView = WKWebView(frame: window.contentView!.bounds, configuration: config)
        webView.autoresizingMask = [.width, .height]

        window.contentView?.addSubview(webView)

        // Locate index.html
        var htmlPath: String? = nil
        let bundlePath = Bundle.main.path(forResource: "index", ofType: "html")
        let localPath = "/Users/umut/mycodespaces/moon-sweepers/public/index.html"

        if let bp = bundlePath, FileManager.default.fileExists(atPath: bp) {
            htmlPath = bp
        } else if FileManager.default.fileExists(atPath: localPath) {
            htmlPath = localPath
        }

        if let validPath = htmlPath {
            do {
                let htmlString = try String(contentsOfFile: validPath, encoding: .utf8)
                let baseURL = URL(fileURLWithPath: (validPath as NSString).deletingLastPathComponent)
                webView.loadHTMLString(htmlString, baseURL: baseURL)
                print("[11Hopem] Loaded game HTML from: \(validPath)")
            } catch {
                let url = URL(fileURLWithPath: validPath)
                webView.loadFileURL(url, allowingReadAccessTo: url.deletingLastPathComponent())
            }
        } else {
            let fallbackHtml = """
            <html><body style='background:#03050B;color:#00F0FF;display:flex;align-items:center;justify-content:center;height:100vh;font-family:sans-serif;'>
            <h2>11Hopem Moon Sweepers</h2>
            </body></html>
            """
            webView.loadHTMLString(fallbackHtml, baseURL: nil)
        }

        setupMenuBar()

        window.makeKeyAndOrderFront(nil)
        NSApp.activate(ignoringOtherApps: true)
    }

    func windowShouldClose(_ sender: NSWindow) -> Bool {
        NSApp.terminate(nil)
        return true
    }

    private func setupMenuBar() {
        let mainMenu = NSMenu()
        let appMenuItem = NSMenuItem()
        mainMenu.addItem(appMenuItem)
        let appMenu = NSMenu()
        appMenuItem.submenu = appMenu
        appMenu.addItem(withTitle: "About 11Hopem Moon Sweepers", action: #selector(showAbout), keyEquivalent: "")
        appMenu.addItem(NSMenuItem.separator())
        appMenu.addItem(withTitle: "Quit Moon Sweepers", action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q")

        let viewMenuItem = NSMenuItem()
        mainMenu.addItem(viewMenuItem)
        let viewMenu = NSMenu(title: "View")
        viewMenuItem.submenu = viewMenu
        viewMenu.addItem(withTitle: "Reload Game", action: #selector(reloadGame), keyEquivalent: "r")
        viewMenu.addItem(withTitle: "Toggle Full Screen", action: #selector(toggleFullScreen), keyEquivalent: "f")

        NSApp.mainMenu = mainMenu
    }

    @objc func showAbout() {
        if let url = URL(string: "https://11hopem.com") {
            NSWorkspace.shared.open(url)
        }
    }

    @objc func reloadGame() {
        webView.reload()
    }

    @objc func toggleFullScreen() {
        window.toggleFullScreen(nil)
    }
}

let app = NSApplication.shared
let delegate = AppDelegate()
app.delegate = delegate
app.run()
