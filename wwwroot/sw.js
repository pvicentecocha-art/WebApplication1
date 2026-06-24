const CACHE_NAME = 'tiendanet-v1';
const STATIC_ASSETS = [
    '/',
    '/Tienda',
    '/css/site.css',
    '/js/site.js',
    '/manifest.json',
    '/icons/icon-192.png',
    '/icons/icon-512.png'
];

// Instalación: cachear recursos estáticos
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => cache.addAll(STATIC_ASSETS))
    );
    self.skipWaiting();
});

// Activación: limpiar caches viejos
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k)))
        )
    );
    self.clients.claim();
});

// Fetch: Network first, fallback a cache
self.addEventListener('fetch', event => {
    // Solo interceptar GET
    if (event.request.method !== 'GET') return;

    // No interceptar llamadas a la API o acciones del carrito
    const url = new URL(event.request.url);
    if (url.pathname.startsWith('/Carrito') || url.pathname.startsWith('/api')) return;

    event.respondWith(
        fetch(event.request)
            .then(response => {
                // Guardar en cache si es exitoso
                const clone = response.clone();
                caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
                return response;
            })
            .catch(() => caches.match(event.request))
    );
});