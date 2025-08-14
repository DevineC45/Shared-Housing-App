/* homigo sw v4 */
const STATIC_CACHE = "homigo-static-v4";

const STATIC_ASSETS = [
  "/",                // cache app root
  "/?source=pwa",     // cache start_url
  "/offline.html",
  "/css/site.css",
  "/lib/bootstrap/dist/css/bootstrap.min.css",
  "/lib/bootstrap/dist/js/bootstrap.bundle.min.js",
  "/lib/jquery/dist/jquery.min.js",
  "/img/Homigo.png",
  "/icons/icon-192.png",
  "/icons/icon-512.png"
];

self.addEventListener("install", (event) => {
  event.waitUntil((async () => {
    const cache = await caches.open(STATIC_CACHE);
    await cache.addAll(STATIC_ASSETS);
  })());
  self.skipWaiting();
});

self.addEventListener("activate", (event) => {
  event.waitUntil((async () => {
    // clean old caches
    const keys = await caches.keys();
    await Promise.all(keys.filter(k => k !== STATIC_CACHE).map(k => caches.delete(k)));
    // (optional) faster navigations when online
    if (self.registration.navigationPreload) {
      await self.registration.navigationPreload.enable();
    }
  })());
  self.clients.claim();
});

self.addEventListener("fetch", (event) => {
  if (event.request.method !== "GET") return;

  const url = new URL(event.request.url);
  if (url.origin !== location.origin) return;           // ignore cross-origin (CDNs)
  if (url.pathname.startsWith("/Users")) return;        // don't cache auth routes

  // Handle top-level navigations/documents
  if (event.request.mode === "navigate" || event.request.destination === "document") {
    event.respondWith((async () => {
      try {
        // Use any preloaded response (if enabled)
        const preload = await event.preloadResponse;
        if (preload) return preload;

        // Network first
        const network = await fetch(event.request);
        const cache = await caches.open(STATIC_CACHE);
        cache.put(event.request, network.clone());
        return network;
      } catch {
        // Offline: try a cached page ignoring query strings, else offline fallback
        const cached = await caches.match(event.request, { ignoreSearch: true });
        return cached || caches.match("/offline.html");
      }
    })());
    return;
  }

  // Static assets: stale-while-revalidate
  event.respondWith((async () => {
    const cached = await caches.match(event.request);
    const fetchPromise = fetch(event.request)
      .then(async (res) => {
        const cache = await caches.open(STATIC_CACHE);
        cache.put(event.request, res.clone());
        return res;
      })
      .catch(() => cached);
    return cached || fetchPromise;
  })());
});
