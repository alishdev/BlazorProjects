let debugConsole = null;

window.initializeDebugConsole = (instance) => {
    debugConsole = instance;
    
    // Store original console methods
    const originalConsole = {
        log: console.log,
        error: console.error,
        warn: console.warn,
        info: console.info
    };

    // Override console methods
    console.log = (...args) => {
        const message = args.map(arg => typeof arg === 'object' ? JSON.stringify(arg, null, 2) : arg.toString()).join(' ');
        debugConsole.invokeMethodAsync('AddLog', message, 'info');
        originalConsole.log.apply(console, args);
    };

    console.error = (...args) => {
        const message = args.map(arg => typeof arg === 'object' ? JSON.stringify(arg, null, 2) : arg.toString()).join(' ');
        debugConsole.invokeMethodAsync('AddLog', message, 'error');
        originalConsole.error.apply(console, args);
    };

    console.warn = (...args) => {
        const message = args.map(arg => typeof arg === 'object' ? JSON.stringify(arg, null, 2) : arg.toString()).join(' ');
        debugConsole.invokeMethodAsync('AddLog', message, 'warn');
        originalConsole.warn.apply(console, args);
    };

    console.info = (...args) => {
        const message = args.map(arg => typeof arg === 'object' ? JSON.stringify(arg, null, 2) : arg.toString()).join(' ');
        debugConsole.invokeMethodAsync('AddLog', message, 'info');
        originalConsole.info.apply(console, args);
    };
};

window.disposeDebugConsole = () => {
    debugConsole = null;
};

window.scrollDebugToBottom = (element) => {
    element.scrollTop = element.scrollHeight;
}; 