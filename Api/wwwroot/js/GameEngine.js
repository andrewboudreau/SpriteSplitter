const GameEngine = (canvas) => {
    let pause = false;
    let running = false;
    let draw = null;
    let update = null;

    const ctx = canvas.getContext("2d");

    const loop = () => {

        // compute
        update(pause);

        // draw
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        draw(ctx, pause);

        // Request the next frame
        requestAnimationFrame(loop);
    };

    // set defaults
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;

    // return game engine
    return {
        getContext: () => ctx,
        pause: () => { pause = !pause; return pause; },
        loop: (drawLoop, updateLoop) => {
            if (running) {
                alert("Already running");
                return;
            }
            running = true;
            draw = drawLoop;
            update = updateLoop;
            loop();
        }
    };
};