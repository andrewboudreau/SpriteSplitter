const CanvasResizeHandler = canvas => {
    let resizeTimeout;

    const handleResize = () => {
        // Clear the resize timeout
        clearTimeout(resizeTimeout);

        // Set a new resize timeout
        resizeTimeout = setTimeout(() => {
            // Update the canvas size
            canvas.width = window.innerWidth;
            canvas.height = window.innerHeight;
        }, 250);
    };

    // Add the resize event listener
    window.addEventListener('resize', handleResize);
};

/*
// Example usage:

const canvas = document.getElementById('canvas');
const resizeHandler = CanvasResizeHandler(canvas);

In this code, the CanvasResizeHandler function is a JavaScript module that accepts a canvas element as an argument.
It creates a handleResize() function that updates the canvas size and debounces the resize event using a 250ms timeout.
The handleResize() function is added as an event listener for the resize event on the window.

To use the module, you can pass a canvas element to the CanvasResizeHandler() function and store the returned value in a variable.
This will create a new CanvasResizeHandler instance that is bound to the canvas element and starts listening for the resize event.

*/


