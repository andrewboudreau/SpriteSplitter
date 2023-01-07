const CanvasMouseMoveHandler = (canvas) => {

    let mouseOverCanvas = true;

    // Get the bounding client rect of the canvas element
    const rect = canvas.getBoundingClientRect();

    const draw = (ctx, text) => {
        if (mouse.mouseOverCanvas) {
            ctx.font = '12px sans-serif';
            ctx.textAlign = 'center';
            ctx.fillStyle = 'white';
            if (!text) {
                text = () => `(${Math.round(mouse.x)}, ${Math.round(mouse.y)})`;
            }
            ctx.fillText(text(), mouse.x, mouse.y);
            
            ctx.fillStyle = "#ff0000";
            ctx.fillRect(mouse.x, mouse.y, 4, 4);
        }
    }

    const mouse = {
        x: 0,
        y: 0,
        draw: draw,
        isOnCanvas: mouseOverCanvas
    };

    canvas.addEventListener('mousemove', (event) => {
        mouse.x = event.clientX - rect.left;
        mouse.y = event.clientY - rect.top;
    });

    window.document.body.addEventListener('mouseover', (event) => {
        mouse.mouseOverCanvas = event.target.nodeName === 'CANVAS';
    });

    return mouse;
};

