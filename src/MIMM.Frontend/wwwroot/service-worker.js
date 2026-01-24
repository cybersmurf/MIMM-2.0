self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', () => clients.claim());
// No fetch handler to avoid no-op overhead warnings in dev
