import Cocoa
import WebKit

class AppDelegate: NSObject, NSApplicationDelegate, NSWindowDelegate {
    var window: NSWindow!
    var webView: WKWebView!

    func applicationDidFinishLaunching(_ notification: Notification) {
        NSApp.setActivationPolicy(.regular)

        let screenRect = NSScreen.main?.visibleFrame ?? NSRect(x: 0, y: 0, width: 1280, height: 800)
        let windowWidth: CGFloat = min(1280, screenRect.width - 80)
        let windowHeight: CGFloat = min(800, screenRect.height - 80)
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
        window.titlebarAppearsTransparent = true
        window.titleVisibility = .visible
        window.backgroundColor = NSColor(red: 0.05, green: 0.05, blue: 0.08, alpha: 1.0)
        window.isReleasedWhenClosed = false
        window.delegate = self
        window.minSize = NSSize(width: 960, height: 600)

        // WebKit Configuration
        let config = WKWebViewConfiguration()
        let prefs = WKWebpagePreferences()
        prefs.allowsContentJavaScript = true
        config.defaultWebpagePreferences = prefs
        config.preferences.setValue(true, forKey: "developerExtrasEnabled")

        webView = WKWebView(frame: window.contentView!.bounds, configuration: config)
        webView.autoresizingMask = [.width, .height]
        webView.setValue(false, forKey: "drawsBackground")

        window.contentView?.addSubview(webView)

        // Determine HTML Path
        let appBundlePath = Bundle.main.bundlePath
        let resourceHtml = Bundle.main.path(forResource: "index", ofType: "html")
        let fallbackPath = (appBundlePath as NSString).deletingLastPathComponent + "/../../public/index.html"
        let projectPath = "/Users/umut/mycodespaces/moon-sweepers/public/index.html"

        var targetUrl: URL?
        if let path = resourceHtml, FileManager.default.fileExists(atPath: path) {
            targetUrl = URL(fileURLWithPath: path)
        } else if FileManager.default.fileExists(atPath: fallbackPath) {
            targetUrl = URL(fileURLWithPath: (fallbackPath as NSString).standardizingPath)
        } else if FileManager.default.fileExists(atPath: projectPath) {
            targetUrl = URL(fileURLWithPath: projectPath)
        }

        if let url = targetUrl {
            let directory = url.deletingLastPathComponent()
            webView.loadFileURL(url, allowingReadAccessTo: directory)
        } else {
            let errorHtml = "<html><body style='background:#111;color:#fff;font-family:sans-serif;padding:40px;'><h2>11Hopem Moon Sweepers</h2><p>Game resources loading...</p></body></html>"
            webView.loadHTMLString(errorHtml, baseURL: nil)
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

        // App Menu
        let appMenuItem = NSMenuItem()
        mainMenu.addItem(appMenuItem)
        let appMenu = NSMenu()
        appMenuItem.submenu = appMenu
        appMenu.addItem(withTitle: "About 11Hopem Moon Sweepers", action: #selector(showAbout), keyEquivalent: "")
        appMenu.addItem(NSMenuItem.separator())
        appMenu.addItem(withTitle: "Quit Moon Sweepers", action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q")

        // View Menu
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
