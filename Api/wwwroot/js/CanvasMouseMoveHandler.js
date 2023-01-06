const CanvasMouseMoveHandler = (canvas, x, y) => {

    // Get the bounding client rect of the canvas element
    const rect = canvas.getBoundingClientRect();

    const mouse = { x: 0, y: 0 };

    canvas.addEventListener('mousemove', (event) => {
        // Calculate the mouse position relative to the canvas
        mouse.x = event.clientX - rect.left;
        mouse.y = event.clientY - rect.top;
    });

    return mouse;
};

