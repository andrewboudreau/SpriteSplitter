const GameEngine = (canvas) => {
    let pause = false;
    let running = false;
    let game = null;

    const ctx = canvas.getContext("2d");

    const loop = () => {
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        game(ctx, pause);

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
        loop: userloop => {
            if (running) {
                alert("Already running");
                return;
            }
            running = true;
            game = userloop;
            loop();
        }
    };
};