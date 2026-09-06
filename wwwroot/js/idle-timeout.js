(() => {
    const IDLE_LIMIT_MS = 2 * 60 * 1000;
    const PING_EVERY_MS = 45 * 1000;
    
    let idleTimer;
    let lastPing = 0;

    function scheduleLogout() {
        clearTimeout(idleTimer);
        idleTimer = setTimeout(() => {
            // 🚨 AQUÍ ESTÁ LA MAGIA IMPOSTORA 🚨
            // Redirigimos directo a nuestra ruta especial en el controlador
            window.location.href = '/Cuenta/LogoutInactividad';
        }, IDLE_LIMIT_MS);
    }

    async function registerActivity() {
        scheduleLogout();
        const now = Date.now();
        
        if (now - lastPing >= PING_EVERY_MS) {
            lastPing = now;
            try {
                await fetch('/Cuenta/Ping', {
                    method: 'GET',
                    credentials: 'same-origin',
                    cache: 'no-store'
                });
            } catch {
                // Si el servidor no responde, la próxima navegación
                // comprobará el estado real de la autenticación.
            }
        }
    }

    // Eventos que reinician el contador de inactividad
    ['click', 'keydown', 'mousemove', 'scroll', 'touchstart'].forEach(eventName =>
        document.addEventListener(eventName, registerActivity, { passive: true })
    );

    scheduleLogout();
})();