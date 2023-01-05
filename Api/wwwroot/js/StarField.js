const StarField = canvas => {
    // Create an array to store the stars
    const stars = [];

    // Create a random number of stars
    for (let i = 0; i < 100; i++) {
        stars.push({
            x: Math.random() * canvas.width,
            y: Math.random() * canvas.height,
            radius: Math.random() * 2 + 1,
            alpha: Math.random(),
            color: '#fff',
        });
    }
    
    // Update the stars
    const update = (after) => {
        for (const star of stars) {
            star.x += 2 * 1 / star.alpha + 1;

            // If the star goes off the bottom of the screen, wrap it around to the top
            // and give it a new random color
            if (star.x > canvas.width) {
                star.x = 0;
                star.y = Math.random() * canvas.height;
                star.radius = Math.random() * 2 + 1;
                star.alpha = Math.random();
            }

            if (after) {
                after(star, canvas, stars);
            }
        }
    };

    // Draw the stars on a canvas
    const draw = ctx => {
        for (const star of stars) {
            // Draw the star
            ctx.beginPath();
            ctx.arc(star.x, star.y, star.radius, 0, Math.PI * 2);
            ctx.closePath();
            ctx.fillStyle = `rgba(255, 255, 255, ${star.alpha})`;
            ctx.fill();
        }
    };

    // Return the public API
    return {
        update,
        draw
    };
};

const random2d = (minX = -256, maxX = 256, minY = -256, maxY = 256) => {
    const x = Math.random() * (maxX - minX) + minX;
    const y = Math.random() * (maxY - minY) + minY;
    return { x, y };
};