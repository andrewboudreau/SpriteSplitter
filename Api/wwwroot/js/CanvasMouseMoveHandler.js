const CanvasMouseMoveHandler = (canvas, x, y) => {

    // Get the bounding client rect of the canvas element
    const rect = canvas.getBoundingClientRect();

    const draw = ctx => {
        ctx.font = '12px sans-serif';
        ctx.textAlign = 'center';
        ctx.fillStyle = 'white';
        ctx.fillText(`(${Math.round(mouse.x)}, ${Math.round(mouse.y)})`, mouse.x, mouse.y);

        ctx.fillStyle = "#000000";
        ctx.fillRect(mouse.x, mouse.y, 4, 4);
    }

    const mouse = {
        x: 0,
        y: 0,
        draw: draw
    };

    canvas.addEventListener('mousemove', (event) => {
        // Calculate the mouse position relative to the canvas
        mouse.x = event.clientX - rect.left;
        mouse.y = event.clientY - rect.top;
    });

    return mouse;
};

